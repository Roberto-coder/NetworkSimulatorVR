using System;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    /// <summary>Autoría de un prefab compartido. Nunca genera UI durante Play.</summary>
    public static class DiagnosticScreenPrefabSetup
    {
        public const string PrefabPath = "Assets/Prefabs/Dispositivos/Modulo3/DiagnosticScreen.prefab";

        [MenuItem("Network Simulator/Module 03/Recrear pantallas desde prefab (Undo)")]
        public static void RecreateScreens()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Sal de Play antes de recrear las pantallas.");
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Cierra Prefab Mode y abre la escena Modulo3.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var networks = Module03ProbeVisualSetup.InScene<Module03NetworkScene>(scene);
            if (networks.Length != 1)
                throw new InvalidOperationException("La escena activa debe contener una única red M3.");
            var network = networks[0];
            var controller = Module03ProbeVisualSetup.ControllerFor(network);
            if (network.initialState == null || controller.settings == null || controller.playerCamera == null ||
                controller.playerCamera.gameObject.scene != scene)
                throw new InvalidOperationException("Conserva el controlador y asigna Initial State, Settings y Player Camera de esta escena.");
            var prefab = AssetDatabase.LoadAssetAtPath<DiagnosticScreenView>(PrefabPath);
            if (prefab == null || prefab.GetComponent<Canvas>() == null)
                throw new InvalidOperationException("Falta el prefab DiagnosticScreen o su Canvas.");
            var definition = network.initialState.CopyDefinition();
            var targets = network.GetComponentsInChildren<DiagnosticInteractionTarget>(true)
                .Where(t => DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)).ToArray();
            var expected = definition.devices.Where(d => DiagnosticWorkspace.HasLocalScreen(definition, d.id)).ToArray();
            if (expected.Length == 0 || expected.Any(d => targets.Count(t => t.DeviceId == d.id) != 1))
                throw new InvalidOperationException("Cada PC y laptop debe conservar un DiagnosticInteractionTarget con Device asignado. Borra solo los Canvas antiguos, no los dispositivos ni sus componentes de interacción.");
            if (targets.Any(t => t.screen != null && t.screen.gameObject.scene != scene))
                throw new InvalidOperationException("Una pantalla apunta fuera de la escena. Limpia esa referencia antes de recrear.");

            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("M3 recrear pantallas desde prefab");
            try
            {
                // Un padre neutro evita heredar escalas de modelos o de otros Canvas de la escena.
                var root = new GameObject("M03 Diagnostic Screens");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
                Undo.RegisterCreatedObjectUndo(root, "Contenedor de pantallas");
                foreach (var target in targets)
                {
                    var old = target.screen;
                    var position = old != null ? old.transform.position :
                        target.device.transform.position + Vector3.up * .35f - target.device.transform.forward * .8f;
                    var rotation = old != null ? old.transform.rotation : target.device.transform.rotation;
                    var view = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, root.transform);
                    Undo.RegisterCreatedObjectUndo(view, "Pantalla nueva");
                    view.name = "Canvas_" + target.DeviceId;
                    view.transform.SetPositionAndRotation(position, rotation);
                    // Tamaño, escala y toda la UI interna proceden del prefab, sin copiar overrides antiguos.
                    var canvas = view.GetComponent<Canvas>();
                    canvas.worldCamera = controller.playerCamera;
                    view.SetActive(true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(view.transform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(view);
                    Undo.RecordObject(target, "Reconectar pantalla del dispositivo");
                    target.screen = view.GetComponent<DiagnosticScreenView>();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                    if (old != null)
                    {
                        Undo.RecordObject(old.gameObject, "Desactivar pantalla anterior");
                        old.name = "Canvas_" + target.DeviceId + " (anterior - borrar)";
                        old.gameObject.SetActive(false);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(old.gameObject);
                    }
                }
                Undo.CollapseUndoOperations(group);
                EditorSceneManager.MarkSceneDirty(scene);
                Selection.activeGameObject = root;
                Debug.Log($"M3: {targets.Length} pantallas recreadas y conectadas. Revisa su ubicación y guarda la escena. Puedes borrar las marcadas '(anterior - borrar)'. No se modificó el prefab ni se guardó la escena automáticamente.", root);
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        public static DiagnosticScreenView LoadOrCreate(Module03NetworkScene network, DiagnosticUiSettings settings)
        {
            var existing = AssetDatabase.LoadAssetAtPath<DiagnosticScreenView>(PrefabPath);
            if (existing != null) return existing; // Respetar los cambios hechos en Prefab Mode.
            PrepareMap(settings, network);
            DiagnosticScreenView temporary = null;
            try
            {
                temporary = DiagnosticCanvasBuilder.Build(null, network, settings, null);
                // El prefab no guarda referencias a objetos de escena (cámara/dispositivo).
                var saved = PrefabUtility.SaveAsPrefabAsset(temporary.gameObject, PrefabPath);
                if (saved == null) throw new InvalidOperationException("No se pudo guardar el prefab de diagnóstico.");
                return saved.GetComponent<DiagnosticScreenView>();
            }
            finally { if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary.gameObject); }
        }

        public static DiagnosticScreenView Instantiate(Transform parent, Module03NetworkScene network,
            DiagnosticUiSettings settings, Camera camera)
        {
            var prefab = LoadOrCreate(network, settings);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
            instance.GetComponent<Canvas>().worldCamera = camera;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<Canvas>());
            return instance.GetComponent<DiagnosticScreenView>();
        }

        public static void Migrate(Module03NetworkScene network, DiagnosticUiSettings settings)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || network == null || settings == null)
                throw new InvalidOperationException("Salir de Play y asignar red y datos UI.");
            var controller = Module03ProbeVisualSetup.ControllerFor(network);
            var definition = network.initialState.CopyDefinition();
            var targets = network.GetComponentsInChildren<DiagnosticInteractionTarget>(true)
                .Where(t => DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)).ToArray();
            if (targets.Any(t => t.screen == null)) throw new InvalidOperationException("Falta una pantalla en los targets.");
            LoadOrCreate(network, settings);
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("M3 migrar Canvas a prefab compartido");
            try
            {
                foreach (var target in targets)
                {
                    var old = target.screen;
                    if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(old.gameObject) == PrefabPath) continue;
                    var view = Instantiate(old.transform.parent, network, settings, controller.playerCamera);
                    Undo.RegisterCreatedObjectUndo(view.gameObject, "Instancia fija de diagnóstico");
                    // Mantener exactamente el lugar y tamaño elegidos por el usuario.
                    view.name = "Canvas_" + target.DeviceId;
                    view.transform.localPosition = old.transform.localPosition;
                    view.transform.localRotation = old.transform.localRotation;
                    view.transform.localScale = old.transform.localScale;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(view.transform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(view.gameObject);
                    Undo.RecordObject(target, "Asignar pantalla compartida"); target.screen = view;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                    Undo.RecordObject(old.gameObject, "Conservar Canvas anterior");
                    old.name += " (respaldo)"; old.gameObject.SetActive(false);
                }
                EditorSceneManager.MarkSceneDirty(network.gameObject.scene);
                Undo.CollapseUndoOperations(group);
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        private static void PrepareMap(DiagnosticUiSettings settings, Module03NetworkScene network)
        {
            // Valores iniciales para esta oficina. Después se editan en el asset o en el prefab.
            foreach (var device in network.initialState.CopyDefinition().devices)
            {
                if (settings.mapNodes.Any(n => n.deviceId == device.id)) continue;
                string file = device.prefabKey switch
                {
                    "computer" => "PC", "laptop" => "lap", "switch" => "switch",
                    "patch-panel" => "PatchPanel", "rosette" => "roseta", _ => null
                };
                Sprite sprite = null;
                if (file != null)
                {
                    string path = "Assets/Recursos/Modulo3/UI/" + file + ".png";
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.SaveAndReimport();
                    }
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) throw new InvalidOperationException("No se encontró sprite: " + path);
                }
                int index = settings.mapNodes.Count;
                Vector2 position = device.id switch
                {
                    "Laptop" => new Vector2(75, -185), "SW-01" => new Vector2(325, -270),
                    "PP-01" => new Vector2(325, -85),
                    "R01" => new Vector2(620, -65), "PC-01" => new Vector2(910, -65),
                    "R02" => new Vector2(620, -185), "PC-02" => new Vector2(910, -185),
                    "R03" => new Vector2(620, -305), "PC-03" => new Vector2(910, -305),
                    _ => new Vector2(75 + index % 4 * 260, -65 - index / 4 * 120)
                };
                string name = device.prefabKey switch
                {
                    "patch-panel" => "PATCH PANEL " + device.id.Split('-').Last(),
                    "switch" => "SWITCH " + device.id.Split('-').Last(),
                    "rosette" => "ROSETA " + device.id.TrimStart('R'),
                    _ => device.id.ToUpperInvariant()
                };
                settings.mapNodes.Add(new DiagnosticMapNode { deviceId = device.id, displayName = name, sprite = sprite, position = position });
            }
            EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        }
    }
}
