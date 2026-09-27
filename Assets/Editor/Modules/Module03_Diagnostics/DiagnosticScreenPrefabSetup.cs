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
