#if UNITY_EDITOR
using System.Collections.Generic;
using Framework.Interaction.Tools;
using GameData.Modules;
using GameData.Objectives;
using Modules.Lobby.Presentation.Tutorial;
using Modules.Lobby;
using Modules.Lobby.Presentation;
using Modules.Lobby.Objectives;
using Modules.Module01_CableMaking;
using Presentacion.NPC;
using Presentacion.Tutorial;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Waypoints;

public static class LobbyModuleSetup
{
    private const string LobbyScene = "Assets/Scenes/Lobby.unity";
    private const string ModuleScene = "Assets/Scenes/Modulo1.unity";
    private const string DataFolder = "Assets/GameData/Lobby";
    private const string ModuleAssetPath = "Assets/GameData/Modules/Lobby.asset";
    private const string ObjectiveAssetPath = "Assets/GameData/Objectives/Lobby/OBJ_CompleteTutorial.asset";
    private const string PrefabFolder = "Assets/Prefabs/Lobby";
    private const string NpcPrefabPath = PrefabFolder + "/InstructorNPC.prefab";
    private const string DialoguePrefabPath = PrefabFolder + "/InstructorDialogue.prefab";

    [MenuItem("Network Simulator/Configurar módulo Lobby")]
    public static void Configure()
    {
        EnsureFolders();
        ModuleDefinition module = CreateModuleData();
        GameObject npcPrefab = CreateNpcPrefab();
        GameObject dialoguePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DialoguePrefabPath);
        Scene lobby = EditorSceneManager.OpenScene(LobbyScene, OpenSceneMode.Single);

        Transform panel = Require("WP_panel");
        Transform museum = Require("WP_zona2");
        Transform final = Require("WP_final");
        Transform managers = Require("_Managers");

        GameObject npc = GameObject.Find("InstructorNPC");
        if (npc == null)
        {
            npc = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab, lobby);
            npc.name = "InstructorNPC";
            npc.transform.SetPositionAndRotation(panel.position + panel.right * 1.2f, panel.rotation);
        }

        NPCPlayerLookController look = GetOrAdd<NPCPlayerLookController>(npc);
        NPCMovementController movement = npc.GetComponent<NPCMovementController>();
        NPCDialogueController dialogue = Object.FindFirstObjectByType<NPCDialogueController>(FindObjectsInactive.Include);
        if (dialogue == null)
            dialogue = ((GameObject)PrefabUtility.InstantiatePrefab(dialoguePrefab, lobby))
                .GetComponent<NPCDialogueController>();

        GameObject tutorialRuntime = GameObject.Find("LobbyTutorialRuntime") ?? new GameObject("LobbyTutorialRuntime");
        tutorialRuntime.transform.SetParent(managers, false);
        TutorialDirector director = GetOrAdd<TutorialDirector>(tutorialRuntime);
        SerializedObject directorData = new(director);
        directorData.FindProperty("dialogueController").objectReferenceValue = dialogue;
        directorData.FindProperty("movementController").objectReferenceValue = movement;
        directorData.ApplyModifiedPropertiesWithoutUndo();
        NPCReactionController reactions = tutorialRuntime.GetComponent<NPCReactionController>();
        ToolManager tools = Object.FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);
        SimulationManager wrongManager = managers.GetComponent<SimulationManager>();
        if (wrongManager != null)
            Object.DestroyImmediate(wrongManager);

        LobbyManager lobbyManager = GetOrAdd<LobbyManager>(managers.gameObject);
        SerializedObject managerData = new(lobbyManager);
        managerData.FindProperty("moduleDefinition").objectReferenceValue = module;
        managerData.ApplyModifiedPropertiesWithoutUndo();

        LobbyTutorialController controller = GetOrAdd<LobbyTutorialController>(managers.gameObject);
        SerializedObject serialized = new(controller);
        serialized.FindProperty("director").objectReferenceValue = director;
        serialized.FindProperty("mainPanelWaypoint").objectReferenceValue = panel.GetComponent<Waypoint>();
        serialized.FindProperty("museumAreaWaypoint").objectReferenceValue = museum.GetComponent<Waypoint>();
        serialized.FindProperty("finalWaypoint").objectReferenceValue = final.GetComponent<Waypoint>();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        LobbyPresentationController presentation = GetOrAdd<LobbyPresentationController>(managers.gameObject);
        SerializedObject presentationData = new(presentation);
        presentationData.FindProperty("tutorialController").objectReferenceValue = controller;
        presentationData.FindProperty("toolManager").objectReferenceValue = tools;
        presentationData.ApplyModifiedPropertiesWithoutUndo();

        Button repeatButton = GameObject.Find("TutorialToggle")?.GetComponent<Button>();
        if (repeatButton != null)
        {
            UnityEventTools.RemovePersistentListener(repeatButton.onClick, presentation.RepeatTutorial);
            UnityEventTools.AddPersistentListener(repeatButton.onClick, presentation.RepeatTutorial);
        }

        EditorSceneManager.MarkSceneDirty(lobby);
        EditorSceneManager.SaveScene(lobby);
        AssetDatabase.SaveAssets();
        Debug.Log("Lobby configurado: tutorial, objetivo, waypoints, NPC, rueda y 3 spawnables.");
    }

    private static ModuleDefinition CreateModuleData()
    {
        ObjectiveData objective = AssetDatabase.LoadAssetAtPath<ObjectiveData>(ObjectiveAssetPath);
        if (objective == null)
        {
            objective = ScriptableObject.CreateInstance<ObjectiveData>();
            Set(objective, "id", CompleteLobbyTutorialObjective.ObjectiveId);
            Set(objective, "title", "Completar el tutorial del Lobby");
            Set(objective, "description", "Realiza el recorrido guiado por el panel principal y el museo.");
            Set(objective, "introductionDialogue", "Sigue al instructor para conocer el laboratorio.");
            Set(objective, "reminderDialogue", "Continúa el recorrido con el instructor.");
            AssetDatabase.CreateAsset(objective, ObjectiveAssetPath);
        }

        ModuleDefinition module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModuleAssetPath);
        if (module == null)
        {
            module = ScriptableObject.CreateInstance<ModuleDefinition>();
            AssetDatabase.CreateAsset(module, ModuleAssetPath);
        }

        SerializedObject serialized = new(module);
        serialized.FindProperty("moduleId").stringValue = "lobby";
        serialized.FindProperty("moduleName").stringValue = "Lobby";
        serialized.FindProperty("description").stringValue = "Introducción a los laboratorios de red.";
        SerializedProperty objectives = serialized.FindProperty("objectives");
        objectives.arraySize = 1;
        objectives.GetArrayElementAtIndex(0).objectReferenceValue = objective;

        SerializedProperty tools = serialized.FindProperty("availableTools");
        string[] names = { "Cubo", "Esfera", "Triángulo" };
        PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cube };
        tools.arraySize = names.Length;
        for (int i = 0; i < names.Length; i++)
        {
            SerializedProperty entry = tools.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("name").stringValue = names[i];
            entry.FindPropertyRelative("prefab").objectReferenceValue = CreateShapePrefab(names[i], types[i], i == 2);
            entry.FindPropertyRelative("icon").objectReferenceValue = CreateIcon(names[i], i);
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(module);
        return module;
    }

    private static GameObject CreateShapePrefab(string name, PrimitiveType type, bool triangle)
    {
        string path = PrefabFolder + "/Tool_" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
            return existing;

        GameObject shape = triangle ? CreateTriangle() : GameObject.CreatePrimitive(type);
        shape.name = "Tool_" + name;
        shape.transform.localScale = Vector3.one * 0.12f;
        if (triangle)
            shape.transform.localScale = new Vector3(0.12f, 0.12f, 0.04f);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shape, path);
        Object.DestroyImmediate(shape);
        return prefab;
    }

    private static GameObject CreateTriangle()
    {
        GameObject result = new("Triangle");
        Mesh mesh = new()
        {
            name = "TrianglePrism",
            vertices = new[]
            {
                new Vector3(-.5f, -.5f, -.1f), new Vector3(.5f, -.5f, -.1f), new Vector3(0f, .5f, -.1f),
                new Vector3(-.5f, -.5f, .1f), new Vector3(.5f, -.5f, .1f), new Vector3(0f, .5f, .1f)
            },
            triangles = new[]
            {
                0, 2, 1, 3, 4, 5,
                0, 1, 4, 0, 4, 3,
                1, 2, 5, 1, 5, 4,
                2, 0, 3, 2, 3, 5
            }
        };
        mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, DataFolder + "/TrianglePrism.asset");
        result.AddComponent<MeshFilter>().sharedMesh = mesh;
        result.AddComponent<MeshRenderer>();
        result.AddComponent<MeshCollider>().sharedMesh = mesh;
        return result;
    }

    private static Sprite CreateIcon(string name, int index)
    {
        string path = DataFolder + "/Icon_" + name + ".asset";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null)
            return existing;

        Texture2D texture = new(64, 64, TextureFormat.RGBA32, false) { name = "IconTexture_" + name };
        Color color = index switch
        {
            0 => new Color(.2f, .7f, 1f, 1f),
            1 => new Color(.45f, 1f, .55f, 1f),
            _ => new Color(1f, .65f, .2f, 1f)
        };
        Color[] pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            bool inside = index == 0
                ? x > 10 && x < 53 && y > 10 && y < 53
                : index == 1
                    ? (new Vector2(x - 31.5f, y - 31.5f).sqrMagnitude < 22f * 22f)
                    : y > 10 && y < 54 && x > 32 - (y - 10) / 2 && x < 32 + (y - 10) / 2;
            pixels[y * 64 + x] = inside ? color : Color.clear;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, path);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64f);
        sprite.name = "Icon_" + name;
        AssetDatabase.AddObjectToAsset(sprite, texture);
        AssetDatabase.ImportAsset(path);
        return sprite;
    }

    private static GameObject CreateNpcPrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath);
        if (existing != null)
            return existing;

        Scene source = EditorSceneManager.OpenScene(ModuleScene, OpenSceneMode.Single);
        GameObject npc = GameObject.Find("InstructorNPC");
        if (npc == null)
            throw new MissingReferenceException("No se encontró InstructorNPC en Modulo1.unity.");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(npc, NpcPrefabPath);

        NPCDialogueController dialogue = Object.FindFirstObjectByType<NPCDialogueController>(FindObjectsInactive.Include);
        if (dialogue == null)
            throw new MissingReferenceException("No se encontró NPCDialogueController en Modulo1.unity.");
        PrefabUtility.SaveAsPrefabAsset(dialogue.gameObject, DialoguePrefabPath);
        EditorSceneManager.CloseScene(source, true);
        return prefab;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/GameData", "Lobby");
        EnsureFolder("Assets/GameData/Objectives", "Lobby");
        EnsureFolder("Assets/Prefabs", "Lobby");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static Transform Require(string name)
    {
        GameObject found = GameObject.Find(name);
        if (found == null)
            throw new MissingReferenceException("No se encontró " + name + " en Lobby.unity.");
        if (found.GetComponent<Waypoint>() == null && name.StartsWith("WP_"))
            found.AddComponent<Waypoint>();
        return found.transform;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.GetComponent<T>() ?? target.AddComponent<T>();

    private static void Set(Object target, string property, string value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(property).stringValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
