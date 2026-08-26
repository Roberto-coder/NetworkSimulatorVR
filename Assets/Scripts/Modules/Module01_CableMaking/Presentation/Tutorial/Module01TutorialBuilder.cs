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
        public TutorialSequence Build(
            Waypoint cableWaypoint,
            Waypoint puzzleWaypoint,
            Waypoint quizWaypoint)
        {
            TutorialSequence sequence = new();
            
            sequence.AddStep(
                new WaitSecondsStep(5));

            sequence.AddStep(
                new DialogueStep(
                    "Bienvenido al laboratorio."
                ));

            sequence.AddStep(
                new DialogueStep(
                    "En este módulo aprenderás a preparar, armar y comprobar un cable de red."
                ));

            sequence.AddStep(
                new MoveNpcStep(cableWaypoint));

            sequence.AddStep(
                new DialogueStep(
                    "El pelado consiste en retirar cuidadosamente una parte de la cubierta exterior sin dañar los conductores internos."
                ));

            sequence.AddStep(new GuidedObjectiveStep(
                "strip_left_end",
                "Selecciona la peladora, apunta al extremo izquierdo del cable y realiza la acción para retirar la cubierta."));

            sequence.AddStep(new DialogueStep(
                "Ahora debes ordenar los ocho conductores siguiendo la norma T568B, respetando la posición asignada a cada color."));
            
            sequence.AddStep(new GuidedObjectiveStep(
                "order_left_t568b",
                "Toma el extremo izquierdo preparado, abre el puzzle y coloca cada conductor en el orden T568B."));

            sequence.AddStep(new DialogueStep(
                "El ponchado fija los conductores dentro del conector RJ45 y crea el contacto eléctrico con sus terminales."));
            sequence.AddStep(new GuidedObjectiveStep(
                "crimp_left_end",
                "Selecciona la ponchadora y úsala sobre el conector del extremo izquierdo."));

            sequence.AddStep(new DialogueStep(
                "El segundo extremo necesita la misma preparación antes de ordenar sus conductores."));
            sequence.AddStep(new GuidedObjectiveStep(
                "strip_right_end",
                "Usa la peladora para retirar la cubierta del extremo derecho."));

            if (puzzleWaypoint != null)
                sequence.AddStep(new MoveNpcStep(puzzleWaypoint));

            sequence.AddStep(new DialogueStep(
                "Repite el orden T568B en el extremo derecho. Ambos conectores deben conservar exactamente la misma distribución."));
            sequence.AddStep(new GuidedObjectiveStep(
                "order_right_t568b",
                "Ordena los conductores del extremo derecho dentro del puzzle."));

            sequence.AddStep(new DialogueStep(
                "Con los conductores ya ordenados, falta asegurar el segundo conector."));
            sequence.AddStep(new GuidedObjectiveStep(
                "crimp_right_end",
                "Usa la ponchadora sobre el conector del extremo derecho."));

            sequence.AddStep(new DialogueStep(
                "Para comprobar el cable, primero conecta ambos extremos en los puertos correspondientes del tester."));
            sequence.AddStep(new GuidedObjectiveStep(
                "connect_tester",
                "Selecciona el tester y conecta los dos extremos del cable."));

            sequence.AddStep(new DialogueStep(
                "El tester verificará la continuidad y el orden de los ocho conductores para detectar conexiones incorrectas."));
            sequence.AddStep(new GuidedObjectiveStep(
                "validate_cable",
                "Inicia la prueba y revisa el resultado del cable."));

            sequence.AddStep(new DialogueStep(
                "Has completado todos los objetivos prácticos. Ahora realizarás un breve quiz para repasar el módulo."));

            if (quizWaypoint != null)
                sequence.AddStep(new MoveNpcStep(quizWaypoint));

            sequence.AddStep(new DialogueStep(
                "Responde todas las preguntas y entrega el quiz. Cualquier resultado completará el módulo."));

            return sequence;
        }
    }
}
