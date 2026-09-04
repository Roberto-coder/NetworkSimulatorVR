#if UNITY_EDITOR
using Core.Module;
using GameData.Modules;
using GameData.Objectives;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Genera el esqueleto no destructivo de un módulo nuevo. A diferencia del
/// setup del Lobby, no presupone nombres, prefabs ni mecánicas concretas.
/// </summary>
public sealed class ModuleSceneSetup : EditorWindow
{
    private string moduleId = "module_02";
    private string moduleName = "Módulo 02";
    private string description = "";
    private string dataFolder = "Assets/GameData/Modules/Module02";
    private int objectiveCount = 1;
    private bool createSceneHierarchy = true;

    [MenuItem("Network Simulator/Crear base de módulo")]
    private static void Open() => GetWindow<ModuleSceneSetup>("Nuevo módulo");

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Crea assets y contenedores vacíos. Después completa cada referencia " +
            "desde ModuleSceneConfiguration en el Inspector.", MessageType.Info);
        moduleId = EditorGUILayout.TextField("ID", moduleId);
        moduleName = EditorGUILayout.TextField("Nombre", moduleName);
        description = EditorGUILayout.TextField("Descripción", description);
        dataFolder = EditorGUILayout.TextField("Carpeta de datos", dataFolder);
        objectiveCount = EditorGUILayout.IntSlider("Objetivos iniciales", objectiveCount, 0, 20);
        createSceneHierarchy = EditorGUILayout.Toggle("Crear jerarquía en escena", createSceneHierarchy);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(moduleId) ||
                                           string.IsNullOrWhiteSpace(dataFolder)))
        {
            if (GUILayout.Button("Crear punto de partida"))
                CreateScaffold();
        }
    }

    private void CreateScaffold()
    {
        EnsureFolder(dataFolder);
        string safeId = Sanitize(moduleId);
        string modulePath = $"{dataFolder}/{safeId}_ModuleDefinition.asset";
        string tutorialPath = $"{dataFolder}/{safeId}_TutorialPlan.asset";

        ModuleDefinition module = LoadOrCreate<ModuleDefinition>(modulePath);
        TutorialPlan tutorial = LoadOrCreate<TutorialPlan>(tutorialPath);
        SerializedObject moduleData = new(module);
        moduleData.FindProperty("moduleId").stringValue = moduleId.Trim();
        moduleData.FindProperty("moduleName").stringValue = moduleName.Trim();
        moduleData.FindProperty("description").stringValue = description;
        SerializedProperty objectives = moduleData.FindProperty("objectives");

        // Sólo agrega espacios faltantes; nunca reemplaza objetivos ya configurados.
        int existing = objectives.arraySize;
        if (existing < objectiveCount)
            objectives.arraySize = objectiveCount;
        for (int i = existing; i < objectiveCount; i++)
        {
            string objectivePath = $"{dataFolder}/{safeId}_Objective_{i + 1:00}.asset";
            ObjectiveData objective = LoadOrCreate<ObjectiveData>(objectivePath);
            objectives.GetArrayElementAtIndex(i).objectReferenceValue = objective;
        }
        moduleData.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(module);

        if (createSceneHierarchy)
            CreateHierarchy(module, tutorial);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = module;
        Debug.Log($"Base del módulo '{moduleName}' creada en {dataFolder}.");
    }

    private static void CreateHierarchy(ModuleDefinition module, TutorialPlan tutorial)
    {
        GameObject root = GameObject.Find("_Module") ?? new GameObject("_Module");
        ModuleSceneConfiguration configuration =
            root.GetComponent<ModuleSceneConfiguration>() ?? root.AddComponent<ModuleSceneConfiguration>();
        SetReference(configuration, "moduleDefinition", module);
        SetReference(configuration, "tutorialPlan", tutorial);

        EnsureChild(root.transform, "Managers");
        EnsureChild(root.transform, "Tutorial");
        EnsureChild(root.transform, "NPC");
        EnsureChild(root.transform, "Objectives");
        EnsureChild(root.transform, "Gameplay");
        EnsureChild(root.transform, "UI");
        EnsureChild(root.transform, "Waypoints");
        EnsureChild(root.transform, "SpawnPoints");

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureChild(Transform parent, string name)
    {
        if (parent.Find(name) != null)
            return;
        GameObject child = new(name);
        Undo.RegisterCreatedObjectUndo(child, "Crear base de módulo");
        child.transform.SetParent(parent, false);
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;
        asset = CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        path = path.Replace('\\', '/').TrimEnd('/');
        if (!path.StartsWith("Assets/") || path.Contains(".."))
            throw new System.ArgumentException("La carpeta debe estar dentro de Assets.");

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static string Sanitize(string value)
    {
        foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');
        return value.Trim().Replace(' ', '_');
    }
}
#endif
