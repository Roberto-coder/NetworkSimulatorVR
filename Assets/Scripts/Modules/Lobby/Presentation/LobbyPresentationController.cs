using Framework.Interaction.Tools;
using Modules.Lobby.Flow;
using Modules.Lobby.Objectives;
using Modules.Lobby.Presentation.Tutorial;
using UnityEngine;

namespace Modules.Lobby.Presentation
{
    public sealed class LobbyPresentationController : MonoBehaviour
    {
        [SerializeField] private LobbyTutorialController tutorialController;
        [SerializeField] private ToolManager toolManager;
        [SerializeField] private bool forceTutorial;

        private LobbyFlowController flow;

        private void Start()
        {
            if (toolManager == null)
                toolManager = FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);

            flow = LobbyManager.Instance?.FlowController;
            if (flow == null || tutorialController == null)
            {
                Debug.LogError("LobbyPresentationController necesita LobbyManager y tutorial.", this);
                return;
            }

            flow.Begin();
            toolManager?.SetAvailableTools(flow.AvailableTools);
            tutorialController.TutorialCompleted += HandleTutorialCompleted;

            bool completed = SaveManager.Instance != null &&
                             SaveManager.Instance.HasCompletedTutorial(
                                 CompleteLobbyTutorialObjective.ObjectiveId);
            if (completed && !forceTutorial)
                flow.CompleteTutorial();
            else
                tutorialController.StartTutorial();
        }

        private void OnDestroy()
        {
            if (tutorialController != null)
                tutorialController.TutorialCompleted -= HandleTutorialCompleted;
        }

        public void RepeatTutorial() => tutorialController?.StartTutorial();

        private void HandleTutorialCompleted()
        {
            flow?.CompleteTutorial();
            SaveManager.Instance?.CompleteTutorialLocally(
                CompleteLobbyTutorialObjective.ObjectiveId,
                "Lobby");
        }
    }
}
