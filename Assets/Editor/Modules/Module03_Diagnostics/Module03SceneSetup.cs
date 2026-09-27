using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;

namespace Modules.Module03_Diagnostics.Editor
{
    /// <summary>Composición explícita con Undo. Nunca guarda o sustituye una escena completa.</summary>
    public sealed class Module03SceneSetup : EditorWindow
    {
        private NetworkInitialStateAsset initial;
        private NetworkLayoutAsset layout;
        private NetworkPrefabCatalog catalog;
        private readonly Dictionary<string, GameObject> bindings = new();
        private Vector2 scroll;
        private string status;
        private const string Data = "Assets/GameData/Module03/";

        [MenuItem("Network Simulator/Module 03/Sprint 2 - Escenario y rutas")]
        public static void Open() => GetWindow<Module03SceneSetup>("M3 - Escenario");
        private void OnEnable()
        {
            initial = AssetDatabase.LoadAssetAtPath<NetworkInitialStateAsset>(Data + "Topologies/OfficeInitialState.asset");
            layout = AssetDatabase.LoadAssetAtPath<NetworkLayoutAsset>(Data + "Layouts/OfficeLayout.asset");
            catalog = AssetDatabase.LoadAssetAtPath<NetworkPrefabCatalog>(Data + "Catalogs/OfficePrefabCatalog.asset");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox("Fuera de Play. Asocia modelos de esta escena o deja el campo vacío para usar el catálogo. Los modelos asociados conservan su posición; los anclajes nuevos son provisionales. Ajusta puertos y splines antes de validar.", MessageType.Info);
            initial = (NetworkInitialStateAsset)EditorGUILayout.ObjectField("Estado inicial", initial, typeof(NetworkInitialStateAsset), false);
            layout = (NetworkLayoutAsset)EditorGUILayout.ObjectField("Layout", layout, typeof(NetworkLayoutAsset), false);
            catalog = (NetworkPrefabCatalog)EditorGUILayout.ObjectField("Catálogo", catalog, typeof(NetworkPrefabCatalog), false);
            if (GUILayout.Button("Crear assets y prefabs provisionales faltantes")) Run(CreateDefaults);
            if (initial != null)
            {
                try
                {
                    foreach (var d in initial.CopyDefinition().devices)
                    {
                        bindings.TryGetValue(d.id, out var existing);
                        bindings[d.id] = (GameObject)EditorGUILayout.ObjectField(d.id, existing, typeof(GameObject), true);
                    }
                }
                catch (ArgumentException e) { EditorGUILayout.HelpBox(e.Message, MessageType.Error); }
            }
            if (GUILayout.Button("Crear / completar infraestructura (Undo)")) Run(Build);
            if (GUILayout.Button("Validar infraestructura")) Run(() =>
            {
                var root = FindRoot() ?? throw new InvalidOperationException("No existe infraestructura en la escena activa.");
                foreach (var route in root.GetComponentsInChildren<FixedNetworkCable>(true)) route.Rebuild();
                var errors = root.ValidateBindings();
                if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                status = "IDs, anclajes y rutas fijas correctos. Falta la comprobación visual de colocación.";
            });
            if (GUILayout.Button("Guardar posiciones y rutas en layout")) Run(Capture);
            EditorGUILayout.HelpBox(status ?? "Listo", MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private void Run(Action action)
        {
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play antes de editar.");
                action();
            }
            catch (Exception e) { status = e.Message; Debug.LogException(e); }
        }

        private static Module03NetworkScene FindRoot()
        {
            var matches = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<Module03NetworkScene>(true)).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("Hay varias raíces de red en la escena activa.");
            return matches.FirstOrDefault();
        }

        private static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/'); Folder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private void CreateDefaults()
        {
            if (initial == null) throw new InvalidOperationException("Selecciona estado inicial.");
            var network = initial.CopyDefinition();
            Folder(Data + "Layouts"); Folder(Data + "Catalogs"); Folder("Assets/Prefabs/Dispositivos/Modulo3/Provisional");
            if (layout == null)
            {
                layout = AssetDatabase.LoadAssetAtPath<NetworkLayoutAsset>(Data + "Layouts/OfficeLayout.asset");
                if (layout == null)
                {
                    layout = CreateInstance<NetworkLayoutAsset>(); layout.scenarioId = network.scenarioId;
                    for (int i = 0; i < network.devices.Count; i++)
                        layout.devices.Add(new DevicePlacement { deviceId = network.devices[i].id,
                            position = new Vector3((i % 3) * 1.6f, 1.1f, (i / 3) * 1.5f), mapPosition = new Vector2(i % 3, i / 3) });
                    AssetDatabase.CreateAsset(layout, Data + "Layouts/OfficeLayout.asset");
                }
            }
            if (catalog == null)
            {
                catalog = AssetDatabase.LoadAssetAtPath<NetworkPrefabCatalog>(Data + "Catalogs/OfficePrefabCatalog.asset");
                if (catalog == null) { catalog = CreateInstance<NetworkPrefabCatalog>(); AssetDatabase.CreateAsset(catalog, Data + "Catalogs/OfficePrefabCatalog.asset"); }
            }
            if (layout.cableMaterial == null)
            {
                string path = Data + "Layouts/FixedCable.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (shader == null) throw new InvalidOperationException("No se encuentra shader URP/Unlit.");
                    material = new Material(shader); material.SetColor("_BaseColor", new Color(0.1f, 0.35f, 0.65f));
                    AssetDatabase.CreateAsset(material, path);
                }
                layout.cableMaterial = material;
            }
            foreach (string key in network.devices.Select(d => d.prefabKey).Distinct())
            {
                if (catalog.entries.Any(e => e.key == key)) continue;
                // Nombres de archivo independientes de claves suministradas por el diseñador.
                string path = $"Assets/Prefabs/Dispositivos/Modulo3/Provisional/Device-{catalog.entries.Count:00}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    var temporary = new GameObject(key);
                    try
                    {
                        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        body.transform.SetParent(temporary.transform, false); body.transform.localScale = new Vector3(0.4f, 0.25f, 0.2f);
                        DestroyImmediate(body.GetComponent<Collider>());
                        prefab = PrefabUtility.SaveAsPrefabAsset(temporary, path);
                    }
                    finally { DestroyImmediate(temporary); }
                }
                catalog.entries.Add(new NetworkPrefabEntry { key = key, prefab = prefab });
            }
            EditorUtility.SetDirty(layout); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            status = "Assets disponibles; puedes cambiar prefabs y posiciones antes de crear.";
        }

        private void Build()
        {
            if (initial == null || layout == null || catalog == null) throw new InvalidOperationException("Faltan assets.");
            var network = initial.CopyDefinition();
            if (layout.scenarioId != network.scenarioId || layout.cableMaterial == null) throw new InvalidOperationException("Layout incompatible o sin material.");
            if (layout.devices.Any(d => d == null) || layout.devices.GroupBy(d => d.deviceId).Any(g => g.Count() > 1) ||
                catalog.entries.Any(e => e == null) || catalog.entries.GroupBy(e => e.key).Any(g => g.Count() > 1) ||
                layout.fixedRoutes.Any(r => r == null || r.knots == null) || layout.fixedRoutes.GroupBy(r => r.cableId).Any(g => g.Count() > 1) ||
                layout.ports.Any(p => p == null) || layout.ports.GroupBy(p => p.portId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Entradas nulas o IDs repetidos en layout/catálogo.");
            var root = FindRoot();
            if (root != null && root.initialState != initial) throw new InvalidOperationException("La raíz pertenece a otra configuración.");
            var selected = bindings.Values.Where(g => g != null).ToArray();
            if (selected.Distinct().Count() != selected.Length || selected.Any(g => selected.Any(other => other != g && g.transform.IsChildOf(other.transform))))
                throw new InvalidOperationException("Cada dispositivo requiere una raíz distinta, sin anidar asociaciones.");
            foreach (var d in network.devices)
            {
                if (!layout.devices.Any(p => p.deviceId == d.id)) throw new InvalidOperationException($"Falta posición para {d.id}.");
                bindings.TryGetValue(d.id, out var existing);
                if (existing != null && (EditorUtility.IsPersistent(existing) || existing.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
                    throw new InvalidOperationException($"Asociación fuera de la escena activa: {d.id}.");
                if (existing == null && !catalog.entries.Any(e => e.key == d.prefabKey && e.prefab != null))
                    throw new InvalidOperationException($"Falta prefab: {d.prefabKey}.");
            }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("M3 crear infraestructura");
            try
            {
                if (root == null)
                {
                    var obj = new GameObject("Module03_Network"); Undo.RegisterCreatedObjectUndo(obj, "Raíz de red");
                    root = Undo.AddComponent<Module03NetworkScene>(obj);
                }
                Undo.RecordObject(root, "Configurar red"); root.initialState = initial; root.layout = layout; root.prefabCatalog = catalog;
                foreach (var d in network.devices)
                {
                    var matches = root.GetComponentsInChildren<NetworkDeviceBinding>(true).Where(b => b.DeviceId == d.id).ToArray();
                    if (matches.Length > 1) throw new InvalidOperationException($"ID repetido: {d.id}.");
                    var binding = matches.FirstOrDefault();
                    bindings.TryGetValue(d.id, out var existing);
                    if (binding != null && existing != null && binding.gameObject != existing) throw new InvalidOperationException($"{d.id} ya está asociado.");
                    if (binding == null)
                    {
                        GameObject obj = existing;
                        if (obj == null)
                        {
                            obj = (GameObject)PrefabUtility.InstantiatePrefab(catalog.entries.First(e => e.key == d.prefabKey).prefab, root.transform);
                            Undo.RegisterCreatedObjectUndo(obj, "Instanciar dispositivo");
                            var pose = layout.devices.First(p => p.deviceId == d.id);
                            obj.transform.localPosition = pose.position; obj.transform.localRotation = Quaternion.Euler(pose.rotation);
                            obj.transform.localScale = pose.scale;
                        }
                        else
                        {
                            if (obj.GetComponentInChildren<NetworkDeviceBinding>(true) != null || root.transform.IsChildOf(obj.transform))
                                throw new InvalidOperationException($"Objeto ya asociado o raíz inválida: {d.id}.");
                            Undo.SetTransformParent(obj.transform, root.transform, "Asociar dispositivo");
                        }
                        binding = Undo.AddComponent<NetworkDeviceBinding>(obj); binding.Configure(d.id);
                    }
                    int index = 0;
                    foreach (var p in network.ports.Where(p => p.deviceId == d.id))
                    {
                        if (binding.GetComponentsInChildren<NetworkPortAnchor>(true).Any(a => a.PortId == p.id)) continue;
                        var obj = new GameObject(p.id); Undo.RegisterCreatedObjectUndo(obj, "Anclaje"); obj.transform.SetParent(binding.transform, false);
                        obj.transform.localPosition = new Vector3(index++ * 0.035f, 0, -0.12f);
                        var portPose = layout.ports.FirstOrDefault(a => a.portId == p.id);
                        if (portPose != null) { obj.transform.localPosition = portPose.localPosition; obj.transform.localRotation = Quaternion.Euler(portPose.localRotation); }
                        Undo.AddComponent<NetworkPortAnchor>(obj).Configure(p.id);
                    }
                }
                var anchors = root.GetComponentsInChildren<NetworkPortAnchor>(true).ToDictionary(a => a.PortId);
                foreach (var c in network.cables.Where(c => !c.interactable))
                {
                    if (root.GetComponentsInChildren<FixedNetworkCable>(true).Any(r => r.CableId == c.id)) continue;
                    var obj = new GameObject("Fixed_" + c.id); Undo.RegisterCreatedObjectUndo(obj, "Ruta fija"); obj.transform.SetParent(root.transform, false);
                    var container = Undo.AddComponent<SplineContainer>(obj);
                    var saved = layout.fixedRoutes.FirstOrDefault(r => r.cableId == c.id);
                    var knots = saved != null && saved.knots.Count >= 2 ? saved.knots.ToArray() : new[] {
                        new BezierKnot(obj.transform.InverseTransformPoint(anchors[c.portA].transform.position)),
                        new BezierKnot(obj.transform.InverseTransformPoint(anchors[c.portB].transform.position)) };
                    container.Spline = new Spline(knots);
                    var route = Undo.AddComponent<FixedNetworkCable>(obj); route.Configure(c.id, anchors[c.portA], anchors[c.portB], layout.routeSamples);
                    var line = obj.GetComponent<LineRenderer>(); line.sharedMaterial = layout.cableMaterial;
                    line.widthMultiplier = layout.cableRadius * 2; line.numCapVertices = 4; line.numCornerVertices = 4;
                    // La malla vive en un hijo para separar MeshRenderer del LineRenderer de respaldo.
                    var tube = new GameObject("Tube"); Undo.RegisterCreatedObjectUndo(tube, "Malla tubular");
                    tube.transform.SetParent(obj.transform, false);
                    var extrude = Undo.AddComponent<SplineExtrude>(tube);
                    extrude.Container = container; extrude.Radius = layout.cableRadius;
                    extrude.Sides = 8; extrude.SegmentsPerUnit = 8; extrude.RebuildOnSplineChange = false;
                    tube.GetComponent<MeshRenderer>().sharedMaterial = layout.cableMaterial;
                    route.Rebuild();
                }
                EditorSceneManager.MarkSceneDirty(root.gameObject.scene); Undo.CollapseUndoOperations(group);
                Selection.activeGameObject = root.gameObject;
                status = "Infraestructura creada. Alinea anclajes; selecciona cada Fixed_* para editar spline y ajustar extremos. No se guardó la escena.";
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        private void Capture()
        {
            var root = FindRoot() ?? throw new InvalidOperationException("No existe raíz.");
            if (layout == null || root.layout != layout) throw new InvalidOperationException("Selecciona el layout de la raíz.");
            var routes = root.GetComponentsInChildren<FixedNetworkCable>(true);
            if (routes.Any(r => r.transform.parent != root.transform || r.transform.localPosition != Vector3.zero ||
                r.transform.localRotation != Quaternion.identity || r.transform.localScale != Vector3.one))
                throw new InvalidOperationException("Las rutas deben mantener transform local identidad bajo la raíz; edita sus knots.");
            foreach (var r in routes) r.Rebuild();
            var errors = root.ValidateBindings();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            Undo.RecordObject(layout, "Guardar layout de red");
            foreach (var d in root.GetComponentsInChildren<NetworkDeviceBinding>(true))
            {
                var pose = layout.devices.FirstOrDefault(p => p.deviceId == d.DeviceId);
                if (pose == null) { pose = new DevicePlacement { deviceId = d.DeviceId }; layout.devices.Add(pose); }
                pose.position = root.transform.InverseTransformPoint(d.transform.position);
                pose.rotation = (Quaternion.Inverse(root.transform.rotation) * d.transform.rotation).eulerAngles;
                pose.scale = d.transform.localScale;
            }
            layout.fixedRoutes = routes.Select(r => new FixedRouteLayout { cableId = r.CableId,
                knots = r.GetComponent<SplineContainer>().Spline.ToList() }).ToList();
            layout.ports = root.GetComponentsInChildren<NetworkPortAnchor>(true).Select(p =>
            {
                var device = p.GetComponentInParent<NetworkDeviceBinding>().transform;
                return new PortPlacement { portId = p.PortId, localPosition = device.InverseTransformPoint(p.transform.position),
                    localRotation = (Quaternion.Inverse(device.rotation) * p.transform.rotation).eulerAngles };
            }).ToList();
            EditorUtility.SetDirty(layout); AssetDatabase.SaveAssets(); status = "Posiciones y knots guardados; mapa 2D conservado.";
        }
    }
}
