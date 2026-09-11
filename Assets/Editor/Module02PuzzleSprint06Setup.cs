#if UNITY_EDITOR
using System;
using System.Linq;
using Modules.Module02_RackInstallation.Interaction;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Module02PuzzleSprint06Setup
{
    private const string TerminalPath = "Assets/Prefabs/Tools/Module02/ConsoleTerminal_Module02.prefab";
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 6 - Puzzle de terminal")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play.");
        var roots = scene.GetRootGameObjects();
        var powers = roots.SelectMany(r => r.GetComponentsInChildren<Module02SwitchPower>()).Where(p => p.DeviceId == "SW1").ToArray();
        var ports = roots.SelectMany(r => r.GetComponentsInChildren<NetworkPort>()).Where(p => p.DeviceId == "SW1" && p.Kind == NetworkPortKind.EthernetRj45).ToArray();
        var cables = roots.SelectMany(r => r.GetComponentsInChildren<PatchCableLink>()).Where(c => c.isActiveAndEnabled).ToArray();
        if (powers.Length != 1 || ports.Length != 4 || cables.Length != 5 || AssetDatabase.LoadAssetAtPath<GameObject>(TerminalPath) == null)
            throw new InvalidOperationException("Configura los sprints anteriores: SW1, cuatro puertos Ethernet, cinco cables y prefab de consola.");

        // Solo se edita la consola existente: conserva su socket, cable y anclaje de mano.
        var terminalRoot = PrefabUtility.LoadPrefabContents(TerminalPath);
        try
        {
            var terminal = terminalRoot.GetComponent<ConsoleTerminalTool>();
            var screen = terminalRoot.transform.Find("Screen") as RectTransform;
            if (terminal == null || screen == null) throw new InvalidOperationException("La consola necesita ConsoleTerminalTool y Screen.");
            screen.sizeDelta = new Vector2(520, 420); screen.localScale = Vector3.one * 0.0006f;
            var background = screen.GetComponent<UnityEngine.UI.Image>(); if (background != null) background.color = Color.black;
            var body = terminalRoot.transform.Find("DeviceBody"); if (body != null) body.localScale = new Vector3(0.335f, 0.275f, 0.025f);
            var status = new SerializedObject(terminal).FindProperty("screen").objectReferenceValue as TMP_Text;
            if (status != null)
            {
                status.rectTransform.anchoredPosition = new Vector2(0, 175); status.rectTransform.sizeDelta = new Vector2(490, 50);
                status.fontSize = 19; status.color = new Color(0.25f, 1f, 0.5f);
            }
            var view = terminalRoot.GetComponent<TerminalPuzzleView>() ?? terminalRoot.AddComponent<TerminalPuzzleView>();
            Set(view, "terminal", terminal); Set(view, "screen", screen);
            PrefabUtility.SaveAsPrefabAsset(terminalRoot, TerminalPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(terminalRoot); }

        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Sprint 6: configuración y enlaces");
        var configuration = powers[0].GetComponent<Module02SwitchConfiguration>() ?? Undo.AddComponent<Module02SwitchConfiguration>(powers[0].gameObject);
        var data = new SerializedObject(configuration);
        var references = data.FindProperty("cables"); references.arraySize = cables.Length;
        for (int i = 0; i < cables.Length; i++)
        {
            references.GetArrayElementAtIndex(i).objectReferenceValue = cables[i];
            var traffic = cables[i].GetComponent<CableTrafficVisualizer>();
            if (traffic == null && cables[i].Kind == NetworkPortKind.EthernetRj45) traffic = Undo.AddComponent<CableTrafficVisualizer>(cables[i].gameObject);
            if (traffic != null)
            {
                var settings = new SerializedObject(traffic); settings.FindProperty("emitDemoTraffic").boolValue = false; settings.ApplyModifiedProperties();
            }
        }
        references = data.FindProperty("switchPorts"); references.arraySize = ports.Length;
        for (int i = 0; i < ports.Length; i++)
        {
            references.GetArrayElementAtIndex(i).objectReferenceValue = ports[i];
            var portData = new SerializedObject(ports[i]);
            portData.FindProperty("indicatePhysicalLink").boolValue = false;
            // Si el modelo no tiene un renderer asignado, crear un LED provisional editable.
            if (portData.FindProperty("linkLed").objectReferenceValue == null)
            {
                var led = GameObject.CreatePrimitive(PrimitiveType.Cube); Undo.RegisterCreatedObjectUndo(led, "Crear LED de enlace");
                led.name = "OperationalLED"; led.transform.SetParent(ports[i].transform, false);
                led.transform.localPosition = new Vector3(0.015f, 0.012f, -0.005f); led.transform.localScale = Vector3.one * 0.004f;
                UnityEngine.Object.DestroyImmediate(led.GetComponent<Collider>());
                portData.FindProperty("linkLed").objectReferenceValue = led.GetComponent<Renderer>();
            }
            portData.ApplyModifiedProperties();
        }
        data.ApplyModifiedProperties(); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(group); Selection.activeGameObject = configuration.gameObject;
        Debug.Log("Sprint 6 listo para probar: consola negra/verde, cuatro bloques y tráfico por enlace. Ajusta los OperationalLED y guarda la escena.");
    }
    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedProperties();
    }
}
#endif
