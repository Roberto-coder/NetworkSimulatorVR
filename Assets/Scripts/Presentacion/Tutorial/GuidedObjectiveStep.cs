using System;
using System.Collections;
using Core.Objectives;
using GameData.Objectives;
using Modules.Module01_CableMaking.Flow;
using UnityEngine;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Presenta la instrucción de un objetivo y espera su finalización.
    /// B puede ocultar la instrucción, pero no omite el objetivo. Si el usuario
    /// completa el objetivo con el diálogo abierto, el step avanza solo.
    /// </summary>
    public sealed class GuidedObjectiveStep : TutorialStep
    {
        private readonly string objectiveId;
        private readonly string instruction;
        private readonly string speaker;

        public GuidedObjectiveStep(
            string objectiveId,
            string instruction,
            string speaker = "Instructor")
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
                throw new ArgumentException(
                    "El identificador del objetivo es obligatorio.",
                    nameof(objectiveId));

            this.objectiveId = objectiveId;
            this.instruction = instruction;
            this.speaker = speaker;
        }

        public override IEnumerator Execute(TutorialDirector director)
        {
            ModuleFlowController flow = director.FlowController;
            if (flow == null)
            {
                Debug.LogError(
                    $"No se puede guiar el objetivo '{objectiveId}' sin ModuleFlowController.");
                yield break;
            }

            ObjectiveBase targetObjective = FindObjective(flow);
            if (targetObjective == null)
            {
                Debug.LogError(
                    $"El objetivo '{objectiveId}' no existe en la definición del módulo.");
                yield break;
            }

            if (targetObjective.IsCompleted)
                yield break;

            bool completed = false;

            void HandleObjectiveCompleted(ObjectiveData objective)
            {
                if (objective != null && objective.Id == objectiveId)
                    completed = true;
            }

            flow.ObjectiveCompleted += HandleObjectiveCompleted;

            try
            {
                yield return director.DialogueController.ShowDialogueUntilConfirmed(
                    instruction,
                    speaker,
                    () => completed || targetObjective.IsCompleted);

                while (!completed && !targetObjective.IsCompleted)
                    yield return null;
            }
            finally
            {
                flow.ObjectiveCompleted -= HandleObjectiveCompleted;
            }
        }

        private ObjectiveBase FindObjective(ModuleFlowController flow)
        {
            foreach (ObjectiveBase objective in flow.Objectives)
            {
                if (objective.Data.Id == objectiveId)
                    return objective;
            }

            return null;
        }
    }
}
