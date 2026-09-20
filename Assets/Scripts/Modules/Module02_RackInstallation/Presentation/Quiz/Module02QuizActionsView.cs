using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module02_RackInstallation.Presentation.Quiz
{
    /// <summary>Acciones adicionales del resultado. No guarda ni carga escenas por sí misma.</summary>
    public sealed class Module02QuizActionsView : MonoBehaviour
    {
        [SerializeField] private Button restartModuleButton;
        [SerializeField] private Button retrySaveButton;
        [SerializeField] private Button finishButton;
        [SerializeField] private TMP_Text saveStatus;
        public event Action RestartModuleRequested;
        public event Action RetrySaveRequested;
        private void Awake()
        {
            restartModuleButton.onClick.AddListener(Restart);
            retrySaveButton.onClick.AddListener(RetrySave);
            ShowSaveState(false, "Entrega el quiz para completar el módulo.", false);
        }
        private void OnDestroy()
        {
            restartModuleButton.onClick.RemoveListener(Restart);
            retrySaveButton.onClick.RemoveListener(RetrySave);
        }
        private void Restart() => RestartModuleRequested?.Invoke();
        private void RetrySave() => RetrySaveRequested?.Invoke();
        public void ShowSaveState(bool saved, string message, bool canRetry)
        {
            restartModuleButton.interactable = saved;
            finishButton.interactable = saved;
            retrySaveButton.gameObject.SetActive(canRetry);
            saveStatus.text = message;
        }
    }
}
