using Modules.Module01_CableMaking.Flow;
using Presentacion.NPC;
using Presentacion.Tutorial;
using UnityEngine;
using Waypoints;

namespace Modules.Module01_CableMaking.Presentation.Tutorial
{
    /// <summary>
    /// Ensambla e inicia el tutorial opcional del módulo 01.
    /// </summary>
    public sealed class Module01TutorialController : MonoBehaviour
    {
        [SerializeField] private bool tutorialEnabled = true;
        [SerializeField] private TutorialDirector director;
        [SerializeField] private NPCReactionController reactionController;
        [SerializeField] private Waypoint cableWaypoint;
        [SerializeField] private Waypoint puzzleWaypoint;
        [SerializeField] private Waypoint quizWaypoint;

        private void Start()
        {
            if (!tutorialEnabled)
                return;

            ModuleFlowController flow = SimulationManager.Instance?.FlowController;
            if (director == null || flow == null || cableWaypoint == null)
            {
                Debug.LogError(
                    "El tutorial necesita director, flujo y waypoint del cable.",
                    this);
                return;
            }

            if (puzzleWaypoint == null)
            {
                Debug.LogWarning(
                    "No se configuró el waypoint del puzzle; se omitirá ese movimiento del NPC.",
                    this);
            }

            if (quizWaypoint == null)
            {
                Debug.LogWarning(
                    "No se configuró el waypoint del quiz; se omitirá ese movimiento del NPC.",
                    this);
            }

            Module01TutorialBuilder builder = new();

            director.SetFlowController(flow);
            director.SetSequence(builder.Build(
                cableWaypoint,
                puzzleWaypoint,
                quizWaypoint));
            reactionController?.Configure(flow, director);
            director.StartTutorial();
        }
    }
}
