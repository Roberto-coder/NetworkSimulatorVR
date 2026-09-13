#if UNITY_EDITOR
using System;
using System.Linq;
using Framework.Interaction.Tools;
using GameData.Modules;
using Modules.Module02_RackInstallation.Interaction;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Module02LabelingSprint04Setup
{
    private const string ToolPath = "Assets/Prefabs/Tools/Module02/LabelMaker_Module02.prefab";
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 4 - Etiquetado")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play Mode.");
        var cables = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PatchCableLink>())
            .Where(c => c.isActiveAndEnabled).ToArray();
        if (cables.Length != 5 || cables.Any(c => c.transform.Find("Start") == null || c.transform.Find("End") == null))
            throw new InvalidOperationException("Se necesitan cinco cables activos con Start y End del sprint 1.");
        var definition = AssetDatabase.LoadAssetAtPath<ModuleDefinition>("Assets/GameData/Modules/Module02_RackInstallation.asset");
        if (definition == null) throw new InvalidOperationException("Falta ModuleDefinition.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ToolPath);
        if (prefab == null) prefab = BuildTool();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Sprint 4: etiquetas");
        foreach (var cable in cables)
        {
            var pair = cable.GetComponent<CableLabelPair>() ?? Undo.AddComponent<CableLabelPair>(cable.gameObject);
            var start = Endpoint(cable.transform.Find("Start"), pair);
            var end = Endpoint(cable.transform.Find("End"), pair);
            Set(pair, "startLabel", start); Set(pair, "endLabel", end);
        }
        var monitor = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Module02LabelingState>(true)).FirstOrDefault();
        if (monitor == null)
        {
            var obj = new GameObject("Labeling_Sprint04"); Undo.RegisterCreatedObjectUndo(obj, "Crear estado de etiquetado");
            monitor = Undo.AddComponent<Module02LabelingState>(obj);
        }
        var monitorData = new SerializedObject(monitor);
        var pairs = monitorData.FindProperty("cables"); pairs.arraySize = cables.Length;
        for (int i = 0; i < cables.Length; i++) pairs.GetArrayElementAtIndex(i).objectReferenceValue = cables[i].GetComponent<CableLabelPair>();
        monitorData.ApplyModifiedProperties();
        var data = new SerializedObject(definition);
        var tools = data.FindProperty("availableTools");
        if (tools.arraySize < 2) tools.arraySize = 2;
        var second = tools.GetArrayElementAtIndex(1);
        second.FindPropertyRelative("name").stringValue = "Etiquetadora";
        second.FindPropertyRelative("prefab").objectReferenceValue = prefab;
        second.FindPropertyRelative("icon").objectReferenceValue = null;
        data.ApplyModifiedProperties();
        EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = cables[0].gameObject;
        Debug.Log("Sprint 4: diez etiquetas y etiquetadora en el segundo espacio. Ajusta LabelVisual sobre cada extremo y guarda la escena. Trigger derecho para etiquetar.");
    }
    private static CableEndpointLabel Endpoint(Transform end, CableLabelPair owner)
    {
        var label = end.GetComponent<CableEndpointLabel>();
        if (label != null) { Set(label, "owner", owner); return label; }
        label = Undo.AddComponent<CableEndpointLabel>(end.gameObject);
        var text = Text(end, "LabelVisual", new Vector3(0.034f, 0.022f, 0), new Vector2(85, 25), 10);
        text.text = "Origen\n↔ Destino";
        Set(label, "owner", owner); Set(label, "text", text); Set(label, "visual", text.transform.parent.gameObject);
        text.transform.parent.gameObject.SetActive(false);
        return label;
    }
    private static GameObject BuildTool()
    {
        var root = new GameObject("LabelMaker_Module02");
        try
        {
            var identity = root.AddComponent<Framework.Interaction.Tools.Tool>();
            var type = new SerializedObject(identity); type.FindProperty("type").intValue = (int)ToolType.LabelMaker;
            type.ApplyModifiedPropertiesWithoutUndo();
            var tool = root.AddComponent<LabelMakerTool>();
            Cube(root.transform, "Body", new Vector3(0, 0, 0.06f), new Vector3(0.085f, 0.055f, 0.13f));
            Cube(root.transform, "NozzleVisual", new Vector3(0, 0, 0.14f), new Vector3(0.04f, 0.02f, 0.035f));
            var nozzle = new GameObject("Nozzle"); nozzle.transform.SetParent(root.transform, false);
            nozzle.transform.localPosition = new Vector3(0, 0, 0.165f);
            Set(tool, "nozzle", nozzle.transform);
            var text = Text(root.transform, "Display", new Vector3(0, 0.04f, 0.05f), new Vector2(95, 35), 10);
            text.text = "Acerca y presiona\ntrigger derecho"; Set(tool, "feedback", text);
            return PrefabUtility.SaveAsPrefabAsset(root, ToolPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    private static void Cube(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = size;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
    }
    private static TMP_Text Text(Transform parent, string name, Vector3 position, Vector2 size, float fontSize)
    {
        var canvas = new GameObject(name, typeof(RectTransform), typeof(Canvas));
        if (parent.gameObject.scene.IsValid()) Undo.RegisterCreatedObjectUndo(canvas, "Crear etiqueta");
        canvas.transform.SetParent(parent, false); canvas.transform.localPosition = position;
        canvas.transform.localScale = Vector3.one * 0.001f;
        ((RectTransform)canvas.transform).sizeDelta = size;
        canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var background = canvas.AddComponent<UnityEngine.UI.Image>(); background.color = Color.white; background.raycastTarget = false;
        var obj = new GameObject("Text", typeof(RectTransform)); obj.transform.SetParent(canvas.transform, false);
        ((RectTransform)obj.transform).sizeDelta = size - new Vector2(4, 2);
        var text = obj.AddComponent<TextMeshProUGUI>(); text.color = Color.black; text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        return text;
    }
    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedProperties();
    }
}
#endif
