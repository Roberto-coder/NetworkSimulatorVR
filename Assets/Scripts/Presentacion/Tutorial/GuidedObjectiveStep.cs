using System;
using System.Collections;
using Core.Objectives;
using GameData.Objectives;
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
        private readonly GameData.NPC.DialogueAudio audio;
        private readonly string legacyVoiceId;
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

        public GuidedObjectiveStep(string objectiveId, GameData.NPC.NPCDialogueLine line)
            : this(objectiveId, line.text, line.speaker)
        { audio = line.audio; legacyVoiceId = line.legacyVoiceId; }

        public GuidedObjectiveStep(string objectiveId, string instruction, GameData.NPC.DialogueAudio audio)
            : this(objectiveId, instruction) { this.audio = audio; }

        public override IEnumerator Execute(TutorialDirector director)
        {
            yield return director.WaitForReactions();
            IObjectiveFlow flow = director.FlowController;
            if (flow == null)
            {
                Debug.LogError(
                    $"No se puede guiar el objetivo '{objectiveId}' sin IObjectiveFlow.");
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
                    instruction, speaker, () => completed || targetObjective.IsCompleted,
                    audio, legacyVoiceId);

                while (!completed && !targetObjective.IsCompleted)
                    yield return null;
            }
            finally
            {
                flow.ObjectiveCompleted -= HandleObjectiveCompleted;
            }
        }

        private ObjectiveBase FindObjective(IObjectiveFlow flow)
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
