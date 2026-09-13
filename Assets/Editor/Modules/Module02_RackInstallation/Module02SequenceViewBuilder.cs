#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Construye el Canvas y botón del flujo. No crea ni completa objetivos.</summary>
public static class Module02SequenceViewBuilder
{
    public static (Button confirm, TMP_Text status) Build(GameObject root, GameObject cardPanel, Vector3 switchPosition)
    {
        // El botón vive en la tarjeta, pero la condición y el contador pertenecen al flujo.
        var confirm = Ui("ConfirmReview", cardPanel.transform, new Vector2(400, 52));
        var confirmRect = (RectTransform)confirm.transform;
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(0.5f, 0); confirmRect.pivot = new Vector2(0.5f, 1);
        confirmRect.anchoredPosition = new Vector2(0, -12);
        var image = confirm.AddComponent<Image>(); image.color = new Color(0.05f, 0.2f, 0.15f);
        var button = Undo.AddComponent<Button>(confirm); button.targetGraphic = image;
        Label(confirm.transform, "Marcar como revisado", new Vector2(390, 48), 24);


        var statusRoot = Ui("SequenceStatus", root.transform, new Vector2(680, 160));
        statusRoot.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        statusRoot.transform.localScale = Vector3.one * 0.001f;
        statusRoot.transform.position = switchPosition + Vector3.up * 0.55f;
        var status = Label(statusRoot.transform, "Objetivos del módulo", new Vector2(670, 150), 24);
        return (button, status);

    }
    private static GameObject Ui(string name, Transform parent, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform)); Undo.RegisterCreatedObjectUndo(obj, "Crear UI de flujo");
        obj.transform.SetParent(parent, false); ((RectTransform)obj.transform).sizeDelta = size; return obj;
    }
    private static TMP_Text Label(Transform parent, string value, Vector2 size, float font)
    {
        var obj = Ui("Text", parent, size); var text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value; text.fontSize = font; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
    }
}
#endif
