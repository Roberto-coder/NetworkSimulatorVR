using GameData.NPC;
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
            NPCDialogueData data,
            Waypoint pedestalWaypoint,
            Waypoint cableWaypoint,
            Waypoint puzzleWaypoint,
            Waypoint quizWaypoint)
        {
            TutorialSequence sequence = new();
            
            sequence.AddStep(
                new WaitSecondsStep(5));
            
            sequence.AddStep(
                new DialogueStep(data.Get("module01_bienvenida")));
            
            sequence.AddStep(
                new MoveNpcStep(pedestalWaypoint));

            sequence.AddStep(
                new DialogueStep(data.Get("module01_conductores")));
            
            sequence.AddStep(
                new GuidedObjectiveStep("select_cable", data.Get("module01_seleccionCable")));
            

            sequence.AddStep(
                new MoveNpcStep(cableWaypoint));

            sequence.AddStep(
                new DialogueStep(data.Get("module01_dialogue_03")));

            sequence.AddStep(new GuidedObjectiveStep("strip_left_end", data.Get("strip_left_end_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_04")));
            
            sequence.AddStep(new GuidedObjectiveStep("order_left_t568b", data.Get("order_left_t568b_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_05")));
            sequence.AddStep(new GuidedObjectiveStep("crimp_left_end", data.Get("crimp_left_end_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_06")));
            sequence.AddStep(new GuidedObjectiveStep("strip_right_end", data.Get("strip_right_end_instruction")));

            if (puzzleWaypoint != null)
                sequence.AddStep(new MoveNpcStep(puzzleWaypoint));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_07")));
            sequence.AddStep(new GuidedObjectiveStep("order_right_t568b", data.Get("order_right_t568b_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_08")));
            sequence.AddStep(new GuidedObjectiveStep("crimp_right_end", data.Get("crimp_right_end_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_09")));
            sequence.AddStep(new GuidedObjectiveStep("connect_tester", data.Get("connect_tester_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_10")));
            sequence.AddStep(new GuidedObjectiveStep("validate_cable", data.Get("validate_cable_instruction")));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_11")));

            if (quizWaypoint != null)
                sequence.AddStep(new MoveNpcStep(quizWaypoint));

            sequence.AddStep(new DialogueStep(data.Get("module01_dialogue_12")));

            return sequence;
        }
    }
}
