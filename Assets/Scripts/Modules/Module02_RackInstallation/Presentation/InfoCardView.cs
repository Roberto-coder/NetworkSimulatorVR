using Modules.Module02_RackInstallation.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module02_RackInstallation.Presentation
{
    public sealed class InfoCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text functionText;
        [SerializeField] private TMP_Text rackUnitsText;
        [SerializeField] private TMP_Text standardText;
        [SerializeField] private TMP_Text approximationText;
        [SerializeField] private Image componentImage;
        [SerializeField] private Image dwellFill;
        [SerializeField] private GameObject cardPanel;
        [SerializeField] private GameObject dwellPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button pinButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private TMP_Text pinLabel;
        [SerializeField] private TMP_Text pageLabel;
        [SerializeField] private InfoCardPointerArea pointerArea;

        public Button CloseButton => closeButton;
        public Button PinButton => pinButton;
        public Button NextButton => nextButton;
        public Button PreviousButton => previousButton;
        public bool IsPointerOverCard => pointerArea != null && pointerArea.IsHovered;
        public void SetPinned(bool pinned) => Set(pinLabel, pinned ? "Liberar" : "Fijar");

        public void ShowDwell(float progress)
        {
            if (dwellPanel != null) dwellPanel.SetActive(true);
            if (dwellFill != null) dwellFill.fillAmount = Mathf.Clamp01(progress);
        }

        public void HideDwell()
        {
            if (dwellPanel != null) dwellPanel.SetActive(false);
        }

        public void Show(RackComponentInfo info, int page = 0)
        {
            if (info == null) return;
            page = Mathf.Clamp(page, 0, info.Sections.Count);
            RackInfoSection section = page == 0 ? null : info.Sections[page - 1];
            Set(titleText, info.DisplayName);
            string heading = page == 0 ? "Información general" : section?.title ?? "Sección por completar";
            Set(categoryText, $"{CategoryName(info.Category)} / {heading}");
            Set(descriptionText, page == 0 ? info.ShortDescription : section?.body);
            Set(functionText, page != 0 || string.IsNullOrWhiteSpace(info.Function) ? string.Empty : "Función: " + info.Function);
            if (descriptionText != null)
                descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, page == 0 ? 115f : 255f);
            Set(rackUnitsText, info.RackUnits > 0 ? $"Altura: {info.RackUnits}U" : string.Empty);
            string reference = page == 0 ? info.StandardReference : section?.reference;
            Set(standardText, string.IsNullOrWhiteSpace(reference) ? string.Empty : "Referencia: " + reference);
            Set(approximationText, info.EducationalApproximation ? "Modelo educativo aproximado" : string.Empty);
            if (componentImage != null)
            {
                Sprite picture = page == 0 ? info.Image : section?.image;
                componentImage.sprite = picture;
                componentImage.gameObject.SetActive(picture != null);
            }
            Set(pageLabel, $"{page + 1} / {info.Sections.Count + 1}");
            if (nextButton != null) nextButton.interactable = page < info.Sections.Count;
            if (previousButton != null) previousButton.interactable = page > 0;
            HideDwell();
            if (cardPanel != null) cardPanel.SetActive(true);
        }

        public void HideCard()
        {
            if (cardPanel != null) cardPanel.SetActive(false);
        }

        private static void Set(TMP_Text field, string value)
        {
            if (field != null)
            {
                field.text = value ?? string.Empty;
                field.gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
            }
        }

        private static string CategoryName(RackComponentCategory category) => category switch
        {
            RackComponentCategory.Ports => "Puertos",
            RackComponentCategory.Power => "Alimentación",
            RackComponentCategory.Indicators => "Indicadores",
            RackComponentCategory.Cabling => "Cableado",
            RackComponentCategory.Router => "Router",
            RackComponentCategory.Server => "Servidor",
            RackComponentCategory.Firewall => "Firewall",
            RackComponentCategory.PatchPanel => "Patch panel",
            RackComponentCategory.UpsPdu => "UPS / PDU",
            RackComponentCategory.Cooling => "Refrigeración",
            RackComponentCategory.FiberOptics => "Fibra óptica",
            RackComponentCategory.Other => "Otros",
            _ => category.ToString()
        };
    }
}
