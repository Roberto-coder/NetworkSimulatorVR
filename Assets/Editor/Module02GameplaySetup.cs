#if UNITY_EDITOR
using System;
using System.IO;
using Framework.Interaction.Tools;
using GameData.Achievements;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module02_RackInstallation;
using Modules.Module02_RackInstallation.Presentation;
using Presentacion.GlobalUI.ObjectivesWristMenu;
using Presentacion.GlobalUI.RadialSelectorTool;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class Module02GameplaySetup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";
    private const string DataFolder = "Assets/GameData/Modules/Module02";
    private const string ObjectivesFolder = "Assets/GameData/Objectives/Module02";
    private const string DefinitionPath = DataFolder + "/Module02_RackInstallation.asset";
    private const string RadialPrefab = "Assets/Prefabs/PlayerUi/RadialSelection.prefab";
    private const string ObjectivesPrefab = "Assets/Prefabs/UI_Components/Objectives/ObjectivesCanva.prefab";

    private static readonly (string id, string title, string description)[] ObjectiveSpecs =
    {
        ("inspect_rack", "Explora el rack", "Identifica la estructura, las unidades U y los componentes instalados mediante sus tarjetas informativas."),
        ("inspect_switch", "Conoce el switch", "Revisa el cuerpo, alimentación, puertos y LEDs del switch educativo de 12 puertos."),
        ("mount_switch", "Inserta el switch", "Sujeta el switch con ambas manos y deslízalo hasta la unidad indicada del rack."),
        ("secure_switch", "Asegura el switch", "Coloca y ajusta los tornillos de montaje en los puntos indicados."),
        ("connect_patch_ports", "Conecta los puertos", "Conecta dos patch cords entre el patch panel y los puertos indicados del switch."),
        ("connect_console", "Conecta la consola", "Prepara la conexión de administración necesaria para configurar el equipo."),
        ("power_on_switch", "Enciende el switch", "Conecta la alimentación y acciona el interruptor del dispositivo."),
        ("configure_access_ports", "Habilita las interfaces", "Representa la configuración y habilitación de los puertos de acceso requeridos."),
        ("verify_installation", "Verifica la instalación", "Comprueba montaje, cableado, estado de los enlaces y nomenclatura final.")
    };

    [MenuItem("Network Simulator/Module 02/Configurar objetivos y rueda XRI")]
    public static void Configure()
    {
        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("No se encontró la escena Modulo2.", ScenePath);

        EnsureFolder(DataFolder);
        EnsureFolder(ObjectivesFolder);
        ModuleDefinition definition = CreateOrUpdateDefinition();
        AssetDatabase.SaveAssets();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = FindOrCreate("_Module02");
        GameObject managers = FindOrCreate("Managers", root.transform);

        Module02Manager manager = GetOrAdd<Module02Manager>(managers);
        SetObjectReference(manager, "moduleDefinition", definition);

        ToolManager tools = GetOrAdd<ToolManager>(managers);
        Transform rightController = FindTransform("Right Controller");
        SetObjectReference(tools, "rightHandAnchor", rightController);

        Module02PresentationController presentation = GetOrAdd<Module02PresentationController>(managers);
        SetObjectReference(presentation, "toolManager", tools);

        GameObject ui = FindOrCreate("UI_XRI", root.transform);
        ConfigureRadialMenu(ui.transform, tools);
        ConfigureObjectivesMenu(ui.transform);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Módulo 2 configurado: 9 objetivos, menú de muñeca y rueda XRI. Completa prefabs e iconos de herramientas en Module02_RackInstallation.asset.");
    }

    private static ModuleDefinition CreateOrUpdateDefinition()
    {
        ModuleDefinition definition = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(DefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<ModuleDefinition>();
            AssetDatabase.CreateAsset(definition, DefinitionPath);
        }

        SerializedObject serialized = new(definition);
        serialized.FindProperty("moduleId").stringValue = "module_02_rack_installation";
        serialized.FindProperty("moduleName").stringValue = "Instalación de dispositivos en rack";
        serialized.FindProperty("description").stringValue =
            "Exploración e instalación guiada de un switch educativo en un rack de comunicaciones.";

        SerializedProperty objectives = serialized.FindProperty("objectives");
        objectives.arraySize = ObjectiveSpecs.Length;
        for (int i = 0; i < ObjectiveSpecs.Length; i++)
            objectives.GetArrayElementAtIndex(i).objectReferenceValue = CreateOrUpdateObjective(i);

        SerializedProperty tools = serialized.FindProperty("availableTools");
        tools.arraySize = 3;
        SetTool(tools.GetArrayElementAtIndex(0), "Destornillador");
        SetTool(tools.GetArrayElementAtIndex(1), "Cable de consola");
        SetTool(tools.GetArrayElementAtIndex(2), "Patch cord Ethernet");

        AchievementDefinition achievement = AssetDatabase.LoadAssetAtPath<AchievementDefinition>(
            "Assets/GameData/Achievements/Module02Completion.asset");
        serialized.FindProperty("completionAchievement").objectReferenceValue = achievement;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ObjectiveData CreateOrUpdateObjective(int index)
    {
        var spec = ObjectiveSpecs[index];
        string path = $"{ObjectivesFolder}/{index + 1:00}_{spec.id}.asset";
        ObjectiveData data = AssetDatabase.LoadAssetAtPath<ObjectiveData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<ObjectiveData>();
            AssetDatabase.CreateAsset(data, path);
        }
        SerializedObject serialized = new(data);
        serialized.FindProperty("id").stringValue = spec.id;
        serialized.FindProperty("title").stringValue = spec.title;
        serialized.FindProperty("description").stringValue = spec.description;
        serialized.FindProperty("introductionDialogue").stringValue = string.Empty;
        serialized.FindProperty("reminderDialogue").stringValue = string.Empty;
        serialized.FindProperty("reminderInterval").floatValue = 20f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }

    private static void SetTool(SerializedProperty tool, string name)
    {
        tool.FindPropertyRelative("name").stringValue = name;
        tool.FindPropertyRelative("prefab").objectReferenceValue = null;
        tool.FindPropertyRelative("icon").objectReferenceValue = null;
    }

    private static void ConfigureRadialMenu(Transform parent, ToolManager tools)
    {
        GameObject existing = GameObject.Find("RadialMenu_XRI");
        GameObject radial = existing;
        if (radial == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RadialPrefab);
            radial = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            radial.name = "RadialMenu_XRI";
            radial.transform.SetParent(parent, false);
        }
        StripOculusComponents(radial);
        ConfigureXriCanvases(radial);
        RadialMenuController controller = radial.GetComponent<RadialMenuController>();
        controller.toolManager = tools;
        controller.playerCamera = Camera.main;
        GetOrAdd<XriRadialMenuInput>(radial);
    }

    private static void ConfigureObjectivesMenu(Transform parent)
    {
        if (GameObject.Find("ObjectivesWrist_XRI") != null)
            return;

        Transform leftController = FindTransform("Left Controller");
        if (leftController == null)
        {
            Debug.LogWarning("No se encontró Left Controller; se creó el menú bajo UI_XRI para asignarlo después.");
            leftController = parent;
        }

        GameObject anchor = new("ObjectivesWrist_XRI");
        anchor.transform.SetParent(leftController, false);
        anchor.transform.localPosition = new Vector3(0.02f, 0.08f, 0.04f);
        anchor.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ObjectivesPrefab);
        GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        canvas.name = "ObjectivesCanvas_XRI";
        canvas.transform.SetParent(anchor.transform, false);
        StripOculusComponents(canvas);
        ConfigureXriCanvases(canvas);

        WristMenuController embedded = canvas.GetComponent<WristMenuController>();
        if (embedded != null)
            UnityEngine.Object.DestroyImmediate(embedded);
        WristMenuController controller = anchor.AddComponent<WristMenuController>();
        SetObjectReference(controller, "headTransform", Camera.main != null ? Camera.main.transform : null);
        SetObjectReference(controller, "wristTransform", anchor.transform);
        SetObjectReference(controller, "wristMenuCanvas", canvas);
    }

    private static void ConfigureXriCanvases(GameObject root)
    {
        foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
        {
            GraphicRaycaster old = canvas.GetComponent<GraphicRaycaster>();
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }

    private static void StripOculusComponents(GameObject root)
    {
        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null || component is Transform)
                continue;
            string ns = component.GetType().Namespace ?? string.Empty;
            if (ns.StartsWith("Oculus.Interaction", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(component);
        }
    }

    private static Transform FindTransform(string exactName)
    {
        foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (transform.name == exactName && transform.gameObject.activeInHierarchy)
                return transform;
        return null;
    }

    private static GameObject FindOrCreate(string name, Transform parent = null)
    {
        Transform existing = parent != null ? parent.Find(name) : GameObject.Find(name)?.transform;
        if (existing != null) return existing.gameObject;
        GameObject created = new(name);
        if (parent != null) created.transform.SetParent(parent, false);
        return created;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.TryGetComponent(out T component) ? component : target.AddComponent<T>();

    private static void SetObjectReference(UnityEngine.Object target, string property, UnityEngine.Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(normalized)) return;
        string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(normalized));
    }
}
#endif
