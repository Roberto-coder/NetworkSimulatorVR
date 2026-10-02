using System;
using System.Linq;
using GameData.Module03;
using Framework.Interaction.Tools;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using Presentacion.NPC;
using Presentacion.Tutorial;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Waypoints;
namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03GuidedFlowSetup
    {
        [MenuItem("Network Simulator/Module 03/Sprint 8 - Flujo guiado y objetivos")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = EditorSceneManager.GetActiveScene();
            T[] All<T>() where T : Component => Module03ProbeVisualSetup.InScene<T>(scene);
            var network = All<Module03NetworkScene>().Single();
            var repair = All<CableRepairController>().Single(r => r.network == network);
            var screens = Module03ProbeVisualSetup.ControllerFor(network);
            var tools = All<ToolManager>().Single();
            var camera = All<Camera>().FirstOrDefault(c => c.CompareTag("MainCamera"));
            var settings = AssetDatabase.LoadAssetAtPath<Module03FlowSettings>("Assets/GameData/Module03/Presentation/Module03FlowSettings.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TutorialNPC/InstructorNPC.prefab");
            if (settings == null || settings.module == null || settings.targets == null || prefab == null || camera == null)
                throw new InvalidOperationException("Faltan GameData, InstructorNPC o MainCamera.");
            if (settings.module.Objectives.Count != 6 || settings.tutorial == null ||
                Enumerable.Range(1, 26).Select(i => "M3D" + i.ToString("00")).Any(id => settings.Find(id) == null))
                throw new InvalidOperationException("Revisar los seis objetivos y Module03Tutorial.asset.");
            var errors = settings.targets.ValidateAgainst(network.initialState);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            if (All<Module03GuidedFlow>().Length > 0)
            { Module03TutorialSetup.Configure(); return; }
            var directors = All<TutorialDirector>();
            if (directors.Length > 1) throw new InvalidOperationException("Deja un único TutorialDirector en la escena.");
            if (All<MonoBehaviour>().Any(c => c != null && (c.GetType().Name == "Module01TutorialController" || c.GetType().Name == "Module02TutorialController" || c.GetType().Name == "LobbyTutorialController")))
                throw new InvalidOperationException("Retira el controlador de tutorial de otro módulo antes de reutilizar su NPC.");
            var npcCandidate = directors.Length == 0 ? prefab : directors[0].transform.root.gameObject;
            if (npcCandidate.GetComponentInChildren<TutorialDirector>(true) == null || npcCandidate.GetComponentInChildren<NPCDialogueController>(true) == null || npcCandidate.GetComponentInChildren<NPCMovementController>(true) == null)
                throw new InvalidOperationException("El instructor requiere diálogo y movimiento.");
            var repairTools = tools.AvailableTools.Where(t => t != null && t.prefab != null && t.prefab.GetComponent<CableRepairTool>() != null).ToList();
            if (!repairTools.Any(t => t.prefab.GetComponent<CableRepairTool>().mode == CableRepairTool.Mode.Tester) ||
                !repairTools.Any(t => t.prefab.GetComponent<CableRepairTool>().mode == CableRepairTool.Mode.LabelMaker))
                throw new InvalidOperationException("Aplica primero el sprint 6: faltan tester o etiquetadora en la rueda.");
            var devices = network.GetComponentsInChildren<NetworkDeviceBinding>(true);
            var laptop = devices.Single(d => d.DeviceId == "Laptop").transform;
            var pc = devices.Single(d => d.DeviceId == "PC-01").transform;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Sprint 8 M3");
            var root = new GameObject("Module03 guided flow"); Undo.RegisterCreatedObjectUndo(root, "Flujo M3");
            var managers = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "_Managers");
            if (managers != null) root.transform.SetParent(managers.transform, false);
            var flow = Undo.AddComponent<Module03GuidedFlow>(root);
            flow.network = network; flow.repair = repair; flow.screens = screens; flow.settings = settings; flow.tools = tools;
            // Conservar los prefabs/iconos preparados en sprint 6; limitar la rueda a las dos herramientas.
            Undo.RecordObject(settings.module, "Herramientas M3");
            settings.module.availableTools = repairTools;
            EditorUtility.SetDirty(settings.module);
            var director = directors.FirstOrDefault();
            if (director == null)
            {
                var npc = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                Undo.RegisterCreatedObjectUndo(npc, "Instructor M3");
                npc.transform.position = laptop.position + Vector3.right * 1.5f;
                // Altura inicial conservadora; ajustar sobre el suelo real antes de Play.
                var position = npc.transform.position; position.y = 0; npc.transform.position = position;
                director = npc.GetComponentInChildren<TutorialDirector>(true);
            }
            var movement = director.GetComponentInParent<NPCMovementController>() ?? director.transform.root.GetComponentInChildren<NPCMovementController>(true);
            var dialogue = director.GetComponentInChildren<NPCDialogueController>(true) ?? director.transform.root.GetComponentInChildren<NPCDialogueController>(true);
            Set(director, "dialogueController", dialogue); Set(director, "movementController", movement);
            Set(dialogue, "lookTarget", camera.transform);
            var input = new SerializedObject(dialogue); input.FindProperty("useLegacyInput").boolValue = true; input.ApplyModifiedProperties();
            var guidance = Undo.AddComponent<Module03GuidancePresenter>(root); guidance.flow = flow; guidance.director = director;
            var waypointRoot = new GameObject("M3 waypoints"); Undo.RegisterCreatedObjectUndo(waypointRoot, "Waypoints M3");
            waypointRoot.transform.SetParent(root.transform, false);
            var creator = waypointRoot.AddComponent<Waypoints_Creator>();
            guidance.laptopWaypoint = Point("Laptop", laptop.position + Vector3.right * 1.5f);
            guidance.cableWaypoint = Point("PC-01", pc.position + Vector3.right * 1.5f);
            Waypoint Point(string name, Vector3 position)
            {
                var obj = new GameObject(name); obj.transform.SetParent(waypointRoot.transform, false);
                position.y = movement != null ? movement.transform.position.y : 0;
                obj.transform.position = position;
                var point = obj.AddComponent<Waypoint>(); point.creator = creator; return point;
            }
            var creatorData = new SerializedObject(creator);
            creatorData.FindProperty("_waypoints_Holder").objectReferenceValue = waypointRoot;
            var points = creatorData.FindProperty("waypoints"); points.arraySize = 2;
            points.GetArrayElementAtIndex(0).objectReferenceValue = guidance.laptopWaypoint;
            points.GetArrayElementAtIndex(1).objectReferenceValue = guidance.cableWaypoint;
            creatorData.ApplyModifiedProperties();
            // La guía utiliza el diálogo del instructor, sin panel adicional de ayudas.
            EditorUtility.SetDirty(flow); EditorUtility.SetDirty(guidance);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Module03TutorialSetup.Configure();
            Debug.Log("Sprint 8 preparado. Guarda la escena; ajusta panel de ayuda, NPC y waypoints al suelo/paso libre. Audios opcionales en Module03Tutorial.asset.", flow);
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            if (target == null) throw new InvalidOperationException("Instructor incompleto.");
            Undo.RecordObject(target, "Referencias NPC M3");
            var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
        [MenuItem("Network Simulator/Module 03/Instalar objetivos en anclaje de muñeca seleccionado")]
        private static void InstallWrist()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var anchor = Selection.activeTransform;
            if (anchor == null || !anchor.gameObject.scene.IsValid())
                throw new InvalidOperationException("Selecciona en la jerarquía el anclaje seguido por la mano/controlador izquierdo.");
            var scene = anchor.gameObject.scene;
            var camera = Module03ProbeVisualSetup.InScene<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI_Components/Objectives/ObjectivesCanva.prefab");
            if (camera == null || prefab == null) throw new InvalidOperationException("Falta MainCamera o prefab de objetivos.");
            var existing = Module03ProbeVisualSetup.InScene<Presentacion.GlobalUI.ObjectivesWristMenu.WristMenuController>(scene);
            if (existing.Length > 0)
            {
                Selection.activeGameObject = existing[0].gameObject;
                Debug.Log("Ya existe un controlador de muñeca. Revisa Head Transform, Wrist Transform y Wrist Menu Canvas en este objeto.", existing[0]);
                return;
            }
            var canvas = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor);
            Undo.RegisterCreatedObjectUndo(canvas, "Objetivos de muñeca M3");
            // Mantener escala y orientación de autoría; ajustar el panel al dorso de la mano en escena.
            canvas.SetActive(true);
            var controller = canvas.GetComponent<Presentacion.GlobalUI.ObjectivesWristMenu.WristMenuController>();
            controller.Configure(camera.transform, anchor, canvas);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = canvas;
            Debug.Log("Objetivos instalados bajo " + anchor.name + ". Ajusta posición, orientación y Local Display Normal antes de Play.", canvas);
        }
    }
}
