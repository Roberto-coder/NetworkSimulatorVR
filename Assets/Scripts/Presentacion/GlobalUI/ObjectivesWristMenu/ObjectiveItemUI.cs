using Core.Objectives;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
    public sealed class ObjectiveItemUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Toggle completedToggle;
        [SerializeField] private Image stateBackground;
        [SerializeField] private Image checkBoxBackground;

        private static readonly Color PendingText = new Color(0.72f, 0.77f, 0.82f, 1f);
        private static readonly Color CurrentText = Color.white;
        private static readonly Color CompletedText = new Color(0.50f, 0.87f, 0.70f, 1f);
        private static readonly Color PendingBackground = new Color(1f, 1f, 1f, 0.035f);
        private static readonly Color CurrentBackground = new Color(0.18f, 0.50f, 0.90f, 0.28f);
        private static readonly Color CompletedBackground = new Color(0.20f, 0.75f, 0.52f, 0.14f);

        public void Setup(string title, ObjectiveVisualState state)
        {
            titleText.text = title;

            completedToggle.SetIsOnWithoutNotify(state == ObjectiveVisualState.Completed);

            switch (state)
            {
                case ObjectiveVisualState.Pending:
                    titleText.fontStyle = FontStyle.Normal;
                    titleText.color = PendingText;
                    SetColors(PendingBackground, new Color(1f, 1f, 1f, 0.18f));
                    break;

                case ObjectiveVisualState.Current:
                    titleText.fontStyle = FontStyle.Bold;
                    titleText.color = CurrentText;
                    SetColors(CurrentBackground, new Color(0.30f, 0.65f, 1f, 1f));
                    break;

                case ObjectiveVisualState.Completed:
                    titleText.fontStyle = FontStyle.Normal;
                    titleText.color = CompletedText;
                    SetColors(CompletedBackground, new Color(0.20f, 0.75f, 0.52f, 1f));
                    break;
            }
        }

        private void SetColors(Color background, Color checkBox)
        {
            if (stateBackground != null)
                stateBackground.color = background;
            if (checkBoxBackground != null)
                checkBoxBackground.color = checkBox;
        }
    }
}
