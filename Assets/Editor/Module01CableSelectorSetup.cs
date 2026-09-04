using Modules.Module01_CableMaking.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Module01CableSelectorSetup
{
    private const string ScenePath = "Assets/Scenes/Modulo1.unity";
    private static readonly Color Navy = new(0.025f, 0.065f, 0.13f, 0.98f);
    private static readonly Color Cyan = new(0.08f, 0.82f, 0.96f, 1f);
    private static readonly Color Yellow = new(1f, 0.75f, 0.08f, 1f);

    [InitializeOnLoadMethod]
    private static void BuildWhenModuleSceneIsAlreadyOpen()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != ScenePath ||
                GameObject.Find("CableSelectionExperience") != null)
                return;

            Build();
        };
    }

    [MenuItem("Tools/Network Simulator/Build Module 1 Cable Selector")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject old = GameObject.Find("CableSelectionExperience");
        if (old != null) Object.DestroyImmediate(old);

        GameObject cable = GameObject.Find("NormalCable");
        if (cable == null)
            throw new MissingReferenceException("No se encontro la instancia NormalCable en Modulo1.");

        cable.SetActive(false);

        GameObject experience = new("CableSelectionExperience");
        Undo.RegisterCreatedObjectUndo(experience, "Create cable selector");
        // Junto al punto de inicio, orientado hacia el area de trabajo.
        experience.transform.SetPositionAndRotation(new Vector3(-3.7f, 0f, 5.4f), Quaternion.Euler(0f, 22f, 0f));

        BuildPedestal(experience.transform);
        Canvas canvas = BuildCanvas(experience.transform);
        RectTransform panel = Panel(canvas.transform, "Panel", Vector2.zero, new Vector2(980, 650), Navy, new Vector2(0.5f, 0.5f));
        AddOutline(panel.gameObject, Cyan, new Vector2(7, -7));

        Text(panel, "Title", "CONFIGURACION DEL CABLE", new Vector2(0, 260), new Vector2(850, 70), 46, FontStyles.Bold, Color.white);
        TMP_Text description = Text(panel, "Description", "", new Vector2(0, 160), new Vector2(650, 86), 23, FontStyles.Normal, new Color(0.78f, 0.9f, 0.95f));

        RectTransform card = Panel(panel, "CableCard", new Vector2(0, -20), new Vector2(510, 270), new Color(0.055f, 0.12f, 0.21f, 1f), new Vector2(0.5f, 0.5f));
        AddOutline(card.gameObject, new Color(0.22f, 0.75f, 0.9f), new Vector2(3, -3));
        Image preview = Panel(card, "CablePreview", new Vector2(0, 35), new Vector2(310, 76), Cyan, new Vector2(0.5f, 0.5f)).GetComponent<Image>();
        AddCableLines(preview.rectTransform);
        TMP_Text name = Text(card, "CableName", "", new Vector2(0, -82), new Vector2(430, 62), 38, FontStyles.Bold, Color.white);

        Button previous = Button(panel, "Previous", "<", new Vector2(-365, -15), new Vector2(105, 105), Yellow, Color.black);
        Button next = Button(panel, "Next", ">", new Vector2(365, -15), new Vector2(105, 105), Yellow, Color.black);
        Button select = Button(panel, "Select", "SELECCIONAR", new Vector2(0, -245), new Vector2(360, 92), Yellow, Color.black);

        CableSelectionPanel controller = experience.AddComponent<CableSelectionPanel>();
        controller.Configure(cable, canvas.gameObject, name, description, preview, previous, next, select,
            new[]
            {
                Option("Cat 5e", "Hasta 1 Gbps · 100 MHz\nOpcion confiable para redes basicas.", new Color(0.2f, 0.72f, 0.38f)),
                Option("Cat 6a", "Hasta 10 Gbps · 500 MHz\nMejor proteccion contra interferencias.", new Color(0.08f, 0.82f, 0.96f)),
                Option("Cat 7", "Hasta 10 Gbps · 600 MHz\nBlindaje reforzado para entornos exigentes.", new Color(0.55f, 0.32f, 0.9f))
            });

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = experience;
        Debug.Log("Selector de cable creado en Modulo1.");
    }

    public static void BuildBatch() => Build();

    private static CableSelectionPanel.CableOption Option(string name, string description, Color color) =>
        new() { displayName = name, description = description, accentColor = color };

    private static void BuildPedestal(Transform parent)
    {
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObject.name = "PedestalBase";
        baseObject.transform.SetParent(parent, false);
        baseObject.transform.localPosition = new Vector3(0, 0.5f, 0);
        baseObject.transform.localScale = new Vector3(1.15f, 0.5f, 0.75f);
        baseObject.GetComponent<Renderer>().sharedMaterial = Material("PedestalNavy", new Color(0.035f, 0.09f, 0.16f), 0.25f);

        GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trim.name = "PedestalTrim";
        trim.transform.SetParent(parent, false);
        trim.transform.localPosition = new Vector3(0, 1.02f, 0);
        trim.transform.localScale = new Vector3(1.25f, 0.08f, 0.82f);
        trim.GetComponent<Renderer>().sharedMaterial = Material("PedestalCyan", Cyan, 0.65f);
    }

    private static Canvas BuildCanvas(Transform parent)
    {
        GameObject go = new("CableSelectionCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0, 2.05f, 0);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.0018f;
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 20;
        RectTransform rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(1100, 720);
        return canvas;
    }

    private static RectTransform Panel(Transform parent, string name, Vector2 position, Vector2 size, Color color, Vector2 pivot)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = pivot;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return rect;
    }

    private static TMP_Text Text(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, FontStyles style, Color color)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = size;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = value; text.fontSize = fontSize; text.fontStyle = style; text.color = color;
        text.alignment = TextAlignmentOptions.Center; text.enableWordWrapping = true;
        return text;
    }

    private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size, Color background, Color foreground)
    {
        RectTransform rect = Panel(parent, name, position, size, background, new Vector2(0.5f, 0.5f));
        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors; colors.highlightedColor = Color.Lerp(background, Color.white, 0.3f); colors.pressedColor = Color.Lerp(background, Color.black, 0.2f); button.colors = colors;
        Text(rect, "Label", label, Vector2.zero, size - new Vector2(16, 12), name == "Select" ? 31 : 54, FontStyles.Bold, foreground);
        return button;
    }

    private static void AddCableLines(RectTransform parent)
    {
        Color[] colors = { Color.white, new(1f, .45f, .08f), Color.white, new(.1f, .55f, 1f), Color.white, new(.2f, .8f, .35f) };
        for (int i = 0; i < colors.Length; i++)
            Panel(parent, $"Wire_{i + 1}", new Vector2(128, 22 - i * 9), new Vector2(145, 5), colors[i], new Vector2(0.5f, 0.5f)).localRotation = Quaternion.Euler(0, 0, (i - 2.5f) * -4f);
    }

    private static void AddOutline(GameObject go, Color color, Vector2 distance)
    {
        UnityEngine.UI.Outline outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private static Material Material(string name, Color color, float metallic)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        System.IO.Directory.CreateDirectory("Assets/Materials");
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}
