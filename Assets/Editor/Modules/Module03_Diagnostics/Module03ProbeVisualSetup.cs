using System;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Presentation;
using Modules.Module03_Diagnostics.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03ProbeVisualSetup
    {
        // Buscar en la escena concreta incluye objetos inactivos, sin depender del padre ni del nombre.
        internal static T[] InScene<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        internal static DiagnosticScreenController ControllerFor(Module03NetworkScene network)
        {
            var matches = InScene<DiagnosticScreenController>(network.gameObject.scene)
                .Where(c => c.networkScene == network).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("Debe existir un único DiagnosticScreenController que referencie esta red. Revisar Network Scene en _UI.");
            return matches[0];
        }

        [MenuItem("Network Simulator/Module 03/Sprint 5 - Animación de ping")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = SceneManager.GetActiveScene();
            Module03NetworkScene network = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<Module03NetworkScene>(true))
                {
                    if (network != null) throw new InvalidOperationException("La escena debe contener una única red M3.");
                    network = candidate;
                }
            if (network == null) throw new InvalidOperationException("Falta la red M3 en la escena activa.");
            var controller = ControllerFor(network);
            // Reutilizar el presenter movido a _Managers antes de crear cualquier objeto.
            var presenters = InScene<ProbeTracePresenter>(scene)
                .Where(p => p.network == network || (p.network == null && (p.screenController == controller || p.screenController == null))).ToArray();
            if (presenters.Length > 1) throw new InvalidOperationException("Hay varios ProbeTracePresenter para esta red. Resolver el duplicado antes de configurar.");
            var presenter = presenters.SingleOrDefault();
            var shader = Shader.Find("NetworkSimulator/SplineDemoUnlit");
            if (shader == null) throw new InvalidOperationException("No se encontró el shader URP del módulo 3.");
            const string folder = "Assets/GameData/Module03/Presentation";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/GameData/Module03", "Presentation");
            var settings = AssetDatabase.LoadAssetAtPath<ProbeVisualSettings>(folder + "/ProbeVisualSettings.asset");
            if (settings == null)
            { settings = ScriptableObject.CreateInstance<ProbeVisualSettings>(); AssetDatabase.CreateAsset(settings, folder + "/ProbeVisualSettings.asset"); }
            if (settings.overlayMaterial == null)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/ProbeOverlay.mat");
                if (material == null)
                {
                    material = new Material(shader); material.SetFloat("_ZWrite", 0); material.renderQueue = 2990;
                    AssetDatabase.CreateAsset(material, folder + "/ProbeOverlay.mat");
                }
                settings.overlayMaterial = material; EditorUtility.SetDirty(settings);
            }
            // Actualizar el prefab compartido una vez; no sobrescribir su diseño ni sus nodos.
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DiagnosticScreenPrefabSetup.PrefabPath) != null)
            {
                var content = PrefabUtility.LoadPrefabContents(DiagnosticScreenPrefabSetup.PrefabPath);
                try
                {
                    DiagnosticCanvasBuilder.AddTraceControls(content.GetComponent<DiagnosticScreenView>());
                    PrefabUtility.SaveAsPrefabAsset(content, DiagnosticScreenPrefabSetup.PrefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(content); }
            }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("M3 animación ping");
            try
            {
                // Los targets conservan referencias a las pantallas, aunque éstas vivan en _UI.
                var views = network.GetComponentsInChildren<DiagnosticInteractionTarget>(true)
                    .Select(t => t.screen).Where(v => v != null && v.gameObject.scene == scene).Distinct();
                foreach (var view in views)
                {
                    if (view.xRayButton != null && view.animationStatus != null && view.nodes.All(n => n.selectionOutline != null)) continue;
                    Undo.RegisterFullObjectHierarchyUndo(view.gameObject, "Controles de traza");
                    bool needsButton = view.xRayButton == null, needsStatus = view.animationStatus == null;
                    DiagnosticCanvasBuilder.AddTraceControls(view);
                    if (needsButton) Undo.RegisterCreatedObjectUndo(view.xRayButton.gameObject, "Botón rayos X");
                    if (needsStatus) Undo.RegisterCreatedObjectUndo(view.animationStatus.gameObject, "Estado de traza");
                    EditorUtility.SetDirty(view); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
                }
                if (presenter == null)
                {
                    var obj = new GameObject("Probe trace visualization");
                    // _Managers sólo define dónde crear un objeto nuevo; no es necesario para encontrarlo después.
                    var managers = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "_Managers");
                    if (managers != null) obj.transform.SetParent(managers.transform, false);
                    Undo.RegisterCreatedObjectUndo(obj, "Visualización");
                    presenter = Undo.AddComponent<ProbeTracePresenter>(obj);
                }
                Undo.RecordObject(presenter, "Referencias de visualización");
                // Completar referencias faltantes también en un presenter creado antes de reorganizar.
                if (presenter.overlay == null)
                {
                    var lineObject = new GameObject("Active segment"); lineObject.transform.SetParent(presenter.transform, false);
                    Undo.RegisterCreatedObjectUndo(lineObject, "Tramo visual");
                    presenter.overlay = Undo.AddComponent<LineRenderer>(lineObject);
                    presenter.overlay.positionCount = 0; presenter.overlay.enabled = false;
                }
                if (presenter.packet == null)
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.name = "Packet (reused)"; sphere.transform.SetParent(presenter.transform, false);
                    UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());
                    Undo.RegisterCreatedObjectUndo(sphere, "Paquete visual");
                    presenter.packet = sphere.GetComponent<Renderer>(); presenter.packet.enabled = false;
                }
                presenter.network = network; presenter.screenController = controller; presenter.settings = settings;
                EditorUtility.SetDirty(presenter); EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group); AssetDatabase.SaveAssets(); Selection.activeGameObject = presenter.gameObject;
                Debug.Log("M3 sprint 5 preparado. Guarda la escena, ejecuta ping y prueba Rayos X. La esfera representa tráfico, no se lanza físicamente.", presenter);
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
    }
}
