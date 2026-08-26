using Presentacion.Tutorial;
using UnityEngine;
using Waypoints;

namespace Modules.Lobby.Presentation.Tutorial
{
    public sealed class LobbyTutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialDirector director;
        [SerializeField] private Waypoint mainPanelWaypoint;
        [SerializeField] private Waypoint museumAreaWaypoint;
        [SerializeField] private Waypoint finalWaypoint;
        public event System.Action TutorialCompleted;

        private void Awake()
        {
            if (director == null || mainPanelWaypoint == null ||
                museumAreaWaypoint == null || finalWaypoint == null)
            {
                Debug.LogError("LobbyTutorialController tiene referencias incompletas.", this);
                return;
            }
            director.TutorialCompleted += HandleTutorialCompleted;
        }

        private void OnDestroy()
        {
            if (director != null)
                director.TutorialCompleted -= HandleTutorialCompleted;
        }

        public void StartTutorial()
        {
            if (director.IsRunning)
                return;
            director.SetSequence(new LobbyTutorialBuilder().Build(
                mainPanelWaypoint, museumAreaWaypoint, finalWaypoint));
            director.StartTutorial();
        }

        private void HandleTutorialCompleted()
        {
            TutorialCompleted?.Invoke();
        }
    }
}
