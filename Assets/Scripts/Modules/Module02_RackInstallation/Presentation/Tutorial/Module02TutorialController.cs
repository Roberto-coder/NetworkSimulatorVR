using System.Collections;
using GameData.Module02;
using Presentacion.Tutorial;
using UnityEngine;
using Waypoints;
using Presentacion.NPC;
using Modules.Module02_RackInstallation.Flow;

namespace Modules.Module02_RackInstallation.Presentation.Tutorial
{
    /// <summary>Conecta el tutorial opcional al director compartido, sin gobernar la práctica.</summary>
    public sealed class Module02TutorialController : MonoBehaviour
    {
        [SerializeField] private bool tutorialEnabled = true;
        [SerializeField] private Module02Manager manager;
        [SerializeField] private TutorialDirector director;
        [SerializeField] private Module02TutorialData data;
        [SerializeField] private Waypoint rackWaypoint;
        [SerializeField] private Waypoint startWaypoint;
        [SerializeField] private NPCReactionController reactionController;
        private Module02SequenceCoordinator coordinator;
        private bool started;

        private IEnumerator Start()
        {
            if (!tutorialEnabled) yield break;
            // Esperar a los Start del flujo y al salto de inspección de depuración.
            yield return null;
            if (!isActiveAndEnabled) yield break;
            if (manager == null) manager = Module02Manager.Instance;
            var flow = manager != null ? manager.FlowController : null;
            if (flow == null || director == null || director.DialogueController == null || data == null)
            {
                Debug.LogError("El tutorial del módulo 2 requiere flujo, director con diálogo y datos.", this);
                yield break;
            }
            // El punto de inicio puede ajustarse en la escena sin mover también el prefab.
            if (startWaypoint != null && director.MovementController != null)
            {
                var body = director.MovementController.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.position = startWaypoint.transform.position;
                    body.rotation = startWaypoint.transform.rotation;
                }
                else director.MovementController.transform.SetPositionAndRotation(
                    startWaypoint.transform.position, startWaypoint.transform.rotation);
            }
            director.SetFlowController(flow);
            director.SetSequence(new Module02TutorialBuilder().Build(flow, data, rackWaypoint));
            reactionController?.Configure(flow, director);
            coordinator = Module02SequenceCoordinator.Instance;
            if (coordinator != null && reactionController != null)
                coordinator.ActionRejected += reactionController.NotifyActionRejected;
            started = true;
            director.StartTutorial();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (coordinator != null && reactionController != null)
                coordinator.ActionRejected -= reactionController.NotifyActionRejected;
            if (started)
            {
                director?.StopTutorial();
                reactionController?.Configure(null, null);
            }
        }
    }
}
