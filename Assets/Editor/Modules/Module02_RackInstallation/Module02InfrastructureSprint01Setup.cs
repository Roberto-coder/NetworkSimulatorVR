using Modules.Module02_RackInstallation.Domain;
#if UNITY_EDITOR
using System;
using System.Linq;
using HPhysic;
using Modules.Module02_RackInstallation.Interaction;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit scene bindings preserve hand-placed devices and existing sockets.</summary>
public sealed class Module02InfrastructureSprint01Setup : EditorWindow
{
    private const string RootName = "Module02_Infrastructure_Sprint01";
    private const string SocketPath = "Assets/Prefabs/Tools/Module02/Cabling/NetworkPortSocket_RJ45.prefab";
    private const string CablePath = "Assets/Prefabs/Tools/Module02/Cabling/PatchCable_RJ45_1_7m.prefab";
    private Transform switchDevice, patchPanel, firewall, pdu;
    private Vector3 hooksPosition = new(1, 1.5f, 0);
    private Vector3 hooksRotation;
    private string message;

    private static readonly string[] Origins = Module02ConnectionPlan.Links.Select(l => l.Origin).ToArray();
    private static readonly string[] Destinations = Module02ConnectionPlan.Links.Select(l => l.Destination).ToArray();

    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 1 - Puertos y ganchos")]
    public static void Open() => GetWindow<Module02InfrastructureSprint01Setup>("M2 - Infraestructura");

    private void OnEnable()
    {
        var existing = SceneObjects<NetworkPort>().FirstOrDefault(p => p.DeviceId == "SW1");
        var grab = existing != null ? existing.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() : null;
        switchDevice = grab != null ? grab.transform : Find("Switch_12Port_Educational");
        patchPanel = Find("Patch panel");
        firewall = Find("Firewall");
        pdu = Find("UPS / PDU vertical");
        var rack = Find("rackV1");
        if (rack != null)
        {
            hooksPosition = rack.position + rack.rotation * new Vector3(0.85f, 1.5f, 0.3f);
            hooksRotation = rack.eulerAngles;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Escena Modulo2 abierta, fuera de Play. Asigna las raíces de los dispositivos. Se conservan sockets existentes con el mismo ID. Las posiciones nuevas son iniciales: alinea sus anclajes con el modelo antes de probar en VR.", MessageType.Info);
        switchDevice = (Transform)EditorGUILayout.ObjectField("SW1", switchDevice, typeof(Transform), true);
        patchPanel = (Transform)EditorGUILayout.ObjectField("PP-A", patchPanel, typeof(Transform), true);
        firewall = (Transform)EditorGUILayout.ObjectField("FW1", firewall, typeof(Transform), true);
        pdu = (Transform)EditorGUILayout.ObjectField("PDU-A", pdu, typeof(Transform), true);
        hooksPosition = EditorGUILayout.Vector3Field("Ganchos (posición mundo)", hooksPosition);
        hooksRotation = EditorGUILayout.Vector3Field("Ganchos (rotación)", hooksRotation);
        EditorGUILayout.Space();
        for (int i = 0; i < Origins.Length; i++) EditorGUILayout.LabelField($"C{i + 1:00}: {Origins[i]} → {Destinations[i]}");
        EditorGUILayout.LabelField("Reserva: SW1/Console (sin cable suelto)");
        if (GUILayout.Button("Crear infraestructura (Undo disponible)"))
        {
            try { Build(); message = "Infraestructura creada. Ajusta posiciones y guarda la escena."; }
            catch (Exception ex) { message = ex.Message; Debug.LogException(ex); }
        }
        if (GUILayout.Button("Validar inventario"))
        {
            try { ValidateInventory(); message = "Inventario correcto: 11 puertos, 4 dispositivos, 5 cables y 5 ganchos."; }
            catch (Exception ex) { message = ex.Message; Debug.LogError(message); }
        }
        if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    private void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play Mode.");
        Transform[] devices = { switchDevice, patchPanel, firewall, pdu };
        if (devices.Any(d => d == null || d.gameObject.scene != scene) || devices.Distinct().Count() != 4)
            throw new InvalidOperationException("Asigna cuatro dispositivos distintos de esta escena.");
        if (Find(RootName) != null)
            throw new InvalidOperationException("El sprint ya existe. Ajusta sus objetos; no se duplicará ni reemplazará la configuración.");
        var socketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SocketPath);
        var cablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CablePath);
        if (socketPrefab == null || cablePrefab == null || cablePrefab.GetComponent<PhysicCable>() == null)
            throw new InvalidOperationException("Faltan prefabs de cableado válidos.");

        Undo.IncrementCurrentGroup();
        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Infraestructura módulo 2 sprint 1");
        try
        {
            var root = Create(RootName, null);
            Identify(switchDevice, "SW1"); Identify(patchPanel, "PP-A");
            Identify(firewall, "FW1"); Identify(pdu, "PDU-A");
            for (int i = 0; i < 4; i++) Port(switchDevice, "SW1", $"Gi{i + 1:00}", NetworkPortKind.EthernetRj45, i, socketPrefab);
            Port(switchDevice, "SW1", "Console", NetworkPortKind.ConsoleRj45, 4, socketPrefab);
            Port(switchDevice, "SW1", "Power", NetworkPortKind.Power, 5, socketPrefab);
            for (int i = 0; i < 3; i++) Port(patchPanel, "PP-A", $"{i + 1:00}", NetworkPortKind.EthernetRj45, i, socketPrefab);
            Port(firewall, "FW1", "eth01", NetworkPortKind.EthernetRj45, 0, socketPrefab);
            Port(pdu, "PDU-A", "AC01", NetworkPortKind.Power, 0, socketPrefab);

            // Disable only unused cabling sockets, never the device's inspection or grab components.
            string[] expected = Origins.Concat(Destinations).Append("SW1/Console").ToArray();
            foreach (var port in SceneObjects<NetworkPort>().Where(p => !expected.Contains(p.Address)))
            {
                Undo.RecordObject(port.gameObject, "Desactivar socket fuera del inventario");
                port.gameObject.SetActive(false);
            }
            // Existing loose test cables remain in the scene, inactive and recoverable with Undo.
            foreach (var cable in SceneObjects<PatchCableLink>())
            {
                Undo.RecordObject(cable.gameObject, "Retirar cable de prueba");
                cable.gameObject.SetActive(false);
            }
            var hooks = Create("CableHooks", root.transform);
            hooks.transform.SetPositionAndRotation(hooksPosition, Quaternion.Euler(hooksRotation));
            for (int i = 0; i < 5; i++)
            {
                var hook = Create($"Hook_C{i + 1:00}", hooks.transform);
                hook.transform.localPosition = new Vector3(i * 0.42f, 0, 0);
                Bar(hook.transform, "Back", new Vector3(0, 0, -0.04f), new Vector3(0.025f, 0.1f, 0.025f));
                Bar(hook.transform, "Support", new Vector3(0, 0, 0), new Vector3(0.025f, 0.025f, 0.1f));
                Bar(hook.transform, "Lip", new Vector3(0, 0.015f, 0.045f), new Vector3(0.025f, 0.05f, 0.025f));
                Label(hook.transform, $"C{i + 1:00}\n{Origins[i]} → {Destinations[i]}", new Vector3(0, 0.13f, 0.02f), 0.36f, 0.07f);
                var cable = (GameObject)PrefabUtility.InstantiatePrefab(cablePrefab, hook.transform);
                Undo.RegisterCreatedObjectUndo(cable, "Crear cable");
                cable.name = $"C{i + 1:00}_{(i == 0 ? "Power" : "Ethernet")}";
                cable.transform.localPosition = new Vector3(0, 0.025f, 0);
                cable.transform.localRotation = Quaternion.identity;
                Set(cable.GetComponent<PatchCableLink>(), "kind", (int)(i == 0 ? NetworkPortKind.Power : NetworkPortKind.EthernetRj45));
                var traffic = cable.GetComponent<CableTrafficVisualizer>();
                if (traffic != null) Set(traffic, "emitDemoTraffic", false);
                Undo.AddComponent<CableHookStorage>(cable);
                if (i == 0) PowerVisual(cable);
            }
            ValidateInventory();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;
            Undo.CollapseUndoOperations(undo);
        }
        catch { Undo.RevertAllDownToGroup(undo); throw; }
    }

    private static void Identify(Transform device, string id)
    {
        var identity = device.GetComponent<DeviceIdentity>() ?? Undo.AddComponent<DeviceIdentity>(device.gameObject);
        Undo.RecordObject(identity, "Identificar dispositivo");
        identity.Configure(id);
        PrefabUtility.RecordPrefabInstancePropertyModifications(identity);
        Label(device, id, Face(device, -1), 0.1f, 0.028f);
    }

    private static void Port(Transform device, string id, string portId, NetworkPortKind kind, int index, GameObject prefab)
    {
        var matching = SceneObjects<NetworkPort>().Where(p => p.Address == $"{id}/{portId}").ToArray();
        if (matching.Length > 1) throw new InvalidOperationException($"ID duplicado: {id}/{portId}");
        NetworkPort port = matching.FirstOrDefault();
        if (port != null && !port.transform.IsChildOf(device))
            throw new InvalidOperationException($"{port.Address} no pertenece al dispositivo asignado. Corrige la raíz SW1/dispositivo.");
        if (port == null)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, device);
            Undo.RegisterCreatedObjectUndo(obj, "Crear puerto");
            obj.name = $"Port_{portId}";
            obj.transform.localPosition = Face(device, index);
            obj.transform.localRotation = Quaternion.identity;
            port = obj.GetComponent<NetworkPort>();
        }
        Undo.RecordObject(port, "Identificar puerto");
        port.Configure(id, portId, kind);
        PrefabUtility.RecordPrefabInstancePropertyModifications(port);
        // Keep physical sizes independent of imported model scale.
        float scale = Mathf.Abs(port.transform.lossyScale.x);
        if (scale > 0) { Undo.RecordObject(port.transform, "Escala socket"); port.transform.localScale /= scale; }
        Label(port.transform, portId, new Vector3(0, 0.023f, 0.012f), 0.028f, 0.012f);
        if (kind == NetworkPortKind.Power) PowerVisual(port.gameObject);
    }

    private static Vector3 Face(Transform device, int index)
    {
        var renderers = device.GetComponentsInChildren<Renderer>(true).Where(r => r.GetComponentInParent<Canvas>() == null && r.GetComponentInParent<NetworkPort>() == null).ToArray();
        if (renderers.Length == 0) return new Vector3(index * 0.035f, 0, 0);
        Bounds bounds = new(device.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (var r in renderers)
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 e = r.bounds.extents;
                bounds.Encapsulate(device.InverseTransformPoint(r.bounds.center + new Vector3((corner & 1) == 0 ? -e.x : e.x, (corner & 2) == 0 ? -e.y : e.y, (corner & 4) == 0 ? -e.z : e.z)));
            }
        float scale = Mathf.Max(0.0001f, Mathf.Abs(device.lossyScale.x));
        return new Vector3(index < 0 ? bounds.center.x : bounds.min.x + (0.04f + index * 0.035f) / scale,
            index < 0 ? bounds.max.y + 0.035f / scale : bounds.center.y, bounds.max.z + 0.015f / scale);
    }

    private static void Label(Transform parent, string text, Vector3 position, float width, float height)
    {
        var obj = new GameObject("VisibleID", typeof(RectTransform), typeof(Canvas));
        Undo.RegisterCreatedObjectUndo(obj, "Crear ID visible");
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        var canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        var rect = (RectTransform)obj.transform;
        rect.sizeDelta = new Vector2(width * 1000, height * 1000);
        Vector3 s = parent.lossyScale;
        rect.localScale = new Vector3(0.001f / Mathf.Abs(s.x), 0.001f / Mathf.Abs(s.y), 0.001f / Mathf.Abs(s.z));
        rect.localRotation = Quaternion.Euler(0, 180, 0);
        var background = obj.AddComponent<Image>(); background.color = new Color(0.025f, 0.055f, 0.08f, 0.94f); background.raycastTarget = false;
        var child = new GameObject("Text", typeof(RectTransform)); child.transform.SetParent(rect, false);
        var label = child.AddComponent<TextMeshProUGUI>();
        label.text = text; label.fontSize = height * 650; label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true; label.fontSizeMin = 4; label.fontSizeMax = height * 650;
        label.raycastTarget = false; label.color = Color.white;
        var tr = (RectTransform)child.transform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
    }

    private static void PowerVisual(GameObject root)
    {
        // Temporary distinct electrical geometry; never present an RJ45 as a power plug.
        var ends = root.GetComponentsInChildren<Modules.Module03_Diagnostics.Cable_physics.Scripts.Connector>(true);
        foreach (var end in ends)
        {
            foreach (var renderer in end.GetComponentsInChildren<MeshRenderer>(true))
            { Undo.RecordObject(renderer, "Ocultar visual RJ45"); renderer.enabled = false; }
            Bar(end.transform, "PowerConnector_Provisional", Vector3.zero, new Vector3(0.018f, 0.016f, 0.027f));
        }
    }

    private static void Bar(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(obj, "Crear soporte/visual");
        obj.name = name; obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = size;
        // Storage handles support; no hook collisions against centimetric cable segments.
        Undo.DestroyObjectImmediate(obj.GetComponent<Collider>());
    }

    private static GameObject Create(string name, Transform parent)
    {
        var obj = new GameObject(name); Undo.RegisterCreatedObjectUndo(obj, "Crear infraestructura");
        if (parent != null) obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void Set(UnityEngine.Object obj, string field, object value)
    {
        var so = new SerializedObject(obj); var p = so.FindProperty(field);
        if (value is bool boolean) p.boolValue = boolean; else p.intValue = (int)value;
        so.ApplyModifiedProperties();
    }

    private static T[] SceneObjects<T>() where T : Component => EditorSceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
    private static Transform Find(string name) => SceneObjects<Transform>().FirstOrDefault(t => t.name == name);

    private static void ValidateInventory()
    {
        string[] expected = Origins.Concat(Destinations).Append("SW1/Console").ToArray();
        var ports = SceneObjects<NetworkPort>().Where(p => p.gameObject.activeInHierarchy).ToArray();
        if (ports.Length != 11 || expected.Any(id => ports.Count(p => p.Address == id) != 1))
            throw new InvalidOperationException("Se requieren exactamente 11 puertos activos, sin IDs repetidos.");
        var devices = SceneObjects<DeviceIdentity>().Where(d => d.gameObject.activeInHierarchy).ToArray();
        if (new[] { "SW1", "PP-A", "FW1", "PDU-A" }.Any(id => devices.Count(d => d.DeviceId == id) != 1))
            throw new InvalidOperationException("Faltan IDs de dispositivos o están duplicados.");
        var cables = SceneObjects<PatchCableLink>().Where(c => c.gameObject.activeInHierarchy).ToArray();
        if (cables.Length != 5 || cables.Count(c => c.Kind == NetworkPortKind.Power) != 1 || cables.Count(c => c.Kind == NetworkPortKind.EthernetRj45) != 4 || cables.Any(c => c.GetComponent<CableHookStorage>() == null))
            throw new InvalidOperationException("Se requieren 1 cable eléctrico y 4 Ethernet, todos con CableHookStorage.");
        if (SceneObjects<Transform>().Count(t => t.name.StartsWith("Hook_C") && t.gameObject.activeInHierarchy) != 5)
            throw new InvalidOperationException("Se requieren cinco ganchos.");
    }
}
#endif
