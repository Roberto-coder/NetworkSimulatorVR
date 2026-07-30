using Core.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
    public class ObjectiveItemUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Toggle completedToggle;

        public void Setup(string title, ObjectiveVisualState state)
        {
            titleText.text = title;

            completedToggle.isOn = state == ObjectiveVisualState.Completed;

            switch (state)
            {
                case ObjectiveVisualState.Pending:
                    titleText.fontStyle = FontStyle.Normal;
                    break;

                case ObjectiveVisualState.Current:
                    titleText.fontStyle = FontStyle.Bold;
                    break;

                case ObjectiveVisualState.Completed:
                    titleText.fontStyle = FontStyle.Normal;
                    break;
            }
        }
    }
}