using Modules.Module01_CableMaking.Presentation.Tutorial;
using Presentacion.Tutorial;
using UnityEngine;
using Waypoints;

namespace Framework.Debug
{
    public class TutorialBootstrap : MonoBehaviour
    {
        [SerializeField] TutorialDirector director;

        [SerializeField] Waypoint cableWaypoint;

        private void Start()
        {
            Module01TutorialBuilder builder =
                new Module01TutorialBuilder();

            director.SetSequence(
                builder.Build(cableWaypoint));

            director.StartTutorial();
        }
    }
}