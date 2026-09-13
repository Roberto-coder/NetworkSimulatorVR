#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Modules.Module02_RackInstallation.Data;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class Module02ExplorationSprint01Setup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";
    private const string DataFolder = "Assets/GameData/Module02/Components";

    private readonly struct InfoSpec
    {
        public readonly string Id, Name, Description, Function, Standard;
        public readonly RackComponentCategory Category;
        public readonly int RackUnits;
        public InfoSpec(string id, string name, RackComponentCategory category, string description,
            string function, int rackUnits = 0, string standard = "")
        {
            Id = id; Name = name; Category = category; Description = description;
            Function = function; RackUnits = rackUnits; Standard = standard;
        }
    }

    private static readonly InfoSpec[] Specs =
    {
        new("rack_22u", "Rack educativo de 22U", RackComponentCategory.Rack,
            "Estructura destinada a organizar y proteger equipos de telecomunicaciones.",
            "Mantiene los dispositivos montados, ordenados y accesibles. Una unidad U equivale a 44.45 mm de altura.", 22,
            "IEC 60297 (dimensiones mecánicas de estructuras de 19 pulgadas)"),
        new("switch_overview", "Switch educativo de 12 puertos", RackComponentCategory.Switch,
            "Modelo simplificado inspirado en switches Cisco; no representa un producto exacto.",
            "Interconecta dispositivos de una red local y reenvía tramas hacia el puerto correspondiente.", 1),
        new("switch_ports", "Puertos Ethernet", RackComponentCategory.Ports,
            "Conectores utilizados para enlazar equipos mediante cableado Ethernet.",
            "Cada puerto proporciona un punto de acceso a la red y puede configurarse según su función."),
        new("switch_power_input", "Entrada de alimentación", RackComponentCategory.Power,
            "Punto de conexión eléctrica del switch.",
            "Recibe la alimentación requerida por el dispositivo. Debe conectarse siguiendo las indicaciones del fabricante."),
        new("switch_power_button", "Interruptor de encendido", RackComponentCategory.Power,
            "Control físico simplificado para encender o apagar el equipo educativo.",
            "Permite iniciar o interrumpir la alimentación del modelo durante la práctica."),
        new("switch_leds", "Indicadores LED", RackComponentCategory.Indicators,
            "Indicadores visuales asociados al estado de alimentación y de los puertos.",
            "Ayudan a identificar energía, actividad de enlace y posibles estados del dispositivo.")
    };

    [MenuItem("Network Simulator/Module 02/Sprint 1 - Configurar exploración con tarjetas")]
    public static void Configure()
    {
        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("No se encontró Modulo2.unity.", ScenePath);
        EnsureFolder(DataFolder);
        Dictionary<string, RackComponentInfo> data = CreateData();
        AssetDatabase.SaveAssets();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject moduleRoot = GameObject.Find("_Module02") ?? new GameObject("_Module02");
        Transform exploration = FindOrCreate("ExplorationSystem", moduleRoot.transform);
        InfoFocusDetector detector = GetOrAdd<InfoFocusDetector>(exploration.gameObject);
        GameObject card = ConfigureCard(exploration, detector);

        ConfigureFirstTargets(data);
        Selection.activeGameObject = card;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Sprint 1 configurado. Apunta 1.5 segundos al rack o a una parte del switch para abrir su tarjeta.");
    }

    private static Dictionary<string, RackComponentInfo> CreateData()
    {
        Dictionary<string, RackComponentInfo> result = new();
        foreach (InfoSpec spec in Specs)
        {
            string path = $"{DataFolder}/{spec.Id}.asset";
            RackComponentInfo info = AssetDatabase.LoadAssetAtPath<RackComponentInfo>(path);
            if (info == null)
            {
                info = ScriptableObject.CreateInstance<RackComponentInfo>();
                AssetDatabase.CreateAsset(info, path);
            }
            SerializedObject serialized = new(info);
            serialized.FindProperty("id").stringValue = spec.Id;
            serialized.FindProperty("displayName").stringValue = spec.Name;
            serialized.FindProperty("category").enumValueIndex = (int)spec.Category;
            serialized.FindProperty("educationalApproximation").boolValue = true;
            serialized.FindProperty("shortDescription").stringValue = spec.Description;
            serialized.FindProperty("function").stringValue = spec.Function;
            serialized.FindProperty("rackUnits").intValue = spec.RackUnits;
            serialized.FindProperty("standardReference").stringValue = spec.Standard;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(info);
            result.Add(spec.Id, info);
        }
        return result;
    }

    private static GameObject ConfigureCard(Transform parent, InfoFocusDetector detector)
    {
        Transform existing = parent.Find("InformationCard_XRI");
        GameObject root = existing != null
            ? existing.gameObject
            : new GameObject("InformationCard_XRI", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        InfoCardController controller = GetOrAdd<InfoCardController>(root);

        Canvas canvas = GetOrAdd<Canvas>(root);
        canvas.renderMode = RenderMode.WorldSpace;
        GetOrAdd<CanvasScaler>(root).dynamicPixelsPerUnit = 10f;
        GraphicRaycaster oldRaycaster = root.GetComponent<GraphicRaycaster>();
        if (oldRaycaster != null) UnityEngine.Object.DestroyImmediate(oldRaycaster);
        GetOrAdd<TrackedDeviceGraphicRaycaster>(root);
        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(720f, 520f);
        canvasRect.localScale = Vector3.one * 0.001f;

        Transform cardPanel = FindOrCreate("CardPanel", root.transform);
        Image background = GetOrAdd<Image>(cardPanel.gameObject);
        background.color = new Color(0.035f, 0.07f, 0.11f, 0.97f);
        Stretch(cardPanel.GetComponent<RectTransform>(), 0f);

        TMP_Text title = CreateText("Title", cardPanel, 36, FontStyles.Bold,
            new Vector2(30, -25), new Vector2(-110, 55));
        TMP_Text category = CreateText("Category", cardPanel, 20, FontStyles.Normal,
            new Vector2(30, -82), new Vector2(-30, 32));
        category.color = new Color(0.25f, 0.8f, 1f);
        TMP_Text description = CreateText("Description", cardPanel, 27, FontStyles.Normal,
            new Vector2(30, -135), new Vector2(-30, 115));
        TMP_Text function = CreateText("Function", cardPanel, 24, FontStyles.Normal,
            new Vector2(30, -265), new Vector2(-30, 100));
        TMP_Text rackUnits = CreateText("RackUnits", cardPanel, 22, FontStyles.Bold,
            new Vector2(30, -375), new Vector2(-30, 32));
        TMP_Text standard = CreateText("Standard", cardPanel, 19, FontStyles.Normal,
            new Vector2(30, -415), new Vector2(-30, 45));
        TMP_Text approximation = CreateText("Approximation", cardPanel, 18, FontStyles.Italic,
            new Vector2(30, -475), new Vector2(-30, 28));
        approximation.color = new Color(1f, 0.78f, 0.25f);

        Button close = CreateButton("CloseButton", cardPanel, "×", new Vector2(-30, -30));

        Transform dwellPanel = FindOrCreate("DwellPanel", root.transform);
        RectTransform dwellRect = dwellPanel.GetComponent<RectTransform>();
        dwellRect.anchorMin = dwellRect.anchorMax = new Vector2(0.5f, 0.5f);
        dwellRect.sizeDelta = new Vector2(90f, 90f);
        dwellRect.anchoredPosition = Vector2.zero;
        Image dwellBackground = GetOrAdd<Image>(dwellPanel.gameObject);
        dwellBackground.color = new Color(0f, 0f, 0f, 0.65f);
        Transform fillObject = FindOrCreate("Fill", dwellPanel);
        Stretch(fillObject.GetComponent<RectTransform>(), 8f);
        Image fill = GetOrAdd<Image>(fillObject.gameObject);
        fill.color = new Color(0.1f, 0.75f, 1f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Radial360;
        fill.fillOrigin = 2;

        InfoCardView view = GetOrAdd<InfoCardView>(root);
        SerializedObject viewData = new(view);
        Set(viewData, "titleText", title); Set(viewData, "categoryText", category);
        Set(viewData, "descriptionText", description); Set(viewData, "functionText", function);
        Set(viewData, "rackUnitsText", rackUnits); Set(viewData, "standardText", standard);
        Set(viewData, "approximationText", approximation); Set(viewData, "dwellFill", fill);
        Set(viewData, "cardPanel", cardPanel.gameObject); Set(viewData, "dwellPanel", dwellPanel.gameObject);
        Set(viewData, "closeButton", close); viewData.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerData = new(controller);
        Set(controllerData, "focusDetector", detector); Set(controllerData, "view", view);
        Set(controllerData, "playerCamera", Camera.main); controllerData.ApplyModifiedPropertiesWithoutUndo();
        cardPanel.gameObject.SetActive(false);
        dwellPanel.gameObject.SetActive(false);
        return root;
    }

    private static void ConfigureFirstTargets(IReadOnlyDictionary<string, RackComponentInfo> data)
    {
        AssignFirst(data["rack_22u"], "TrabajoTerminal_Rack_22U", "rackV1");
        AssignFirst(data["switch_overview"], "SwitchBody");
        AssignAll(data["switch_ports"], "Outputs1R", "Outputs2L");
        AssignFirst(data["switch_power_input"], "Input", "input");
        AssignAll(data["switch_power_button"], "Boton", "Apagador");
        AssignAll(data["switch_leds"], "LightsIN", "LightsRU", "LightsRD", "LightsLU", "LightsLD");
    }

    private static void AssignFirst(RackComponentInfo info, params string[] names)
    {
        foreach (string name in names)
        {
            Transform found = FindTransform(name);
            if (found != null) { ConfigureTarget(found.gameObject, info); return; }
        }
        Debug.LogWarning($"No se encontró un objeto para la tarjeta {info.Id}. Asígnala manualmente cuando el modelo esté listo.");
    }

    private static void AssignAll(RackComponentInfo info, params string[] names)
    {
        bool foundAny = false;
        foreach (string name in names)
        {
            Transform found = FindTransform(name);
            if (found == null) continue;
            ConfigureTarget(found.gameObject, info);
            foundAny = true;
        }
        if (!foundAny) Debug.LogWarning($"No se encontraron objetos para la tarjeta {info.Id}.");
    }

    private static void ConfigureTarget(GameObject target, RackComponentInfo info)
    {
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null) collider = target.AddComponent<BoxCollider>();
        FitCollider(collider, target.GetComponentsInChildren<Renderer>(true));
        GetOrAdd<XRSimpleInteractable>(target);
        RackInfoTarget infoTarget = GetOrAdd<RackInfoTarget>(target);
        SerializedObject serialized = new(infoTarget);
        Set(serialized, "information", info);
        SerializedProperty renderers = serialized.FindProperty("highlightRenderers");
        Renderer[] targets = target.GetComponentsInChildren<Renderer>(true);
        renderers.arraySize = targets.Length;
        for (int i = 0; i < targets.Length; i++) renderers.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void FitCollider(BoxCollider collider, Renderer[] renderers)
    {
        if (renderers.Length == 0) return;
        Bounds localBounds = new(collider.transform.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.bounds;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
                localBounds.Encapsulate(collider.transform.InverseTransformPoint(
                    bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))));
        }
        collider.center = localBounds.center;
        collider.size = localBounds.size;
    }

    private static TMP_Text CreateText(string name, Transform parent, float size, FontStyles style,
        Vector2 position, Vector2 dimensions)
    {
        Transform child = FindOrCreate(name, parent);
        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(child.gameObject);
        text.fontSize = size; text.fontStyle = style; text.color = Color.white;
        text.enableWordWrapping = true; text.raycastTarget = false;
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f); rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 position)
    {
        Transform child = FindOrCreate(name, parent);
        Image image = GetOrAdd<Image>(child.gameObject); image.color = new Color(0.12f, 0.25f, 0.35f);
        Button button = GetOrAdd<Button>(child.gameObject); button.targetGraphic = image;
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = position; rect.sizeDelta = new Vector2(64f, 64f);
        TMP_Text text = CreateText("Label", child, 42, FontStyles.Bold, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform, 0f); text.text = label; text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void Stretch(RectTransform rect, float margin)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * margin; rect.offsetMax = Vector2.one * -margin;
    }

    private static Transform FindTransform(string name)
    {
        foreach (Transform candidate in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (string.Equals(candidate.name, name, StringComparison.OrdinalIgnoreCase) && candidate.gameObject.activeInHierarchy)
                return candidate;
        return null;
    }

    private static Transform FindOrCreate(string name, Transform parent)
    {
        Transform child = parent.Find(name);
        if (child != null) return child;
        GameObject created = new(name, typeof(RectTransform));
        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.TryGetComponent(out T component) ? component : target.AddComponent<T>();

    private static void Set(SerializedObject serialized, string name, UnityEngine.Object value) =>
        serialized.FindProperty(name).objectReferenceValue = value;

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(normalized)) return;
        string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(normalized));
    }
}
#endif
