using System;
using System.Collections;
using System.Collections.Generic;
using Modules.Module01_CableMaking.Flow;
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
        [SerializeField]
        private ModuleFlowController flowController;

        public ModuleFlowController FlowController => flowController;

        private TutorialSequence _sequence= new();
        
        public bool IsRunning { get; private set; }

        public int CurrentStepIndex { get; private set; } = -1;
        
        public TutorialStep CurrentStep { get; private set; }
        
        public event Action<TutorialStep> StepStarted;
        public event Action<TutorialStep> StepCompleted;
        public event Action TutorialFinished;



        public void SetSequence(TutorialSequence sequence)
        {
            _sequence = sequence;
        }

        /// <summary>
        /// Agrega un paso al final de la secuencia.
        /// </summary>
        public void AddStep(TutorialStep step)
        {
            if (step == null)
                return;

            _sequence.AddStep(step);
        }

        /// <summary>
        /// Elimina todos los pasos pendientes.
        /// </summary>
        public void ClearSteps()
        {
            _sequence.Clear();
        }

        /// <summary>
        /// Comienza la ejecución del tutorial.
        /// </summary>
        public void StartTutorial()
        {
            if (IsRunning)
                return;

                StartCoroutine(RunTutorial());
        }

        /// <summary>
        /// Detiene inmediatamente el tutorial.
        /// </summary>
        public void StopTutorial()
        {
            StopAllCoroutines();

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
                CurrentStepIndex = i;
                CurrentStep = _sequence.Steps[i];

                StepStarted?.Invoke(CurrentStep);

                yield return CurrentStep.Execute(this);

                StepCompleted?.Invoke(CurrentStep);
            }

            IsRunning = false;

            TutorialFinished?.Invoke();
        }
    }
}