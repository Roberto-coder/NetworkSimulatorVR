#if UNITY_EDITOR
using System;
using System.Linq;
using Framework.Interaction.Tools;
using GameData.Modules;
using HPhysic;
using Modules.Module02_RackInstallation.Interaction;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class Module02PowerConsoleSprint05Setup
{
    private const string Folder = "Assets/Prefabs/Tools/Module02/";
    private const string CablePath = Folder + "Cabling/ConsoleCable_Integrated.prefab";
    private const string ToolPath = Folder + "ConsoleTerminal_Module02.prefab";
    private const string SocketPath = Folder + "Cabling/NetworkPortSocket_RJ45.prefab";
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 5 - Encendido y consola")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play.");
        var ports = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<NetworkPort>()).Where(p => p.isActiveAndEnabled).ToArray();
        var powerPorts = ports.Where(p => p.Address == "SW1/Power" && p.Kind == NetworkPortKind.Power).ToArray();
        if (powerPorts.Length != 1 || ports.Count(p => p.Address == "SW1/Console" && p.Kind == NetworkPortKind.ConsoleRj45) != 1)
            throw new InvalidOperationException("Se requieren SW1/Power y SW1/Console únicos con sus tipos correctos.");
        var device = powerPorts[0].GetComponentInParent<XRGrabInteractable>();
        if (device == null) throw new InvalidOperationException("SW1/Power debe pertenecer al switch con XRGrabInteractable.");
        var definition = AssetDatabase.LoadAssetAtPath<ModuleDefinition>("Assets/GameData/Modules/Module02_RackInstallation.asset");
        if (definition == null) throw new InvalidOperationException("Falta ModuleDefinition.");
        var tool = AssetDatabase.LoadAssetAtPath<GameObject>(ToolPath);
        if (tool == null) tool = BuildTool();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Sprint 5: encendido y consola");
        var power = device.GetComponentInChildren<Module02SwitchPower>(true);
        if (power == null)
        {
            var root = new GameObject("PowerControls_Sprint05"); Undo.RegisterCreatedObjectUndo(root, "Crear controles");
            root.transform.SetParent(device.transform, false);
            var scale = device.transform.lossyScale;
            root.transform.localScale = new Vector3(1 / Mathf.Abs(scale.x), 1 / Mathf.Abs(scale.y), 1 / Mathf.Abs(scale.z));
            power = Undo.AddComponent<Module02SwitchPower>(root);
            var button = Primitive(root.transform, "PowerButton", new Vector3(0.18f, 0, -0.19f), new Vector3(0.025f, 0.025f, 0.015f), true);
            var simple = Undo.AddComponent<XRSimpleInteractable>(button);
            simple.colliders.Add(button.GetComponent<Collider>());
            var relay = Undo.AddComponent<SwitchPowerButton>(button); Set(relay, "power", power);
            var led = Primitive(root.transform, "PowerLED", new Vector3(0.145f, 0, -0.19f), Vector3.one * 0.008f, false);
            Set(power, "powerLed", led.GetComponent<Renderer>());
        }
        Set(power, "powerPort", powerPorts[0]);
        foreach (var port in ports.Where(p => p.DeviceId == "SW1"))
        {
            var data = new SerializedObject(port); data.FindProperty("indicatePhysicalLink").boolValue = false; data.ApplyModifiedProperties();
        }
        var definitionData = new SerializedObject(definition);
        var tools = definitionData.FindProperty("availableTools"); if (tools.arraySize < 3) tools.arraySize = 3;
        var third = tools.GetArrayElementAtIndex(2); third.FindPropertyRelative("name").stringValue = "Consola";
        third.FindPropertyRelative("prefab").objectReferenceValue = tool;
        third.FindPropertyRelative("icon").objectReferenceValue = null;
        definitionData.ApplyModifiedProperties(); EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = power.gameObject;
        Debug.Log("Sprint 5 configurado. Alinea PowerButton/PowerLED con el modelo y guarda la escena. La consola está en el tercer espacio.");
    }
    private static GameObject BuildTool()
    {
        var socketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SocketPath);
        if (socketPrefab == null) throw new InvalidOperationException("Falta el prefab de socket.");
        var cable = AssetDatabase.LoadAssetAtPath<GameObject>(CablePath);
        if (cable == null)
        {
            var copy = PrefabUtility.LoadPrefabContents(Folder + "Cabling/PatchCable_RJ45_1_7m.prefab");
            try
            {
                copy.name = "ConsoleCable_Integrated";
                var physics = copy.GetComponent<PhysicCable>(); physics.ConfigureDimensions(10, 0.08f, 0.006f); physics.RebuildConfiguredPoints();
                var data = new SerializedObject(copy.GetComponent<PatchCableLink>()); data.FindProperty("kind").intValue = (int)NetworkPortKind.ConsoleRj45; data.ApplyModifiedPropertiesWithoutUndo();
                var traffic = copy.GetComponent<CableTrafficVisualizer>(); if (traffic != null) UnityEngine.Object.DestroyImmediate(traffic);
                var storage = copy.GetComponent<CableHookStorage>(); if (storage != null) UnityEngine.Object.DestroyImmediate(storage);
                cable = PrefabUtility.SaveAsPrefabAsset(copy, CablePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(copy); }
        }
        var root = new GameObject("ConsoleTerminal_Module02");
        try
        {
            var identity = root.AddComponent<Framework.Interaction.Tools.Tool>();
            var data = new SerializedObject(identity); data.FindProperty("type").intValue = (int)ToolType.ConsoleTerminal; data.ApplyModifiedPropertiesWithoutUndo();
            var terminal = root.AddComponent<ConsoleTerminalTool>();
            Primitive(root.transform, "DeviceBody", new Vector3(0, 0, 0.08f), new Vector3(0.22f, 0.15f, 0.025f), false);
            var screenRoot = new GameObject("Screen", typeof(RectTransform), typeof(Canvas)); screenRoot.transform.SetParent(root.transform, false);
            screenRoot.transform.localPosition = new Vector3(0, 0, 0.065f); screenRoot.transform.localScale = Vector3.one * 0.001f;
            ((RectTransform)screenRoot.transform).sizeDelta = new Vector2(200, 130); screenRoot.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var background = screenRoot.AddComponent<UnityEngine.UI.Image>(); background.color = new Color(0.025f, 0.05f, 0.08f); background.raycastTarget = false;
            var textRoot = new GameObject("Status", typeof(RectTransform)); textRoot.transform.SetParent(screenRoot.transform, false);
            ((RectTransform)textRoot.transform).sizeDelta = new Vector2(185, 115);
            var text = textRoot.AddComponent<TextMeshProUGUI>(); text.fontSize = 12; text.alignment = TextAlignmentOptions.Center;
            text.text = "Conecta la consola"; text.raycastTarget = false;
            var socket = (GameObject)PrefabUtility.InstantiatePrefab(socketPrefab, root.transform); socket.name = "IntegratedConsoleSocket";
            socket.transform.localPosition = new Vector3(0.1f, -0.065f, 0.08f);
            var port = socket.GetComponent<NetworkPort>(); port.Configure("TERM1", "Console", NetworkPortKind.ConsoleRj45);
            foreach (var renderer in socket.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            Set(terminal, "integratedSocket", port); Set(terminal, "cablePrefab", cable); Set(terminal, "screen", text);
            return PrefabUtility.SaveAsPrefabAsset(root, ToolPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    private static GameObject Primitive(Transform parent, string name, Vector3 position, Vector3 size, bool collider)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position; obj.transform.localScale = size;
        if (!collider) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedProperties();
    }
}
#endif
