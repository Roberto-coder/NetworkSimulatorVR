#if UNITY_EDITOR
using System;
using System.Linq;
using Framework.Spawning;
using GameData.Modules;
using GameData.Quiz;
using Modules.Module02_RackInstallation;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Presentation.Quiz;
using Modules.Module02_RackInstallation.Presentation.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Waypoints;

/// <summary>Conecta el cierre, manteniendo la práctica y el tutorial existentes.</summary>
public static class Module02FinaleSprint09Setup
{
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 9 - Quiz y cierre")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play.");
        T[] All<T>() where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
        if (All<Module02FinaleController>().Length > 0)
        { Debug.Log("El cierre ya está conectado; ajusta QuizSpawner_Module02 y sus referencias sin repetir el configurador."); return; }
        var manager = All<Module02Manager>().Single();
        All<Module02SequenceCoordinator>().Single();
        var tutorial = All<Module02TutorialController>().SingleOrDefault();
        var waypoint = All<Waypoint>().SingleOrDefault(p => p.name == "waypoint_quiz");
        var camera = All<Camera>().SingleOrDefault(c => c.CompareTag("MainCamera") && c.gameObject.activeInHierarchy);
        var events = All<EventSystem>().SingleOrDefault(e => e.isActiveAndEnabled);
        if (waypoint == null || camera == null || events == null || events.GetComponent<XRUIInputModule>() == null)
            throw new InvalidOperationException("Requiere waypoint_quiz del sprint 8, MainCamera y EventSystem con XRUIInputModule.");
        var quiz = AssetDatabase.LoadAssetAtPath<QuizData>("Assets/GameData/Quiz/Module02Quiz.asset");
        var definition = new SerializedObject(manager).FindProperty("moduleDefinition").objectReferenceValue as ModuleDefinition;
        if (quiz == null || !quiz.IsValid || definition == null || definition.CompletionAchievement == null)
            throw new InvalidOperationException("Faltan Module02Quiz válido, definición del módulo o insignia.");
        var prefab = Module02QuizPrefabBuilder.Build();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Sprint 9: cierre del módulo 2");
        Set(definition, "finalQuiz", quiz);
        var root = new GameObject("Module02_Finale"); Undo.RegisterCreatedObjectUndo(root, "Crear cierre");
        var point = new GameObject("QuizSpawner_Module02"); Undo.RegisterCreatedObjectUndo(point, "Crear posición del quiz");
        point.transform.SetParent(root.transform, false);
        // El NPC queda a un costado, con el Canvas orientado hacia el espacio del jugador.
        var direction = Vector3.ProjectOnPlane(waypoint.transform.position - camera.transform.position, Vector3.up).normalized;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        point.transform.position = waypoint.transform.position + Vector3.Cross(Vector3.up, direction) * 1.1f;
        point.transform.position = new Vector3(point.transform.position.x, camera.transform.position.y, point.transform.position.z);
        point.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        var spawner = Undo.AddComponent<ObjectSpawner>(point);
        Set(spawner, "prefab", prefab); Set(spawner, "spawnPoint", point.transform);
        var finale = Undo.AddComponent<Module02FinaleController>(root);
        Set(finale, "manager", manager); Set(finale, "quizSpawner", spawner); Set(finale, "tutorial", tutorial);
        if (tutorial != null) Set(tutorial, "quizWaypoint", waypoint);
        // Reiniciar y salir necesitan ambas escenas habilitadas en la compilación.
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Select(s =>
            s.path == scene.path || s.path == "Assets/Scenes/Lobby.unity" ? new EditorBuildSettingsScene(s.path, true) : s).ToArray();
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = point;
        Debug.Log("Sprint 9 listo. Ajusta QuizSpawner_Module02 y la ruta al waypoint_quiz; guarda la escena. El quiz aparece al terminar práctica y tutorial.");
    }
    private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
    { var serialized = new SerializedObject(target); serialized.FindProperty(name).objectReferenceValue = value; serialized.ApplyModifiedProperties(); }
}
#endif
