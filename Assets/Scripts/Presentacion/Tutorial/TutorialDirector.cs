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
            StopAllCoroutines();
            movementController?.Stop();
            VoiceController?.Stop();
            dialogueController?.HideImmediate();
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
                yield return _sequence.Steps[i].Execute(this);
            }

            IsRunning = false;
            TutorialCompleted?.Invoke();
        }

        private void ResolveNpcControllers()
        {
            if (movementController == null)
                return;

            LookController ??= movementController.GetComponent<NPCPlayerLookController>();
            VoiceController ??= movementController.GetComponent<NPCVoiceController>();
            if (VoiceController == null)
                VoiceController = movementController.gameObject.AddComponent<NPCVoiceController>();
        }
    }
}
