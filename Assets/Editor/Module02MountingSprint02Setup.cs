#if UNITY_EDITOR
using System;
using System.Linq;
using Framework.Interaction.Tools;
using GameData.Modules;
using Modules.Module02_RackInstallation.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class Module02MountingSprint02Setup
{
    private const string ToolPath = "Assets/Prefabs/Tools/Module02/Screwdriver_Module02.prefab";
    private const string DefinitionPath = "Assets/GameData/Modules/Module02_RackInstallation.asset";

    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 2 - Montaje y destornillador")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play Mode.");
        var slots = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<RackInsertionSlot>(true)).ToArray();
        if (slots.Length != 1) throw new InvalidOperationException("Se necesita una única RackInsertionSlot en Modulo2.");
        var slot = slots[0];
        var slotData = new SerializedObject(slot);
        var entry = slotData.FindProperty("entryPose").objectReferenceValue as Transform;
        var destination = slotData.FindProperty("installedPose").objectReferenceValue as Transform;
        if (entry == null || destination == null || Vector3.Distance(entry.position, destination.position) < 0.001f)
            throw new InvalidOperationException("Asigna EntryPose e InstalledPose con un recorrido no nulo.");
        var grabs = new System.Collections.Generic.List<XRGrabInteractable>();
        if (slot.AcceptedGrab != null) grabs.Add(slot.AcceptedGrab);
        var additional = slotData.FindProperty("additionalAcceptedGrabs");
        for (int i = 0; i < additional.arraySize; i++)
        {
            var grab = additional.GetArrayElementAtIndex(i).objectReferenceValue as XRGrabInteractable;
            if (grab != null && !grabs.Contains(grab)) grabs.Add(grab);
        }
        if (grabs.Count == 0 || grabs.Any(g => g.GetComponent<RackInsertionGrabTransformer>() == null || g.GetComponent<Rigidbody>() == null))
            throw new InvalidOperationException("Cada switch aceptado requiere Rigidbody y RackInsertionGrabTransformer. Configura primero la inserción.");
        var definition = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(DefinitionPath);
        if (definition == null) throw new InvalidOperationException("Falta ModuleDefinition de módulo 2.");
        var toolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ToolPath);
        if (toolPrefab == null) toolPrefab = BuildTool();

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Sprint 2: montaje y tornillos");
        foreach (var grab in grabs)
        {
            Undo.RecordObject(grab, "Estabilizar agarre");
            grab.throwOnDetach = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(grab);
            var body = grab.GetComponent<Rigidbody>();
            Undo.RecordObject(body, "Amortiguar rotación libre");
            body.angularDamping = Mathf.Max(body.angularDamping, 2f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            if (grab.GetComponentInChildren<RackFasteningAssembly>(true) != null) continue;
            var root = New("Fastening_Sprint02", grab.transform);
            // Work in metres even when the imported mesh's root is scaled.
            Vector3 scale = grab.transform.lossyScale;
            root.transform.localScale = new Vector3(1 / Mathf.Abs(scale.x), 1 / Mathf.Abs(scale.y), 1 / Mathf.Abs(scale.z));
            var assembly = Undo.AddComponent<RackFasteningAssembly>(root);
            Set(assembly, "slot", slot); Set(assembly, "device", grab);
            // Initial positions on a 19-inch front. These remain editable for each model.
            var left = Screw(root.transform, "LeftTab", new Vector3(-0.235f, 0, -0.18f), assembly);
            var right = Screw(root.transform, "RightTab", new Vector3(0.235f, 0, -0.18f), assembly);
            Set(assembly, "leftScrew", left); Set(assembly, "rightScrew", right);
        }
        Undo.RecordObject(definition, "Registrar destornillador en rueda");
        var data = new SerializedObject(definition);
        var tools = data.FindProperty("availableTools");
        if (tools.arraySize == 0) tools.arraySize = 1;
        var first = tools.GetArrayElementAtIndex(0);
        first.FindPropertyRelative("name").stringValue = "Destornillador";
        first.FindPropertyRelative("prefab").objectReferenceValue = toolPrefab;
        var icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Layer Lab/2D Icons-PictoIconPack01/Icons/PictoIcon_128/Icon_PictoIcon_Screwdriver.Png");
        if (icon != null) first.FindPropertyRelative("icon").objectReferenceValue = icon;
        data.ApplyModifiedProperties();
        EditorUtility.SetDirty(definition);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = grabs[0].GetComponentInChildren<RackFasteningAssembly>(true).gameObject;
        Debug.Log("Sprint 2 configurado. Ajusta LeftTab/RightTab sobre las pestañas de cada switch y guarda la escena. Local Insertion Euler = 0 usa +Z como dirección de entrada. La rueda equipa el destornillador en su primer espacio.", slot);
    }

    private static GameObject BuildTool()
    {
        var root = new GameObject("Screwdriver_Module02");
        try
        {
            var tool = root.AddComponent<Framework.Interaction.Tools.Tool>();
            var serialized = new SerializedObject(tool);
            serialized.FindProperty("type").intValue = (int)ToolType.Screwdriver;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var body = root.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            Cylinder(root.transform, "Handle", new Vector3(0, 0, 0.025f), 0.028f, 0.08f);
            Cylinder(root.transform, "Shaft", new Vector3(0, 0, 0.115f), 0.005f, 0.1f);
            var tip = new GameObject("Tip"); tip.transform.SetParent(root.transform, false);
            tip.transform.localPosition = new Vector3(0, 0, 0.17f);
            var trigger = tip.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 0.007f;
            tip.AddComponent<ScrewdriverTip>();
            // Simple procedural geometry keeps this tool immediately usable; visual can be replaced.
            return PrefabUtility.SaveAsPrefabAsset(root, ToolPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static RackScrewZone Screw(Transform parent, string name, Vector3 position, RackFasteningAssembly assembly)
    {
        var obj = New(name, parent); obj.transform.localPosition = position;
        var trigger = Undo.AddComponent<SphereCollider>(obj); trigger.isTrigger = true; trigger.radius = 0.022f;
        var zone = Undo.AddComponent<RackScrewZone>(obj);
        var visual = Cylinder(obj.transform, "ScrewHead", Vector3.zero, 0.012f, 0.004f);
        Set(zone, "assembly", assembly); Set(zone, "screwVisual", visual);
        var labelRoot = new GameObject("Progress", typeof(RectTransform), typeof(Canvas));
        labelRoot.transform.SetParent(obj.transform, false);
        labelRoot.transform.localPosition = new Vector3(0, 0.026f, -0.01f);
        labelRoot.transform.localScale = Vector3.one * 0.001f;
        labelRoot.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        ((RectTransform)labelRoot.transform).sizeDelta = new Vector2(65, 25);
        var textRoot = new GameObject("Text", typeof(RectTransform)); textRoot.transform.SetParent(labelRoot.transform, false);
        var text = textRoot.AddComponent<TextMeshProUGUI>(); text.text = "2 s"; text.fontSize = 16;
        text.color = Color.white; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        ((RectTransform)textRoot.transform).sizeDelta = new Vector2(65, 25);
        Set(zone, "statusLabel", text);
        return zone;
    }

    private static Renderer Cylinder(Transform parent, string name, Vector3 position, float diameter, float length)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name = name;
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        obj.transform.localScale = new Vector3(diameter, length / 2, diameter);
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj.GetComponent<Renderer>();
    }
    private static GameObject New(string name, Transform parent)
    {
        var obj = new GameObject(name); obj.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(obj, "Crear fijación"); return obj;
    }
    private static void Set(UnityEngine.Object obj, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(obj); data.FindProperty(field).objectReferenceValue = value;
        data.ApplyModifiedProperties();
    }
}
#endif
