using System.Collections;
using GameData.NPC;
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
        [SerializeField] private NPCDialogueData data;
        [SerializeField] private NPCReactionController reactionController;
        [SerializeField] private Waypoint pedestalWaypoint;
        [SerializeField] private Waypoint cableWaypoint;
        [SerializeField] private Waypoint puzzleWaypoint;
        [SerializeField] private Waypoint quizWaypoint;

        private IEnumerator Start()
        {
            if (!tutorialEnabled)
                yield break;

            yield return null; // Esperar a que SimulationManager.Start active el flujo.
            ModuleFlowController flow = SimulationManager.Instance?.FlowController;
            if (data == null || director == null || flow == null || pedestalWaypoint == null || cableWaypoint == null)
            {
                Debug.LogError(
                    "El tutorial necesita GameData, director, flujo y los waypoints del pedestal y del cable.",
                    this);
                yield break;
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
                data,
                pedestalWaypoint,
                cableWaypoint,
                puzzleWaypoint,
                quizWaypoint));
            if (reactionController != null) reactionController.Configure(flow, director, data.Find);
            director.StartTutorial();
        }
    }
}
