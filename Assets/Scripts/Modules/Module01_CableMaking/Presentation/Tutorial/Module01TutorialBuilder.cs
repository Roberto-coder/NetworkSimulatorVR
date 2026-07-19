using Presentacion.Tutorial;
using Waypoints;

namespace Modules.Module01_CableMaking.Presentation.Tutorial
{
    /// <summary>
    /// Construye la secuencia del tutorial para el módulo 01.
    /// No ejecuta el tutorial; únicamente registra los pasos
    /// dentro del TutorialDirector.
    /// </summary>
    public class Module01TutorialBuilder
    {
        public TutorialSequence Build(Waypoint cableWaypoint)
        {
            TutorialSequence sequence = new();
            
            sequence.AddStep(
                new WaitSecondsStep(5));

            sequence.AddStep(
                new DialogueStep(
                    "Bienvenido al laboratorio."
                ));

            sequence.AddStep(
                new WaitSecondsStep(1));

            sequence.AddStep(
                new MoveNpcStep(cableWaypoint));

            sequence.AddStep(
                new DialogueStep(
                    "Acércate al cable. Comenzaremos pelando la cubierta."
                ));

            return sequence;
        }
    }
}