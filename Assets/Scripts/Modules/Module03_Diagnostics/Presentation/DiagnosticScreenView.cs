using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module03_Diagnostics.Presentation
{
    [Serializable]
    public sealed class DiagnosticNodeBinding
    {
        public string deviceId;
        public Button button;
        public TMP_Text label;
        public UnityEngine.UI.Outline selectionOutline;
        public TMP_Text status; // Separado del nombre para conservar la etiqueta bajo el sprite.
    }
    /// <summary>Jerarquía preexistente: no instancia elementos visuales en Play.</summary>
    public sealed class DiagnosticScreenView : MonoBehaviour
    {
        public TMP_Text title, output;
        public TMP_Text animationStatus, xRayLabel;
        public Button xRayButton;
        public Image xRayIndicator;
        public Sprite xRayOffSprite, xRayOnSprite;
        public void SetXRay(bool value)
        {
            if (xRayLabel != null) xRayLabel.text = value ? "Rayos X · Activos" : "Rayos X · Inactivos";
            if (xRayIndicator != null) xRayIndicator.sprite = value ? xRayOnSprite : xRayOffSprite;
        }
        public RectTransform outputRect;
        public Button[] commands;
        public List<DiagnosticNodeBinding> nodes = new();
        public void ShowOutput(string value)
        {
            // Espaciado fijo para alinear las columnas IP/MAC con la fuente TMP existente.
            output.richText = true;
            output.text = "<mspace=0.55em>" + value + "</mspace>"; output.ForceMeshUpdate();
            // Solo cambia el contenido desplazable; el Canvas conserva su transform.
            float viewportHeight = outputRect.parent is RectTransform viewport ? viewport.rect.height : 390;
            outputRect.sizeDelta = new Vector2(outputRect.sizeDelta.x, Mathf.Max(viewportHeight, output.preferredHeight + 30));
            outputRect.anchoredPosition = Vector2.zero;
        }
    }
}
