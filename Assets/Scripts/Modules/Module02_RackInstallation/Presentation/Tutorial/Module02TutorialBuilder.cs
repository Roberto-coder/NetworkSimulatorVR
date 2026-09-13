using Core.Objectives;
using GameData.Module02;
using Presentacion.Tutorial;
using Waypoints;
using Presentacion.NPC;

namespace Modules.Module02_RackInstallation.Presentation.Tutorial
{
    /// <summary>
    /// Construye los pasos narrativos como Module01TutorialBuilder.
    /// No mueve objetos, cambia permisos ni completa objetivos del flujo.
    /// </summary>
    public sealed class Module02TutorialBuilder
    {
        public TutorialSequence Build(IObjectiveFlow flow, Module02TutorialData data, Waypoint rackWaypoint)
        {
            var sequence = new TutorialSequence();
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            if (!string.IsNullOrWhiteSpace(data.Introduction))
                sequence.AddStep(new DialogueStep(data.Introduction));
            if (rackWaypoint != null)
            {
                sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
                sequence.AddStep(new MoveNpcStep(rackWaypoint));
                sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            }

            // Reutilizar la definición real evita mantener una segunda lista de objetivos.
            // También omite los completados por la opción de depuración.
            foreach (var objective in flow.Objectives)
            {
                if (objective.IsCompleted) continue;
                var info = objective.Data;
                sequence.AddStep(new Module02GuidedTutorialStep(objective, data.Find(info.Id)));
            }
            if (!string.IsNullOrWhiteSpace(data.Completion))
                sequence.AddStep(new DialogueStep(data.Completion));
            return sequence;
        }
    }
}
