using Modules.Lobby.UI;
using GameData.Achievements;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyProfilePanelSetup
{
    private const string ScenePath = "Assets/Scenes/Lobby.unity";

    [InitializeOnLoadMethod]
    private static void FixOpenLobbyAfterCompilation()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == ScenePath && FindInScene(activeScene, "ContentProfile") != null)
                FixScene();
        };
    }

    [MenuItem("Tools/Network Simulator/Fix Lobby Profile Panel")]
    public static void FixFromMenu() => FixScene();

    public static void FixFromCommandLine()
    {
        FixScene();
        EditorApplication.Exit(0);
    }

    private static void FixScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Fix Lobby Profile Panel solo puede ejecutarse fuera de Play Mode.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject profile = FindInScene(scene, "ContentProfile");
        if (profile == null)
            throw new MissingReferenceException("No se encontró ContentProfile en Lobby.");

        ConfigureRoot(profile);
        ConfigureHeader(profile.transform.Find("Header"));
        ConfigureBody(profile.transform.Find("ProfileBody"));
        ConfigureIdentity(profile.transform.Find("ProfileBody/IdentityCard"));
        ConfigureProgress(profile.transform.Find("ProfileBody/ProgressCard"));
        ConfigureSprint2(profile.transform.Find("ProfileBody/ProgressCard"));
        ConfigureTexts(profile.transform);

        if (profile.GetComponent<ProfilePanelPresenter>() == null)
            profile.AddComponent<ProfilePanelPresenter>();

        PanelController controller = Object.FindFirstObjectByType<PanelController>(FindObjectsInactive.Include);
        if (controller != null)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("contentProfile").objectReferenceValue = profile;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            Button backButton = profile.transform.Find("Header/ButtonVolver")?.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                UnityEventTools.AddStringPersistentListener(backButton.onClick, controller.ShowCanvas, "BackToMain");
                EditorUtility.SetDirty(backButton);
            }
        }

        profile.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("ContentProfile corregido y conectado correctamente.");
    }

    private static void ConfigureRoot(GameObject profile)
    {
        RectTransform rect = profile.GetComponent<RectTransform>();
        Stretch(rect);
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(profile);
        layout.padding = new RectOffset(30, 30, 24, 24);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static void ConfigureHeader(Transform header)
    {
        if (header == null) return;
        SetLayout(header.gameObject, -1, 58, 1, 0);
        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(header.gameObject);
        layout.padding = new RectOffset(10, 14, 8, 8);
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        SetLayout(header.Find("ButtonVolver")?.gameObject, 90, 42, 0, 0);
        SetLayout(header.Find("Title")?.gameObject, -1, -1, 1, 0);
        SetLayout(header.Find("SyncStatus")?.gameObject, 150, 42, 0, 0);
    }

    private static void ConfigureBody(Transform body)
    {
        if (body == null) return;
        SetLayout(body.gameObject, -1, 320, 1, 1);
        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(body.gameObject);
        layout.spacing = 18;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
    }

    private static void ConfigureIdentity(Transform card)
    {
        if (card == null) return;
        SetLayout(card.gameObject, 235, 300, 0, 1);
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(card.gameObject);
        layout.padding = new RectOffset(20, 20, 18, 14);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Transform avatar = card.Find("AvatarFrame");
        SetLayout(avatar?.gameObject, 112, 112, 0, 0);
        SetLayout(card.Find("UsernameText")?.gameObject, -1, 34, 1, 0);
        SetLayout(card.Find("EmailText")?.gameObject, -1, 28, 1, 0);
        SetLayout(card.Find("ActiveSlotText")?.gameObject, -1, 30, 1, 0);
        if (avatar != null && avatar.Find("AvatarImage") is RectTransform image)
        {
            image.anchorMin = Vector2.zero;
            image.anchorMax = Vector2.one;
            image.anchoredPosition = Vector2.zero;
            image.sizeDelta = new Vector2(-14, -14);
        }
    }

    private static void ConfigureProgress(Transform card)
    {
        if (card == null) return;
        SetLayout(card.gameObject, 380, 300, 1, 1);
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(card.gameObject);
        layout.padding = new RectOffset(18, 18, 12, 12);
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        SetLayout(card.Find("ProgressTitle")?.gameObject, -1, 34, 1, 0);
        SetLayout(card.Find("Divider")?.gameObject, -1, 2, 1, 0);

        foreach (string rowName in new[] { "ModulesRow", "AchievementsRow", "PlayTimeRow", "LastSaveRow" })
        {
            Transform row = card.Find(rowName);
            if (row == null) continue;
            SetLayout(row.gameObject, -1, 42, 1, 0);
            HorizontalLayoutGroup rowLayout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            rowLayout.spacing = 12;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            SetLayout(row.Find("Label")?.gameObject, -1, 42, 1, 0);
            SetLayout(row.Find("Value")?.gameObject, 150, 42, 0, 0);
        }
    }

    private static void ConfigureSprint2(Transform card)
    {
        if (card == null) return;

        foreach (string legacyRow in new[] { "ModulesRow", "AchievementsRow", "PlayTimeRow", "LastSaveRow" })
        {
            Transform row = card.Find(legacyRow);
            if (row != null) row.gameObject.SetActive(false);
        }

        Transform tabs = EnsureChild(card, "ModuleTabs");
        SetSiblingAfter(tabs, card.Find("Divider"));
        SetLayout(tabs.gameObject, -1, 34, 1, 0);
        HorizontalLayoutGroup tabsLayout = GetOrAdd<HorizontalLayoutGroup>(tabs.gameObject);
        tabsLayout.spacing = 8;
        tabsLayout.childAlignment = TextAnchor.MiddleCenter;
        tabsLayout.childControlWidth = true;
        tabsLayout.childControlHeight = true;
        tabsLayout.childForceExpandWidth = true;
        tabsLayout.childForceExpandHeight = true;
        for (int index = 1; index <= 3; index++)
            EnsureTabButton(tabs, index);

        Transform detail = EnsureChild(card, "ModuleDetail");
        detail.SetSiblingIndex(tabs.GetSiblingIndex() + 1);
        SetLayout(detail.gameObject, -1, 120, 1, 0);
        Image detailBackground = GetOrAdd<Image>(detail.gameObject);
        detailBackground.color = new Color32(14, 48, 81, 255);
        detailBackground.raycastTarget = false;
        VerticalLayoutGroup detailLayout = GetOrAdd<VerticalLayoutGroup>(detail.gameObject);
        detailLayout.padding = new RectOffset(12, 12, 6, 6);
        detailLayout.spacing = 1;
        detailLayout.childControlWidth = true;
        detailLayout.childControlHeight = true;
        detailLayout.childForceExpandWidth = true;
        detailLayout.childForceExpandHeight = false;
        EnsureText(detail, "ModuleName", "Módulo 1 · Fabricación de cable", 17, FontStyles.Bold, 24);
        EnsureText(detail, "Status", "Estado: Pendiente", 15, FontStyles.Normal, 20);
        EnsureText(detail, "Objectives", "Objetivos: 0/8", 15, FontStyles.Normal, 20);
        EnsureText(detail, "PlayTime", "Tiempo: 00:00:00", 15, FontStyles.Normal, 20);
        EnsureText(detail, "CompletionDate", "Finalización: —", 15, FontStyles.Normal, 20);

        Transform badgesTitle = EnsureChild(card, "BadgesTitle");
        badgesTitle.SetSiblingIndex(detail.GetSiblingIndex() + 1);
        EnsureTextComponent(badgesTitle, "INSIGNIAS", 16, FontStyles.Bold);
        SetLayout(badgesTitle.gameObject, -1, 18, 1, 0);

        Transform badges = EnsureChild(card, "BadgesContainer");
        badges.SetSiblingIndex(badgesTitle.GetSiblingIndex() + 1);
        SetLayout(badges.gameObject, -1, 70, 1, 0);
        HorizontalLayoutGroup badgesLayout = GetOrAdd<HorizontalLayoutGroup>(badges.gameObject);
        badgesLayout.spacing = 14;
        badgesLayout.childAlignment = TextAnchor.MiddleCenter;
        badgesLayout.childControlWidth = true;
        badgesLayout.childControlHeight = true;
        badgesLayout.childForceExpandWidth = true;
        badgesLayout.childForceExpandHeight = true;

        Sprite moduleOneIcon = AssetDatabase.LoadAssetAtPath<AchievementDefinition>(
            "Assets/GameData/Achievements/Module01Completion.asset")?.Icon;
        Sprite moduleTwoIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Recursos/LobbyUI/M2sprite_v1.png");
        Sprite moduleThreeIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Recursos/LobbyUI/M3sprite_v1.png");
        EnsureBadge(badges, 1, moduleOneIcon);
        EnsureBadge(badges, 2, moduleTwoIcon);
        EnsureBadge(badges, 3, moduleThreeIcon);
    }

    private static void EnsureTabButton(Transform parent, int index)
    {
        Transform tab = EnsureChild(parent, $"ButtonM{index}");
        Image image = GetOrAdd<Image>(tab.gameObject);
        image.color = index == 1 ? new Color32(52, 200, 255, 255) : new Color32(14, 48, 81, 255);
        Button button = GetOrAdd<Button>(tab.gameObject);
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color32(75, 215, 255, 255);
        colors.pressedColor = new Color32(35, 150, 205, 255);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        SetLayout(tab.gameObject, 100, 34, 1, 0);
        string tabText = index == 1 ? "M1" : $"M{index} · BLOQ.";
        TMP_Text label = EnsureText(tab, "Label", tabText, index == 1 ? 17 : 13, FontStyles.Bold, 34);
        label.alignment = TextAlignmentOptions.Center;
        Stretch((RectTransform)label.transform);
    }

    private static void EnsureBadge(Transform parent, int index, Sprite sprite)
    {
        Transform badge = EnsureChild(parent, $"BadgeM{index}");
        SetLayout(badge.gameObject, 92, 70, 1, 0);
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(badge.gameObject);
        layout.spacing = 2;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Transform iconTransform = EnsureChild(badge, "Icon");
        Image icon = GetOrAdd<Image>(iconTransform.gameObject);
        if (sprite != null) icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.color = new Color32(82, 98, 115, 140);
        SetLayout(iconTransform.gameObject, 48, 48, 0, 0);

        TMP_Text state = EnsureText(badge, "State", "BLOQUEADA", 11, FontStyles.Bold, 16);
        state.alignment = TextAlignmentOptions.Center;
        state.color = new Color32(183, 201, 216, 255);
    }

    private static TMP_Text EnsureText(Transform parent, string name, string value, float size, FontStyles style, float height)
    {
        Transform child = EnsureChild(parent, name);
        TMP_Text text = EnsureTextComponent(child, value, size, style);
        SetLayout(child.gameObject, -1, height, 1, 0);
        return text;
    }

    private static TMP_Text EnsureTextComponent(Transform target, string value, float size, FontStyles style)
    {
        TMP_Text text = GetOrAdd<TextMeshProUGUI>(target.gameObject);
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = new Color32(242, 247, 252, 255);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing;
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static void SetSiblingAfter(Transform target, Transform previous)
    {
        if (previous != null)
            target.SetSiblingIndex(previous.GetSiblingIndex() + 1);
    }

    private static void ConfigureTexts(Transform root)
    {
        SetText(root, "Header/Title", "PERFIL DEL USUARIO");
        SetText(root, "Header/SyncStatus/Text (TMP)", "DATOS LOCALES");
        SetText(root, "ProfileBody/IdentityCard/UsernameText", "USUARIO");
        SetText(root, "ProfileBody/IdentityCard/EmailText", "Correo no disponible");
        SetText(root, "ProfileBody/IdentityCard/ActiveSlotText", "SIN SLOT ACTIVO");
        SetText(root, "ProfileBody/ProgressCard/ProgressTitle", "RESUMEN DE PROGRESO");
        SetText(root, "ProfileBody/ProgressCard/ModulesRow/Label", "Módulos completados");
        SetText(root, "ProfileBody/ProgressCard/AchievementsRow/Label", "Insignias obtenidas");
        SetText(root, "ProfileBody/ProgressCard/PlayTimeRow/Label", "Tiempo total");
        SetText(root, "ProfileBody/ProgressCard/LastSaveRow/Label", "Último guardado");
        foreach (string row in new[] { "ModulesRow", "AchievementsRow", "PlayTimeRow", "LastSaveRow" })
            SetText(root, $"ProfileBody/ProgressCard/{row}/Value", "—");
    }

    private static void SetText(Transform root, string path, string value)
    {
        TMP_Text text = root.Find(path)?.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = value;
            text.raycastTarget = false;
            EditorUtility.SetDirty(text);
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void SetLayout(GameObject target, float width, float height, float flexibleWidth, float flexibleHeight)
    {
        if (target == null) return;
        LayoutElement element = GetOrAdd<LayoutElement>(target);
        element.preferredWidth = width;
        element.preferredHeight = height;
        element.flexibleWidth = flexibleWidth;
        element.flexibleHeight = flexibleHeight;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.GetComponent<T>() ?? target.AddComponent<T>();

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in transforms)
                if (item.name == name) return item.gameObject;
        }
        return null;
    }
}
