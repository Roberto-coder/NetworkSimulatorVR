using System.Collections;
using System;
using Core.Objectives;
using Presentacion.NPC;
using UnityEngine;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Coordina la ejecución secuencial de los pasos del tutorial.
    /// No conoce detalles de UI, movimiento o mecánicas.
    /// Simplemente ejecuta TutorialSteps.
    /// </summary>
    public class TutorialDirector : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField]
        private NPCDialogueController dialogueController;

        public NPCDialogueController DialogueController => dialogueController;
        [SerializeField]
        private NPCMovementController movementController;

        public NPCMovementController MovementController => movementController;
        public NPCPlayerLookController LookController { get; private set; }
        public NPCVoiceController VoiceController { get; private set; }
        private NPCReactionController reactions;
        public void ConfigureReactions(NPCReactionController controller) => reactions = controller;
        public IEnumerator WaitForReactions()
        {
            if (reactions != null) yield return reactions.WaitForReactions();
        }
        private IObjectiveFlow flowController;

        public IObjectiveFlow FlowController => flowController;

        private TutorialSequence _sequence= new();
        
        public bool IsRunning { get; private set; }
        public event Action TutorialCompleted;

        private void Awake()
        {
            ResolveNpcControllers();
        }

        public void SetSequence(TutorialSequence sequence)
        {
            _sequence = sequence;
        }

        public void SetFlowController(IObjectiveFlow controller)
        {
            flowController = controller;
        }

        /// <summary>
        /// Comienza la ejecución del tutorial.
        /// </summary>
        public void StartTutorial()
        {
            if (IsRunning)
                return;

            ResolveNpcControllers();
            StartCoroutine(RunTutorial());
        }

        private void OnDisable()
        {
            StopTutorial();
        }

        /// <summary>Cancela la narración y el movimiento; cancelar no equivale a completar.</summary>
        public void StopTutorial()
        {
            if (reactions != null) reactions.CancelReactions();
            StopAllCoroutines();
            if (movementController != null) movementController.Stop();
            if (VoiceController != null) VoiceController.Stop();
            if (dialogueController != null) dialogueController.HideImmediate();
            IsRunning = false;
        }

        /// <summary>
        /// Ejecuta los pasos secuencialmente.
        /// </summary>
        private IEnumerator RunTutorial()
        {
            if (_sequence == null)
            {
                Debug.LogWarning("No TutorialSequence assigned.");
                yield break;
            }

            IsRunning = true;
            
            for (int i = 0; i < _sequence.Count; i++)
            {
                if (reactions != null) yield return reactions.WaitForReactions();
                yield return _sequence.Steps[i].Execute(this);
            }

            if (reactions != null) yield return reactions.WaitForReactions();
            IsRunning = false;
            TutorialCompleted?.Invoke();
        }

        private void ResolveNpcControllers()
        {
            if (movementController == null)
                return;

            if (LookController == null) LookController = movementController.GetComponent<NPCPlayerLookController>();
            if (VoiceController == null) VoiceController = movementController.GetComponent<NPCVoiceController>();
            if (VoiceController == null)
                VoiceController = movementController.gameObject.AddComponent<NPCVoiceController>();
            if (dialogueController != null) dialogueController.ConfigureVoice(VoiceController);
        }
    }
}
