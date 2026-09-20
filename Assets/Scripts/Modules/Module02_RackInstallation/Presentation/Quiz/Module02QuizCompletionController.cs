using Core.Quiz.Domain;
using Modules.Module02_RackInstallation.Flow;
using Presentacion.Quiz;
using Systems.Scenes;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Presentation.Quiz
{
    /// <summary>Entrega, guarda una sola vez y permite reiniciar o volver al lobby con el progreso conservado.</summary>
    public sealed class Module02QuizCompletionController : MonoBehaviour
    {
        private QuizController quiz;
        private Module02QuizActionsView actions;
        private Module02FlowController flow;
        private float startedAt;
        private float completedPlayTime;
        private bool saved;
        private bool submitted;
        private string moduleScene;

        public void Configure(QuizController quiz, Module02QuizActionsView actions, Module02FlowController flow,
            float startedAt, string moduleScene)
        {
            this.quiz = quiz; this.actions = actions; this.flow = flow;
            this.startedAt = startedAt; this.moduleScene = moduleScene;
            quiz.QuizCompleted += OnSubmitted;
            quiz.FinishRequested += ReturnToLobby;
            actions.RestartModuleRequested += RestartModule;
            actions.RetrySaveRequested += SaveCompletion;
        }
        private void OnDestroy()
        {
            if (quiz != null) { quiz.QuizCompleted -= OnSubmitted; quiz.FinishRequested -= ReturnToLobby; }
            if (actions != null)
            { actions.RestartModuleRequested -= RestartModule; actions.RetrySaveRequested -= SaveCompletion; }
        }
        private void OnSubmitted(QuizResult _)
        {
            if (!flow.ArePracticalObjectivesCompleted || !quiz.HasSubmitted) return;
            // Repetir el quiz no vuelve a sumar la duración ni concede otra insignia.
            if (!submitted) completedPlayTime = Mathf.Max(0f, Time.realtimeSinceStartup - startedAt);
            submitted = true;
            flow.CompleteFinalQuiz();
            SaveCompletion();
        }
        private void SaveCompletion()
        {
            if (!submitted) return;
            if (!saved)
            {
                // Permite comprobar Modulo2 directamente en Play; desde Lobby se reutiliza el manager persistente.
                if (SaveManager.Instance == null) new GameObject("SaveManager").AddComponent<SaveManager>();
                var data = flow.ModuleDefinition;
                if (!SaveManager.Instance.TryCompleteModuleLocally(data.ModuleId, data.ModuleName,
                    data.CompletionAchievement, data.Objectives.Count, completedPlayTime, out var error))
                {
                    actions.ShowSaveState(false, error, true);
                    return;
                }
                saved = true;
            }
            quiz.ShowAchievement(flow.ModuleDefinition.CompletionAchievement);
            actions.ShowSaveState(true, "Módulo completado y progreso guardado en este dispositivo.", false);
        }
        private void ReturnToLobby() => Navigate("Lobby");
        private void RestartModule() => Navigate(moduleScene);
        private void Navigate(string scene)
        {
            if (!saved || !quiz.HasSubmitted || SceneTransitionManager.IsLoading) return;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                actions.ShowSaveState(true, $"No se puede abrir {scene}: habilita la escena en Build Settings.", false);
                return;
            }
            // Recargar elimina cables, objetivos y NPC de esta sesión; el archivo guardado se conserva.
            SceneTransitionManager.LoadScene(scene);
        }
    }
}
