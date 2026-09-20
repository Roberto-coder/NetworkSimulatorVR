using System.Collections;
using Core.Objectives;
using GameData.Module02;
using Presentacion.Tutorial;

namespace Modules.Module02_RackInstallation.Presentation.Tutorial
{
    /// <summary>Omite explicaciones antiguas si el usuario ya completó su objetivo físico.</summary>
    public sealed class Module02GuidedTutorialStep : TutorialStep
    {
        private readonly ObjectiveBase objective;
        private readonly Module02TutorialObjective narration;
        public Module02GuidedTutorialStep(ObjectiveBase objective, Module02TutorialObjective narration)
        { this.objective = objective; this.narration = narration; }

        public override IEnumerator Execute(TutorialDirector director)
        {
            yield return director.WaitForReactions();
            if (objective.IsCompleted) yield break;
            if (!string.IsNullOrWhiteSpace(narration?.explanation))
            {
                yield return director.DialogueController.ShowDialogueUntilConfirmed(
                    narration.explanation, "Instructor", () => objective.IsCompleted,
                    narration.explanationAudio);
            }
            if (objective.IsCompleted) yield break;
            string instruction = string.IsNullOrWhiteSpace(narration?.instruction)
                ? objective.Data.Description : narration.instruction;
            // B puede cerrar la instrucción; únicamente el flujo físico completa el objetivo.
            yield return new GuidedObjectiveStep(objective.Data.Id, instruction, narration?.instructionAudio).Execute(director);
        }
    }
}
