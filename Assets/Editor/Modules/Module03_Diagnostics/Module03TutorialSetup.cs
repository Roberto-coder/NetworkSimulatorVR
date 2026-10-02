using System;
using System.Linq;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Waypoints;
namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03TutorialSetup
    {
        [MenuItem("Network Simulator/Module 03/Actualizar recorrido y presentación del tutorial")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Sal de Play antes de configurar.");
            var scene = EditorSceneManager.GetActiveScene();
            var flow = Module03ProbeVisualSetup.InScene<Module03GuidedFlow>(scene).Single();
            var guidance = Module03ProbeVisualSetup.InScene<Module03GuidancePresenter>(scene).Single();
            if (guidance.laptopWaypoint == null || guidance.cableWaypoint == null)
                throw new InvalidOperationException("Asigna primero los waypoints existentes de laptop y PC-01.");
            Undo.RecordObject(flow, "Tutorial M3"); Undo.RecordObject(guidance, "Tutorial M3");
            flow.guidance = guidance;
            var camera = Module03ProbeVisualSetup.InScene<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
            var parent = guidance.laptopWaypoint.transform.parent;
            float height = guidance.laptopWaypoint.transform.position.y;
            Waypoint Point(string name, Vector3 position)
            {
                var obj = new GameObject(name); Undo.RegisterCreatedObjectUndo(obj, "Waypoint M3");
                obj.transform.SetParent(parent, false); position.y = height; obj.transform.position = position;
                return Undo.AddComponent<Waypoint>(obj);
            }
            // Crear sólo lo faltante. Nunca sobreescribir posiciones que el autor ya acomodó.
            if (guidance.initialWaypoint == null)
                guidance.initialWaypoint = Point("Waypoint_inicial", camera != null ? camera.transform.position + camera.transform.forward * 2 : guidance.transform.position);
            if (guidance.laptopEntryWaypoint == null)
            {
                var pos = guidance.laptopWaypoint.transform.position;
                pos.z = guidance.cableWaypoint.transform.position.z;
                guidance.laptopEntryWaypoint = Point("Waypoint_laptop1", pos);
            }
            if (guidance.quizWaypoint == null)
            {
                var finale = Module03ProbeVisualSetup.InScene<Module03FinaleController>(scene).FirstOrDefault();
                var pos = finale != null && finale.quizRoot != null ? finale.quizRoot.transform.position : guidance.initialWaypoint.transform.position;
                guidance.quizWaypoint = Point("Waypoint_quiz", pos + Vector3.right);
            }
            var presenter = flow.GetComponent<Module03PresentationController>();
            if (presenter == null) presenter = Undo.AddComponent<Module03PresentationController>(flow.gameObject);
            Undo.RecordObject(presenter, "Inventario M3"); presenter.flow = flow;
            if (presenter.replacement == null)
                presenter.replacement = flow.repair.cables.Single(c => c.cableId == "Replacement-01");
            EditorUtility.SetDirty(flow); EditorUtility.SetDirty(guidance); EditorUtility.SetDirty(presenter);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = guidance.gameObject;
            Debug.Log("Tutorial actualizado. Ajusta Waypoint_inicial frente al jugador, laptop1 en la esquina de entrada, laptopWaypoint dentro del cuarto, PC-01 y quiz en espacio libre. Guarda la escena. Los Canvas existentes se conservan.", guidance);
        }
    }
}
