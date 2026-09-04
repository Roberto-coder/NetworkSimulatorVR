using System.Linq;
using Systems.Scenes;
using Systems.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PauseMenuViewController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private string menuSceneName = "Menu";

    private PauseManager pauseManager;
    private Slider musicSlider;
    private bool initialized;

    private void Awake()
    {
        ResolvePanels();
        BuildSecondaryPanelsIfNeeded();
        WireMainButtons();
        ShowMain();
    }

    private void OnEnable()
    {
        ShowMain();
        SyncMusicSlider();
    }

    public void ShowMain()
    {
        ResolvePanels();
        SetVisible(mainPanel);
    }

    public void ShowSettings()
    {
        SetVisible(settingsPanel);
        SyncMusicSlider();
    }

    public void ShowConfirmation() => SetVisible(confirmationPanel);

    public void Resume()
    {
        pauseManager ??= FindFirstObjectByType<PauseManager>();
        pauseManager?.ResumeFromMenu();
    }

    public void ConfirmReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.LoadScene(menuSceneName);
    }

    private void ResolvePanels()
    {
        mainPanel ??= transform.Find("MainPanel")?.gameObject;
        settingsPanel ??= transform.Find("SettingsPanel")?.gameObject;
        confirmationPanel ??= transform.Find("ConfirmationPanel")?.gameObject;
    }

    private void WireMainButtons()
    {
        Button resume = FindButton("ButtonResume");
        Button settings = FindButton("ButtonSettings");
        Button home = FindButton("ButtonHome");

        if (resume != null)
            resume.onClick.AddListener(Resume);
        if (settings != null)
            settings.onClick.AddListener(ShowSettings);
        if (home != null)
            home.onClick.AddListener(ShowConfirmation);
    }

    private Button FindButton(string objectName) =>
        GetComponentsInChildren<Button>(true).FirstOrDefault(button => button.name == objectName);

    private void SetVisible(GameObject selected)
    {
        if (mainPanel != null)
            mainPanel.SetActive(selected == mainPanel);
        if (settingsPanel != null)
            settingsPanel.SetActive(selected == settingsPanel);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(selected == confirmationPanel);
    }

    public void BuildSecondaryPanelsIfNeeded()
    {
        if (initialized || settingsPanel == null || confirmationPanel == null)
            return;

        if (settingsPanel.transform.childCount > 0 && confirmationPanel.transform.childCount > 0)
        {
            musicSlider = settingsPanel.GetComponentInChildren<Slider>(true);
            initialized = true;
            return;
        }
        initialized = true;

        CreateText(settingsPanel.transform, "SettingsTitle", "AJUSTES", 30f, new Vector2(0f, 95f), new Vector2(220f, 45f), FontStyles.Bold);
        CreateText(settingsPanel.transform, "MusicLabel", "Música", 20f, new Vector2(0f, 42f), new Vector2(200f, 35f));
        musicSlider = CreateSlider(settingsPanel.transform, "SliderMusic", new Vector2(0f, 5f));
        CreateButton(settingsPanel.transform, "ButtonSettingsBack", "REGRESAR", new Vector2(0f, -88f), ShowMain);

        CreateText(confirmationPanel.transform, "ConfirmationTitle", "VOLVER AL MENÚ", 28f, new Vector2(0f, 82f), new Vector2(225f, 45f), FontStyles.Bold);
        CreateText(confirmationPanel.transform, "ConfirmationMessage", "¿Quieres abandonar el módulo?\nEl progreso no guardado podría perderse.", 17f, new Vector2(0f, 20f), new Vector2(220f, 75f));
        CreateButton(confirmationPanel.transform, "ButtonConfirmMenu", "CONFIRMAR", new Vector2(0f, -52f), ConfirmReturnToMenu, new Color32(24, 137, 183, 255));
        CreateButton(confirmationPanel.transform, "ButtonCancelMenu", "CANCELAR", new Vector2(0f, -102f), ShowMain);
    }

    private void SyncMusicSlider()
    {
        if (musicSlider == null)
            return;

        GlobalSettingsManager manager = GlobalSettingsManager.Instance;
        musicSlider.interactable = manager != null;
        if (manager != null)
            musicSlider.SetValueWithoutNotify(manager.Current.musicVolume);
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size, Vector2 position, Vector2 dimensions, FontStyles style = FontStyles.Normal)
    {
        GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        SetRect(rect, position, dimensions);
        TMP_Text text = child.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color32(235, 246, 255, 255);
        text.raycastTarget = false;
        return text;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 position)
    {
        GameObject root = new(name, typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        SetRect(root.GetComponent<RectTransform>(), position, new Vector2(190f, 26f));

        Image background = CreateImage(root.transform, "Background", new Color32(25, 55, 78, 255));
        Stretch(background.rectTransform, new Vector2(0f, 7f), new Vector2(0f, -7f));
        Image fill = CreateImage(root.transform, "Fill", new Color32(0, 198, 238, 255));
        Stretch(fill.rectTransform, new Vector2(4f, 9f), new Vector2(-4f, -9f));
        Image handle = CreateImage(root.transform, "Handle", new Color32(235, 246, 255, 255));
        SetRect(handle.rectTransform, Vector2.zero, new Vector2(22f, 32f));

        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        return slider;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction action, Color32? color = null)
    {
        Image image = CreateImage(parent, name, color ?? new Color32(18, 62, 91, 255));
        SetRect(image.rectTransform, position, new Vector2(190f, 40f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateText(image.transform, "Label", label, 18f, Vector2.zero, new Vector2(180f, 36f), FontStyles.Bold);
        return button;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(parent, false);
        Image image = child.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, Vector2 minimum, Vector2 maximum)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = minimum;
        rect.offsetMax = maximum;
    }
}
