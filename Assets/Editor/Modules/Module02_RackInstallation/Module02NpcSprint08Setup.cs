#if UNITY_EDITOR
using System;
using System.Linq;
using GameData.Module02;
using Modules.Module02_RackInstallation;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Interaction;
using Modules.Module02_RackInstallation.Presentation.Tutorial;
using Presentacion.NPC;
using Presentacion.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity_WaypointEditor_master.Unity_WaypointEditor_master.Scripts;
using Waypoints;

/// <summary>Conecta una instancia del instructor y sus puntos sin alterar el prefab compartido.</summary>
public static class Module02NpcSprint08Setup
{
    private const string PrefabPath = "Assets/Prefabs/TutorialNPC/InstructorNPC.prefab";
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 8 - NPC y waypoints")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play.");
        T[] All<T>() where T : Component => scene.GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
        T One<T>() where T : Component
        {
            var values = All<T>();
            if (values.Length != 1) throw new InvalidOperationException($"Se requiere un único {typeof(T).Name}.");
            return values[0];
        }
        var existing = All<Module02TutorialController>();
        if (existing.Length > 0)
        {
            Selection.activeGameObject = existing[0].gameObject;
            Debug.Log("Ya existe Module02TutorialController. Revisa sus referencias; no se duplicó el NPC ni se movieron tus waypoints.");
            return;
        }
        var manager = One<Module02Manager>();
        One<Module02SequenceCoordinator>();
        var slot = One<RackInsertionSlot>();
        var camera = All<Camera>().SingleOrDefault(c => c.CompareTag("MainCamera") && c.gameObject.activeInHierarchy);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var data = AssetDatabase.LoadAssetAtPath<Module02TutorialData>("Assets/GameData/Module02/Module02Tutorial.asset");
        if (camera == null || prefab == null || data == null)
            throw new InvalidOperationException("Faltan MainCamera activa, InstructorNPC.prefab o Module02Tutorial.asset.");
        var directors = All<TutorialDirector>();
        if (directors.Length > 1) throw new InvalidOperationException("Hay varios TutorialDirector; deja un único instructor en Modulo2.");
        var candidate = directors.Length == 1
            ? (directors[0].MovementController != null ? directors[0].MovementController.gameObject : directors[0].transform.root.gameObject)
            : prefab;
        var movement = candidate.GetComponentInChildren<NPCMovementController>(true);
        var dialogue = candidate.GetComponentInChildren<NPCDialogueController>(true);
        if (movement == null || dialogue == null || movement.GetComponent<WaypointFollower>() == null ||
            candidate.GetComponentInChildren<TutorialDirector>(true) == null)
            throw new InvalidOperationException("El instructor necesita director, diálogo, movimiento y WaypointFollower.");
        // Evitar reutilizar un instructor que aún esté gobernado por otro módulo.
        if (candidate.GetComponentsInChildren<MonoBehaviour>(true).Any(c => c != null &&
            (c.GetType().Name == "Module01TutorialController" || c.GetType().Name == "LobbyTutorialController")))
            throw new InvalidOperationException("El instructor aún tiene un tutorial de otro módulo conectado.");

        var waypointRoots = scene.GetRootGameObjects().Where(r => r.name == "WaypointsController").ToArray();
        if (waypointRoots.Length > 1) throw new InvalidOperationException("Hay varios WaypointsController en la escena.");
        foreach (var waypointName in new[] { "waypoint_inicio", "waypoint_rack", "waypoint_quiz" })
            if (waypointRoots.Length == 1 && waypointRoots[0].GetComponentsInChildren<Waypoint>(true).Count(p => p.name == waypointName) > 1)
                throw new InvalidOperationException($"Waypoint duplicado: {waypointName}");

        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Sprint 8: instructor y waypoints módulo 2");
        var npc = directors.Length == 1 ? candidate : (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        if (directors.Length == 0) Undo.RegisterCreatedObjectUndo(npc, "Crear instructor");
        var director = npc.GetComponentInChildren<TutorialDirector>(true);
        movement = npc.GetComponentInChildren<NPCMovementController>(true);
        dialogue = npc.GetComponentInChildren<NPCDialogueController>(true);

        // Aproximación inicial: instructor al costado del espacio de trabajo.
        // El follower recorre segmentos rectos, por lo que el usuario debe revisar el paso libre.
        Vector3 towardPlayer = Vector3.ProjectOnPlane(camera.transform.position - slot.transform.position, Vector3.up).normalized;
        if (towardPlayer.sqrMagnitude < 0.01f) towardPlayer = -Vector3.forward;
        Vector3 side = Vector3.Cross(Vector3.up, towardPlayer);
        Vector3 rackPosition = slot.transform.position + towardPlayer * 1.3f + side * 1.2f;
        rackPosition.y = candidate.transform.position.y;
        Vector3 startPosition = directors.Length == 1 ? npc.transform.position : rackPosition + towardPlayer * 1.2f;

        var root = waypointRoots.FirstOrDefault();
        if (root == null) { root = new GameObject("WaypointsController"); Undo.RegisterCreatedObjectUndo(root, "Crear waypoints"); }
        var creator = root.GetComponent<Waypoints_Creator>() ?? Undo.AddComponent<Waypoints_Creator>(root);
        var holder = root.transform.Find("Waypoint Holder");
        if (holder == null)
        {
            var holderObject = new GameObject("Waypoint Holder"); Undo.RegisterCreatedObjectUndo(holderObject, "Crear contenedor");
            holder = holderObject.transform; holder.SetParent(root.transform, false);
        }
        var start = Point("waypoint_inicio", startPosition);
        var rack = Point("waypoint_rack", rackPosition);
        Point("waypoint_quiz", rackPosition - side * 2f); // Reservado: el quiz todavía no está integrado.
        Waypoint Point(string name, Vector3 position)
        {
            var matches = root.GetComponentsInChildren<Waypoint>(true).Where(p => p.name == name).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException($"Waypoint duplicado: {name}");
            if (matches.Length == 1) return matches[0];
            var obj = new GameObject(name); Undo.RegisterCreatedObjectUndo(obj, "Crear punto");
            obj.transform.SetParent(holder, false); obj.transform.position = position;
            obj.transform.rotation = Quaternion.LookRotation(-towardPlayer, Vector3.up);
            var point = Undo.AddComponent<Waypoint>(obj); point.creator = creator;
            return point;
        }
        Set(creator, "_waypoints_Holder", holder.gameObject);
        var creatorData = new SerializedObject(creator);
        var points = root.GetComponentsInChildren<Waypoint>(true).Where(p => p.creator == creator).ToArray();
        var list = creatorData.FindProperty("waypoints"); list.arraySize = points.Length;
        for (int i = 0; i < points.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
        creatorData.ApplyModifiedProperties();
        if (directors.Length == 0)
        {
            npc.transform.position = start.transform.position;
            npc.transform.rotation = Quaternion.LookRotation(-towardPlayer, Vector3.up);
        }
        Set(director, "dialogueController", dialogue); Set(director, "movementController", movement);
        Set(dialogue, "lookTarget", camera.transform);
        var dialogueData = new SerializedObject(dialogue);
        dialogueData.FindProperty("useLegacyInput").boolValue = false; dialogueData.ApplyModifiedProperties();
        var input = npc.GetComponent<Module02NpcInput>() ?? Undo.AddComponent<Module02NpcInput>(npc);
        Set(input, "dialogue", dialogue);
        var tutorial = Undo.AddComponent<Module02TutorialController>(npc);
        Set(tutorial, "manager", manager); Set(tutorial, "director", director); Set(tutorial, "data", data);
        Set(tutorial, "rackWaypoint", rack); Set(tutorial, "startWaypoint", start);
        var reactions = npc.GetComponentInChildren<NPCReactionController>(true);
        Set(tutorial, "reactionController", reactions);
        if (reactions != null)
        {
            Set(reactions, "dialogueController", dialogue);
            var reactionData = new SerializedObject(reactions);
            var animator = reactionData.FindProperty("animator").objectReferenceValue as Animator;
            var controller = animator != null ? animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController : null;
            if (controller != null)
            {
                // Usar las reacciones que realmente existen en el Animator del prefab.
                foreach (var pair in new[] { ("completedAnimationTrigger", "Feliz"), ("alertAnimationTrigger", "Triste") })
                    if (string.IsNullOrEmpty(reactionData.FindProperty(pair.Item1).stringValue) &&
                        controller.parameters.Any(p => p.name == pair.Item2 && p.type == AnimatorControllerParameterType.Trigger))
                        reactionData.FindProperty(pair.Item1).stringValue = pair.Item2;
                reactionData.ApplyModifiedProperties();
            }
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(npc.transform);
        Undo.CollapseUndoOperations(group); EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = root;
        Debug.Log("Sprint 8 conectado. Revisa la ruta libre y altura de waypoint_rack, la posición inicial del NPC y guarda la escena. B confirma; Enter permite depurar. waypoint_quiz queda reservado.");
    }
    private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(name).objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
    }
}
#endif
