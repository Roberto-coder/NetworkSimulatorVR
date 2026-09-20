using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module01_CableMaking.Presentation
{
    /// <summary>
    /// Completa el objetivo de selección al confirmar una categoría de cable.
    /// Todas las opciones habilitan el cable de práctica configurado en la escena.
    /// </summary>
    public sealed class CableSelectionPanel : MonoBehaviour
    {
        [Serializable]
        public struct CableOption
        {
            public string displayName;
            [TextArea(8, 16)] public string description;
            public Color accentColor;
            public Sprite image;
        }

        [Header("Scene references")]
        [SerializeField] private GameObject cableToEnable;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text cableNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image cablePreview;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button selectButton;

        [Header("Informative options")]
        [SerializeField] private CableOption[] options;

        private int selectedIndex;
        private bool hasSpawned;

        public bool HasSpawned => hasSpawned;
        public string SelectedCableName =>
            options != null && options.Length > 0 ? options[selectedIndex].displayName : string.Empty;

        private void Awake()
        {
            if (cableToEnable != null)
                cableToEnable.SetActive(false);

            previousButton?.onClick.AddListener(ShowPrevious);
            nextButton?.onClick.AddListener(ShowNext);
            selectButton?.onClick.AddListener(SelectCurrentCable);
            Refresh();
        }

        private void OnDestroy()
        {
            previousButton?.onClick.RemoveListener(ShowPrevious);
            nextButton?.onClick.RemoveListener(ShowNext);
            selectButton?.onClick.RemoveListener(SelectCurrentCable);
        }

        public void ShowPrevious()
        {
            if (hasSpawned || options == null || options.Length == 0)
                return;

            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            Refresh();
        }

        public void ShowNext()
        {
            if (hasSpawned || options == null || options.Length == 0)
                return;

            selectedIndex = (selectedIndex + 1) % options.Length;
            Refresh();
        }

        public void SelectCurrentCable()
        {
            if (hasSpawned || cableToEnable == null || options == null || options.Length == 0)
                return;

            var flow = SimulationManager.Instance?.FlowController;
            if (flow?.CurrentObjectiveData?.Id != "select_cable")
                return;

            hasSpawned = true;
            if (selectButton != null)
                selectButton.interactable = false;
            cableToEnable.SetActive(true);
            flow.RegisterCableSelection();
            Debug.Log($"[CableSelection] Cable seleccionado: {SelectedCableName}.", this);

            // El panel completo se oculta y no puede volver a generar otro cable.
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Refresh()
        {
            if (options == null || options.Length == 0)
                return;

            CableOption option = options[selectedIndex];
            if (cableNameText != null)
            {
                cableNameText.richText = true;
                cableNameText.text = $"<b>{option.displayName}</b>";
            }
            if (descriptionText != null)
            {
                descriptionText.richText = true;
                descriptionText.text = option.description;
            }
            if (cablePreview != null)
            {
                cablePreview.sprite = option.image;
                cablePreview.preserveAspect = true;
                cablePreview.color = option.image != null ? Color.white : option.accentColor;
            }
        }

#if UNITY_EDITOR
        public void Configure(
            GameObject cable,
            GameObject root,
            TMP_Text cableName,
            TMP_Text description,
            Image preview,
            Button previous,
            Button next,
            Button select,
            CableOption[] cableOptions)
        {
            cableToEnable = cable;
            panelRoot = root;
            cableNameText = cableName;
            descriptionText = description;
            cablePreview = preview;
            previousButton = previous;
            nextButton = next;
            selectButton = select;
            options = cableOptions;
            Refresh();
        }
#endif
    }
}
