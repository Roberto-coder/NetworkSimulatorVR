using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.Quiz
{
    /// <summary>
    /// Representa una opcion seleccionable dentro de la interfaz del quiz.
    /// </summary>
    public sealed class QuizAnswerOptionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text optionText;
        [SerializeField] private Image selectionIndicator;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new(0.2f, 0.65f, 1f, 1f);

        private int optionIndex;
        private Action<int> selectedCallback;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        public void Setup(int index, string label, bool isSelected, Action<int> onSelected)
        {
            optionIndex = index;
            selectedCallback = onSelected;

            if (optionText != null)
                optionText.text = label;

            SetSelected(isSelected);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionIndicator != null)
                selectionIndicator.color = isSelected ? selectedColor : normalColor;
        }

        private void HandleClick()
        {
            selectedCallback?.Invoke(optionIndex);
        }
    }
}
