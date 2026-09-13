#if UNITY_EDITOR
using System;
using Modules.Module02_RackInstallation.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>Actualiza sólo la UI de Sprint 1. No regenera datos ni colliders.</summary>
public static class Module02ExplorationSprint02Setup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";

    [MenuItem("Network Simulator/Module 02/Sprint 2 - Actualizar tarjetas")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play Mode antes de actualizar las tarjetas.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
        InfoCardView view = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            view = root.GetComponentInChildren<InfoCardView>(true);
            if (view != null) break;
        }
        if (view == null) throw new InvalidOperationException("Primero configura el Sprint 1; no se encontró InfoCardView.");

        var canvas = view.GetComponent<Canvas>();
        var rect = view.GetComponent<RectTransform>();
        var panel = view.transform.Find("CardPanel") as RectTransform;
        if (canvas == null || rect == null || panel == null)
            throw new InvalidOperationException("La tarjeta necesita Canvas, RectTransform y CardPanel.");

        Undo.RegisterFullObjectHierarchyUndo(view.gameObject, "Actualizar tarjeta Sprint 2");
        canvas.renderMode = RenderMode.WorldSpace;
        rect.sizeDelta = new Vector2(720f, 780f);
        // Conserva la escala de mundo que el usuario haya ajustado.
        canvas.worldCamera = Camera.main;
        if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            Undo.AddComponent<TrackedDeviceGraphicRaycaster>(canvas.gameObject);
        Layout(panel, Vector2.zero, new Vector2(720, 780), new Vector2(0.5f, 0.5f));
        var pointer = panel.GetComponent<InfoCardPointerArea>() ?? Undo.AddComponent<InfoCardPointerArea>(panel.gameObject);
        var background = panel.GetComponent<Image>();
        if (background != null) background.raycastTarget = true;

        Text(panel, "Title", new Vector2(28, -24), new Vector2(580, 60), 34);
        Text(panel, "Category", new Vector2(28, -88), new Vector2(650, 36), 23);
        Text(panel, "Description", new Vector2(28, -140), new Vector2(664, 115), 26);
        Text(panel, "Function", new Vector2(28, -267), new Vector2(664, 130), 24);
        Text(panel, "RackUnits", new Vector2(280, -430), new Vector2(410, 40), 23);
        Text(panel, "Standard", new Vector2(280, -478), new Vector2(410, 155), 22);
        Text(panel, "Approximation", new Vector2(28, -647), new Vector2(664, 36), 20);
        var imageRect = Child(panel, "ComponentImage");
        Layout(imageRect, new Vector2(28, -430), new Vector2(230, 185));
        var picture = imageRect.GetComponent<Image>() ?? Undo.AddComponent<Image>(imageRect.gameObject);
        picture.preserveAspect = true;
        picture.raycastTarget = false;

        var pin = Button(panel, "PinButton", "Fijar", new Vector2(28, -704), new Vector2(170, 56));
        var previous = Button(panel, "PreviousButton", "Anterior", new Vector2(245, -704), new Vector2(145, 56));
        var next = Button(panel, "NextButton", "Siguiente", new Vector2(545, -704), new Vector2(147, 56));
        var counter = Text(panel, "PageLabel", new Vector2(394, -704), new Vector2(147, 56), 23);
        counter.alignment = TextAlignmentOptions.Center;
        counter.text = "1 / 1";
        var close = Button(panel, "CloseButton", "X", new Vector2(628, -24), new Vector2(64, 60));

        var serialized = new SerializedObject(view);
        Ref(serialized, "pointerArea", pointer);
        Ref(serialized, "componentImage", picture);
        Ref(serialized, "pinButton", pin);
        Ref(serialized, "nextButton", next);
        Ref(serialized, "previousButton", previous);
        Ref(serialized, "closeButton", close);
        Ref(serialized, "pinLabel", pin.GetComponentInChildren<TMP_Text>(true));
        Ref(serialized, "pageLabel", counter);
        serialized.ApplyModifiedProperties();

        // El indicador anterior no tenía Sprite: Image.Filled requiere uno para dibujar progreso.
        var dwell = view.transform.Find("DwellPanel");
        if (dwell != null)
        {
            foreach (var graphic in dwell.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            var fill = dwell.Find("Fill")?.GetComponent<Image>();
            if (fill != null)
            {
                fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Radial360;
            }
        }

        EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = view.gameObject;
        Debug.Log("Sprint 2 actualizado. Fijar/liberar, páginas e imágenes listos. Los ScriptableObjects y colliders existentes no fueron modificados.");
    }

    private static void Ref(SerializedObject data, string field, UnityEngine.Object value) =>
        data.FindProperty(field).objectReferenceValue = value;

    private static RectTransform Child(Transform parent, string name)
    {
        var found = parent.Find(name) as RectTransform;
        if (found != null) return found;
        var created = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(created, "Crear control de tarjeta");
        created.transform.SetParent(parent, false);
        return (RectTransform)created.transform;
    }

    private static void Layout(RectTransform rect, Vector2 position, Vector2 size, Vector2? anchor = null)
    {
        rect.anchorMin = rect.anchorMax = anchor ?? new Vector2(0f, 1f);
        rect.pivot = anchor ?? new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static TMP_Text Text(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        var rect = Child(parent, name);
        Layout(rect, position, size);
        var text = rect.GetComponent<TextMeshProUGUI>() ?? Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 20f;
        text.fontSizeMax = fontSize;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        var rect = Child(parent, name);
        Layout(rect, position, size);
        var image = rect.GetComponent<Image>() ?? Undo.AddComponent<Image>(rect.gameObject);
        image.color = new Color(0.12f, 0.25f, 0.35f);
        image.raycastTarget = true;
        var button = rect.GetComponent<Button>() ?? Undo.AddComponent<Button>(rect.gameObject);
        button.targetGraphic = image;
        var text = Text(rect, "Label", Vector2.zero, size, 23f);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }
}
#endif
