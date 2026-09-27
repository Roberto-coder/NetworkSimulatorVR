using System;
using System.Linq;
using TMPro;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Modules.Module03_Diagnostics.Editor
{
    public sealed class Module03DiagnosticUiSetup : EditorWindow
    {
        private Module03NetworkScene network;
        private Camera camera;
        private Transform aim;

        private GameObject radial;
        private DiagnosticUiSettings settings;
        private string status;
        [MenuItem("Network Simulator/Module 03/Sprint 4 - Pantalla y botón A")]
        public static void Open() => GetWindow<Module03DiagnosticUiSetup>("M3 - Pantalla");
        private void OnEnable()
        {
            network = FindObjectsByType<Module03NetworkScene>(FindObjectsSortMode.None).FirstOrDefault(n => n.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            camera = Camera.main;

            settings = AssetDatabase.LoadAssetAtPath<DiagnosticUiSettings>("Assets/GameData/Module03/Presentation/DiagnosticUiSettings.asset");
        }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("A abre/cierra. El gatillo opera botones XRI. Asigna el origen del rayo derecho y el canvas de la rueda para bloquear aperturas simultáneas. No se cambian bindings globales.", MessageType.Info);
            network = (Module03NetworkScene)EditorGUILayout.ObjectField("Red", network, typeof(Module03NetworkScene), true);
            camera = (Camera)EditorGUILayout.ObjectField("Cámara XR", camera, typeof(Camera), true);
            aim = (Transform)EditorGUILayout.ObjectField("Origen rayo derecho", aim, typeof(Transform), true);

            radial = (GameObject)EditorGUILayout.ObjectField("Canvas de rueda", radial, typeof(GameObject), true);
            settings = (DiagnosticUiSettings)EditorGUILayout.ObjectField("Datos UI", settings, typeof(DiagnosticUiSettings), false);
            if (GUILayout.Button("Crear Canvas fijos y zonas en escena (Undo)"))
            {
                try { Build(); status = "Configurado. Ajusta zonas/anclajes y prueba A en Play. No se guardó la escena."; }
                catch (Exception e) { status = e.Message; Debug.LogException(e); }
            }
            if (GUILayout.Button("Desactivar pantallas sobrantes de infraestructura (Undo)"))
            {
                try { DisableInfrastructureScreens(); status = "Pantallas sobrantes desactivadas. Se conservaron PCs y laptop; guarda la escena."; }
                catch (Exception e) { status = e.Message; Debug.LogException(e); }
            }
            if (GUILayout.Button("Migrar Canvas existentes a prefab compartido (Undo)"))
            {
                try { DiagnosticScreenPrefabSetup.Migrate(network, settings); status = "Instancias vinculadas al prefab. Anteriores desactivados como respaldo. Guarda la escena."; }
                catch (Exception e) { status = e.Message; Debug.LogException(e); }
            }
            EditorGUILayout.HelpBox(status ?? "Listo", MessageType.None);
        }
        private void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            if (network == null || camera == null || aim == null)
                throw new InvalidOperationException("Asignar red, cámara y rayo. La rueda solo es un bloqueo opcional.");
            var scene = network.gameObject.scene;
            if (scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene() || camera.gameObject.scene != scene || aim.gameObject.scene != scene || (radial != null && radial.scene != scene))
                throw new InvalidOperationException("Todas las referencias deben pertenecer a la escena activa.");
            var errors = network.ValidateBindings();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            if (Module03ProbeVisualSetup.InScene<DiagnosticScreenController>(scene).Any(c => c.networkScene == network)) throw new InvalidOperationException("La pantalla ya existe: ajusta sus referencias en Inspector.");
            var events = FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Where(e => e.gameObject.scene == scene).ToArray();
            if (events.Length > 1 || (events.Length == 1 && events[0].GetComponent<XRUIInputModule>() == null))
                throw new InvalidOperationException("Se necesita un único EventSystem con XRUIInputModule. Ajusta el existente para evitar módulos de entrada competidores.");
            if (settings == null)
            {
                const string folder = "Assets/GameData/Module03/Presentation";
                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/GameData/Module03", "Presentation");
                settings = AssetDatabase.LoadAssetAtPath<DiagnosticUiSettings>(folder + "/DiagnosticUiSettings.asset");
                if (settings == null) { settings = CreateInstance<DiagnosticUiSettings>(); AssetDatabase.CreateAsset(settings, folder + "/DiagnosticUiSettings.asset"); }
            }
            if (settings.commands == null || settings.commands.Length != 6) throw new InvalidOperationException("Los datos UI requieren seis comandos: Ping, ARP, Configuración local, Puertos, Bitácora, Cerrar.");
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("M3 pantalla diagnóstica");
            try
            {
                if (events.Length == 0)
                {
                    var obj = new GameObject("M03 EventSystem"); Undo.RegisterCreatedObjectUndo(obj, "EventSystem");
                    Undo.AddComponent<EventSystem>(obj); Undo.AddComponent<XRUIInputModule>(obj);
                }
                var root = new GameObject("Diagnostic workspace"); Undo.RegisterCreatedObjectUndo(root, "Pantalla"); root.transform.SetParent(network.transform, false);
                var controller = Undo.AddComponent<DiagnosticScreenController>(root);
                controller.networkScene = network; controller.settings = settings; controller.playerCamera = camera; controller.rightAim = aim; controller.radialCanvas = radial;

                foreach (var device in network.GetComponentsInChildren<NetworkDeviceBinding>(true))
                {
                    if (!DiagnosticWorkspace.HasLocalScreen(network.initialState.CopyDefinition(), device.DeviceId)) continue;
                    // La laptop es una estación fija de escena, no una herramienta equipable.
                    if (device.DeviceId == settings.laptopDeviceId)
                    {
                        foreach (var grab in device.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(true))
                        { Undo.RecordObject(grab, "Fijar laptop"); grab.enabled = false; }
                        foreach (var body in device.GetComponentsInChildren<Rigidbody>(true))
                        { Undo.RecordObject(body, "Fijar laptop"); body.isKinematic = true; body.useGravity = false; }
                    }
                    if (device.GetComponent<DiagnosticInteractionTarget>() != null) continue;
                    var target = Undo.AddComponent<DiagnosticInteractionTarget>(device.gameObject); target.device = device;
                    // Se construye una sola vez en Editor. La vista queda serializada y editable.
                    var view = DiagnosticScreenPrefabSetup.Instantiate(root.transform, network, settings, camera);
                    view.name = "Canvas_" + device.DeviceId;
                    Undo.RegisterCreatedObjectUndo(view.gameObject, "Canvas fijo");
                    view.transform.position = device.transform.position + Vector3.up * 0.35f - device.transform.forward * 0.8f;
                    view.transform.rotation = device.transform.rotation;
                    target.screen = view;
                    var prompt = new GameObject("Diagnostic A hint");
                    Undo.RegisterCreatedObjectUndo(prompt, "Indicador fijo");
                    prompt.transform.SetParent(device.transform, false);
                    prompt.transform.localPosition = Vector3.up * 0.3f;
                    var text = Undo.AddComponent<TextMeshPro>(prompt);
                    text.text = settings.focusHint; text.fontSize = 0.2f;
                    text.alignment = TextAlignmentOptions.Center;
                    text.rectTransform.sizeDelta = new Vector2(1, 0.2f);
                    target.focusHint = text; prompt.SetActive(false);
                    var zone = new GameObject("Diagnostic hit zone"); Undo.RegisterCreatedObjectUndo(zone, "Zona A"); zone.transform.SetParent(device.transform, false);
                    var box = Undo.AddComponent<BoxCollider>(zone); box.isTrigger = true; box.size = new Vector3(0.5f, 0.35f, 0.3f);
                    target.hitZone = box;
                }
                EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group); Selection.activeGameObject = root;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        private void DisableInfrastructureScreens()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || network == null)
                throw new InvalidOperationException("Selecciona la red y sal de Play.");
            var definition = network.initialState.CopyDefinition();
            var targets = network.GetComponentsInChildren<DiagnosticInteractionTarget>(true);
            var protectedViews = targets.Where(t => DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)).Select(t => t.screen).ToArray();
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("M3 desactivar pantallas de infraestructura");
            foreach (var target in targets.Where(t => !DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)))
            {
                // No eliminar objetos: se conserva cualquier edición del usuario y se permite Undo.
                Undo.RecordObject(target, "Desactivar interacción local"); target.enabled = false;
                if (target.screen != null && !protectedViews.Contains(target.screen))
                { Undo.RecordObject(target.screen.gameObject, "Ocultar Canvas sobrante"); target.screen.gameObject.SetActive(false); }
                if (target.focusHint != null)
                { Undo.RecordObject(target.focusHint.gameObject, "Ocultar indicación"); target.focusHint.gameObject.SetActive(false); }
            }
            Undo.CollapseUndoOperations(group); EditorSceneManager.MarkSceneDirty(network.gameObject.scene);
        }
    }
}

