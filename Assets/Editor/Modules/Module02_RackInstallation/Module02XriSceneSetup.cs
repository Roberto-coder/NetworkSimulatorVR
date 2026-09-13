#if UNITY_EDITOR
using System.IO;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Instala en SampleScene una base XRI 3.x tomada de los Starter Assets.
/// Mantiene desactivado el rig experimental anterior para poder comparar o
/// recuperar referencias sin mezclar ambos sistemas durante Play Mode.
/// </summary>
public static class Module02XriSceneSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string StarterRigPath =
        "Assets/Samples/XR Interaction Toolkit/3.2.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
    private const string HandsRigPath =
        "Assets/Samples/XR Interaction Toolkit/3.2.2/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig).prefab";
    private const string PlayerFolder = "Assets/Prefabs/Player/XRI";
    private const string PlayerPrefabPath = PlayerFolder + "/PlayerVR_XRI.prefab";
    private const string HandsPlayerPrefabPath = PlayerFolder + "/PlayerVR_XRI_Hands.prefab";
    private const string Module02ScenePath = "Assets/Scenes/Modulo2.unity";

    [MenuItem("Network Simulator/Module 02/Configurar SampleScene con XRI")]
    public static void Configure()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureFolder(PlayerFolder);

        GameObject playerPrefab = CreatePlayerVariant();
        DisableLegacyRig();
        GameObject player = EnsurePlayerInstance(playerPrefab, scene);
        EnsureInteractionManager();
        EnsureEventSystem();

        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log(
            "SampleScene configurada con PlayerVR_XRI. El PlayerVR anterior quedó " +
            "desactivado como respaldo. Prueba primero tracking, ray, UI y snap turn.");
    }

    [MenuItem("Network Simulator/Module 02/Renombrar escena y activar Hand Tracking")]
    public static void EnableHandsAndRenameScene()
    {
        string sourceScene = File.Exists(Module02ScenePath) ? Module02ScenePath : ScenePath;
        Scene scene = EditorSceneManager.OpenScene(sourceScene, OpenSceneMode.Single);
        EnsureFolder(PlayerFolder);

        GameObject handsPrefab = CreateLocalRigCopy(
            HandsRigPath, HandsPlayerPrefabPath, "PlayerVR_XRI_Hands");

        DisableExistingPlayerForHandsMigration();
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(handsPrefab, scene);
        player.name = "PlayerVR_XRI";
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Undo.RegisterCreatedObjectUndo(player, "Crear PlayerVR_XRI con manos");

        EnsureInteractionManager();
        EnsureEventSystem();
        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (sourceScene != Module02ScenePath)
        {
            string error = AssetDatabase.MoveAsset(sourceScene, Module02ScenePath);
            if (!string.IsNullOrEmpty(error))
                throw new IOException("No se pudo renombrar SampleScene: " + error);
            UpdateBuildSettingsScene(sourceScene, Module02ScenePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Module02ScenePath, OpenSceneMode.Single);
        Debug.Log(
            "Modulo2 configurado con controladores y hand tracking. Abre Project " +
            "Validation, corrige los avisos de OpenXR y prueba dejando los controles.");
    }

    private static GameObject CreatePlayerVariant()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (existing != null)
            return existing;

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(StarterRigPath);
        if (source == null)
            throw new FileNotFoundException(
                "No se encontró el prefab XR Origin de Starter Assets.", StarterRigPath);

        string copiedPath = AssetDatabase.GenerateUniqueAssetPath(PlayerPrefabPath);
        if (!AssetDatabase.CopyAsset(StarterRigPath, copiedPath))
            throw new IOException("No se pudo crear la variante local de PlayerVR_XRI.");

        GameObject copy = AssetDatabase.LoadAssetAtPath<GameObject>(copiedPath);
        using (PrefabUtility.EditPrefabContentsScope scope =
               new PrefabUtility.EditPrefabContentsScope(copiedPath))
        {
            scope.prefabContentsRoot.name = "PlayerVR_XRI";
        }
        return copy;
    }

    private static GameObject CreateLocalRigCopy(
        string sourcePath, string destinationPath, string rootName)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(destinationPath);
        if (existing != null)
            return existing;

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (source == null)
            throw new FileNotFoundException("No se encontró el rig oficial de XRI.", sourcePath);
        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
            throw new IOException("No se pudo crear " + destinationPath);

        using (PrefabUtility.EditPrefabContentsScope scope =
               new PrefabUtility.EditPrefabContentsScope(destinationPath))
        {
            scope.prefabContentsRoot.name = rootName;
        }
        return AssetDatabase.LoadAssetAtPath<GameObject>(destinationPath);
    }

    private static void DisableExistingPlayerForHandsMigration()
    {
        XROrigin[] origins =
            Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (XROrigin origin in origins)
        {
            if (!origin.gameObject.activeSelf)
                continue;
            Undo.RecordObject(origin.gameObject, "Desactivar rig XRI sin manos");
            origin.gameObject.name = "PlayerVR_XRI_ControllerBackup";
            origin.gameObject.SetActive(false);
        }
    }

    private static void UpdateBuildSettingsScene(string oldPath, string newPath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        bool changed = false;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path != oldPath)
                continue;
            scenes[i] = new EditorBuildSettingsScene(newPath, scenes[i].enabled);
            changed = true;
        }
        if (changed)
            EditorBuildSettings.scenes = scenes;
    }

    private static void DisableLegacyRig()
    {
        GameObject legacy = GameObject.Find("PlayerVR");
        if (legacy == null || legacy.name == "PlayerVR_XRI")
            return;

        Undo.RecordObject(legacy, "Desactivar PlayerVR experimental");
        legacy.name = "PlayerVR_Legacy_Disabled";
        legacy.SetActive(false);
    }

    private static GameObject EnsurePlayerInstance(GameObject prefab, Scene scene)
    {
        GameObject existing = GameObject.Find("PlayerVR_XRI");
        if (existing != null)
            return existing;

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        player.name = "PlayerVR_XRI";
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Undo.RegisterCreatedObjectUndo(player, "Crear PlayerVR_XRI");

        XROrigin origin = player.GetComponent<XROrigin>();
        if (origin == null)
            throw new MissingComponentException("PlayerVR_XRI no contiene XROrigin.");

        return player;
    }

    private static void EnsureInteractionManager()
    {
        XRInteractionManager manager =
            Object.FindFirstObjectByType<XRInteractionManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            GameObject target = new("XR Interaction Manager");
            Undo.RegisterCreatedObjectUndo(target, "Crear XR Interaction Manager");
            target.AddComponent<XRInteractionManager>();
            return;
        }

        manager.gameObject.SetActive(true);

        // El rig oficial ya habilita XRI Default Input Actions. Evita que el
        // objeto de la escena habilite el mismo asset una segunda vez.
        InputActionManager duplicate = manager.GetComponent<InputActionManager>();
        if (duplicate != null)
            Undo.DestroyObjectImmediate(duplicate);
    }

    private static void EnsureEventSystem()
    {
        EventSystem[] systems =
            Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EventSystem eventSystem;
        if (systems.Length == 0)
        {
            GameObject target = new("EventSystem_XRI");
            Undo.RegisterCreatedObjectUndo(target, "Crear EventSystem XRI");
            eventSystem = target.AddComponent<EventSystem>();
        }
        else
        {
            eventSystem = systems[0];
            eventSystem.gameObject.SetActive(true);
        }

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in modules)
            if (module is not XRUIInputModule)
                Undo.DestroyObjectImmediate(module);

        if (eventSystem.GetComponent<XRUIInputModule>() == null)
            eventSystem.gameObject.AddComponent<XRUIInputModule>();

        for (int i = 1; i < systems.Length; i++)
            systems[i].gameObject.SetActive(false);
    }

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace('\\', '/').TrimEnd('/');
        string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        string name = Path.GetFileName(normalized);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            throw new IOException("Ruta de carpeta inválida: " + path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        if (!AssetDatabase.IsValidFolder(normalized))
            AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
