#if UNITY_EDITOR
using System;
using System.Linq;
using Modules.Module02_RackInstallation.Interaction;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class Module02ConnectionsSprint03Setup
{
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 3 - Validar cinco conexiones")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play Mode.");
        var roots = scene.GetRootGameObjects();
        var ports = roots.SelectMany(r => r.GetComponentsInChildren<NetworkPort>()).Where(p => p.isActiveAndEnabled).ToArray();
        var cables = roots.SelectMany(r => r.GetComponentsInChildren<PatchCableLink>()).Where(c => c.isActiveAndEnabled).ToArray();
        var plan = Module02ConnectionPlan.Links;
        var expected = plan.SelectMany(l => new[] { l.Origin, l.Destination }).Append("SW1/Console").ToArray();
        if (ports.Length != expected.Length || expected.Any(id => ports.Count(p => p.Address == id) != 1))
            throw new InvalidOperationException("Se requieren los 11 puertos del sprint 1 con IDs únicos. No se modificarán ni regenerarán tus sockets.");
        if (cables.Length != 5 || cables.Count(c => c.Kind == NetworkPortKind.Power) != 1 || cables.Count(c => c.Kind == NetworkPortKind.EthernetRj45) != 4)
            throw new InvalidOperationException("Se requieren 5 cables activos: 1 Power y 4 EthernetRj45.");
        foreach (var cable in cables)
            foreach (string endName in new[] { "Start", "End" })
            {
                var end = cable.transform.Find(endName);
                if (end == null || end.GetComponent<Connector>() == null || end.GetComponent<XRGrabInteractable>() == null ||
                    end.GetComponent<PhysicCableCon>() == null || end.GetComponentInChildren<CableEndSocketDetector>() == null)
                    throw new InvalidOperationException($"{cable.name}/{endName} necesita Connector, XRGrabInteractable, PhysicCableCon y CableEndSocketDetector.");
            }
        foreach (var port in ports)
            if (port.Socket == null || port.Socket.GetComponent<Rigidbody>() == null)
                throw new InvalidOperationException($"{port.Address}: falta Socket o Rigidbody.");

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Sprint 3: conexiones");
        foreach (var port in ports)
        {
            NetworkPortKind kind = port.Address == "SW1/Console" ? NetworkPortKind.ConsoleRj45 :
                plan.First(l => l.Origin == port.Address || l.Destination == port.Address).Kind;
            Undo.RecordObject(port, "Familia del puerto");
            port.Configure(port.DeviceId, port.PortId, kind);
            PrefabUtility.RecordPrefabInstancePropertyModifications(port);
            var body = port.Socket.GetComponent<Rigidbody>();
            Undo.RecordObject(body, "Anclaje del socket"); body.isKinematic = true; body.useGravity = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            var settings = new SerializedObject(port.Socket);
            settings.FindProperty("makeConnectionKinematic").boolValue = true;
            settings.ApplyModifiedProperties();
        }
        foreach (var cable in cables)
        {
            var traffic = cable.GetComponent<CableTrafficVisualizer>();
            if (traffic == null) continue;
            var settings = new SerializedObject(traffic);
            settings.FindProperty("emitDemoTraffic").boolValue = false;
            settings.ApplyModifiedProperties();
        }
        // The current sprint reports state only. The final flow will consume it later.
        foreach (var legacy in roots.SelectMany(r => r.GetComponentsInChildren<Module02CablingObjective>(true)))
        { Undo.RecordObject(legacy, "Desacoplar objetivo antiguo"); legacy.enabled = false; }
        var states = roots.SelectMany(r => r.GetComponentsInChildren<Module02CablingState>(true)).ToArray();
        if (states.Length > 1) { Undo.RevertAllDownToGroup(group); throw new InvalidOperationException("Hay más de un Module02CablingState."); }
        var state = states.FirstOrDefault();
        if (state == null)
        {
            var obj = new GameObject("Cabling_Sprint03"); Undo.RegisterCreatedObjectUndo(obj, "Crear estado de conexiones");
            state = Undo.AddComponent<Module02CablingState>(obj);
            var hooks = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "CableHooks");
            obj.transform.position = hooks != null ? hooks.position + hooks.up * 0.5f : new Vector3(0, 1.6f, 0);
            if (hooks != null) obj.transform.rotation = hooks.rotation;
        }
        var serialized = new SerializedObject(state);
        var list = serialized.FindProperty("cables"); list.arraySize = cables.Length;
        for (int i = 0; i < cables.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = cables[i];
        if (serialized.FindProperty("statusText").objectReferenceValue == null)
        {
            var panel = new GameObject("ConnectionTable", typeof(RectTransform), typeof(Canvas));
            Undo.RegisterCreatedObjectUndo(panel, "Crear tabla"); panel.transform.SetParent(state.transform, false);
            panel.transform.localScale = Vector3.one * 0.001f;
            panel.transform.localRotation = Quaternion.Euler(0, 180, 0);
            panel.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            ((RectTransform)panel.transform).sizeDelta = new Vector2(650, 330);
            var textObj = new GameObject("Text", typeof(RectTransform)); textObj.transform.SetParent(panel.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>(); text.fontSize = 23; text.color = Color.white; text.raycastTarget = false;
            ((RectTransform)textObj.transform).sizeDelta = new Vector2(650, 330);
            text.text = "Conexiones correctas: 0/5";
            serialized.FindProperty("statusText").objectReferenceValue = text;
        }
        serialized.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = state.gameObject;
        Debug.Log("Sprint 3 configurado: cinco enlaces y tabla de estado. Ajusta la tabla hacia el usuario y guarda la escena. Objetivos desacoplados; transmisión de demo apagada.", state);
    }
}
#endif
