using Framework.Interaction.Tools;
using Modules.Lobby.Flow;
using Modules.Lobby.Objectives;
using Modules.Lobby.Presentation.Tutorial;
using UnityEngine;
using System.Collections;
using Systems.Auth;
using Systems.Scenes;

namespace Modules.Lobby.Presentation
{
    public sealed class LobbyPresentationController : MonoBehaviour
    {
        [SerializeField] private LobbyTutorialController tutorialController;
        [SerializeField] private ToolManager toolManager;
        private static string replayUser;
        private static bool replayRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetReplay() { replayUser = null; replayRequested = false; }

        private LobbyFlowController flow;

        private IEnumerator Start()
        {
            yield return null;
            if (toolManager == null)
                toolManager = FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);

            flow = LobbyManager.Instance?.FlowController;
            if (flow == null || tutorialController == null)
            {
                Debug.LogError("LobbyPresentationController necesita LobbyManager y tutorial.", this);
                yield break;
            }

            flow.Begin();
            toolManager?.SetAvailableTools(flow.AvailableTools);
            tutorialController.TutorialCompleted += HandleTutorialCompleted;

            bool replay = replayRequested && replayUser == SessionContext.LocalStorageKey;
            ResetReplay();
            bool firstVisit = !SessionContext.IsDebugSession && SaveManager.Instance != null &&
                !SaveManager.Instance.HasSeenTutorial(CompleteLobbyTutorialObjective.ObjectiveId);
            if (!replay && !firstVisit)
            {
                flow.CompleteTutorial();
                yield break;
            }
            SaveManager.Instance?.MarkTutorialStarted(CompleteLobbyTutorialObjective.ObjectiveId);
            // La secuencia comienza después de retirar el panel de carga.
            while (SceneTransitionManager.IsLoading) yield return null;
            tutorialController.StartTutorial();
        }

        private void OnDestroy()
        {
            if (tutorialController != null)
                tutorialController.TutorialCompleted -= HandleTutorialCompleted;
        }

        public void RepeatTutorial()
        {
            if (SceneTransitionManager.IsLoading || !Application.CanStreamedLevelBeLoaded("Lobby")) return;
            replayUser = SessionContext.LocalStorageKey;
            replayRequested = true;
            SceneTransitionManager.LoadScene("Lobby");
        }

        private void HandleTutorialCompleted()
        {
            flow?.CompleteTutorial();
            SaveManager.Instance?.CompleteTutorialLocally(
                CompleteLobbyTutorialObjective.ObjectiveId,
                "Lobby");
        }
    }
}
