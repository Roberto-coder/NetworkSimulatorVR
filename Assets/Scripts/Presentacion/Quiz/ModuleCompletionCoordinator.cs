using GameData.Modules;
using Modules.Module01_CableMaking.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;
using Systems.Scenes;

namespace Presentacion.Quiz
{
    /// <summary>
    /// Finaliza y persiste un modulo desde el resultado del quiz. No depende
    /// del tutorial, por lo que tambien funciona cuando este esta desactivado.
    /// </summary>
    public sealed class ModuleCompletionCoordinator : MonoBehaviour
    {
        private QuizController quizController;
        private ModuleDefinition moduleDefinition;
        private ModuleFlowController flowController;
        private bool quizWasCompleted;
        private float moduleStartedAt;

        public void Configure(
            QuizController controller,
            ModuleDefinition definition,
            ModuleFlowController flow)
        {
            Unsubscribe();
            quizController = controller;
            moduleDefinition = definition;
            flowController = flow;
            moduleStartedAt = Time.realtimeSinceStartup;
            quizController.QuizCompleted += HandleQuizCompleted;
            quizController.FinishRequested += FinishAndReturnToLobby;
        }

        private void OnDestroy() => Unsubscribe();

        private void HandleQuizCompleted(Core.Quiz.Domain.QuizResult _)
        {
            quizWasCompleted = true;
            flowController?.CompleteFinalQuiz();
            quizController.ShowAchievement(moduleDefinition?.CompletionAchievement);
        }

        private void FinishAndReturnToLobby()
        {
            if (!quizWasCompleted || moduleDefinition == null)
                return;

            if (SaveManager.Instance == null)
            {
                Debug.LogError("No hay SaveManager activo; no se abandonara el modulo sin guardar.", this);
                return;
            }

            SaveManager.Instance.CompleteModuleLocally(
                moduleDefinition.ModuleId,
                moduleDefinition.ModuleName,
                moduleDefinition.CompletionAchievement,
                moduleDefinition.Objectives.Count,
                Mathf.Max(0f, Time.realtimeSinceStartup - moduleStartedAt));

            SceneTransitionManager.LoadScene("Lobby");
        }

        private void Unsubscribe()
        {
            if (quizController == null)
                return;

            quizController.QuizCompleted -= HandleQuizCompleted;
            quizController.FinishRequested -= FinishAndReturnToLobby;
        }
    }
}
