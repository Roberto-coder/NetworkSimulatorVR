using System.Collections;
using GameData.Objectives;
using Modules.Module01_CableMaking.Flow;
using Modules.Module01_CableMaking.Flow.Validation;
using Presentacion.Tutorial;
using UnityEngine;

namespace Presentacion.NPC
{
    /// <summary>
    /// Ejecuta recordatorios no bloqueantes y reacciones de finalización del NPC.
    /// Animación y audio son opcionales para permitir una integración incremental.
    /// </summary>
    public sealed class NPCReactionController : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private NPCDialogueController dialogueController;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;

        [Header("Reminder")]
        [SerializeField, Min(0f)] private float messageDuration = 4f;
        [SerializeField] private string reminderAnimationTrigger;
        [SerializeField] private AudioClip reminderClip;

        [Header("Objective completed")]
        [SerializeField] private string completedAnimationTrigger;
        [SerializeField] private AudioClip completedClip;

        [Header("Alert")]
        [SerializeField] private string alertAnimationTrigger;
        [SerializeField] private AudioClip alertClip;

        private ModuleFlowController flow;
        private TutorialDirector tutorialDirector;
        private ObjectiveData currentObjective;
        private float inactiveTime;
        private bool reminderPlaying;
        private bool alertPlaying;
        private string pendingAlert;

        public void Configure(
            ModuleFlowController moduleFlow,
            TutorialDirector director)
        {
            Unsubscribe();

            flow = moduleFlow;
            tutorialDirector = director;
            currentObjective = flow?.CurrentObjectiveData;
            inactiveTime = 0f;

            if (flow == null)
                return;

            flow.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
            flow.ObjectiveCompleted += HandleObjectiveCompleted;
            flow.ActionValidator.ActionRejected += HandleActionRejected;
        }

        private void Update()
        {
            if (currentObjective == null || tutorialDirector == null ||
                !tutorialDirector.IsRunning || dialogueController == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(pendingAlert) &&
                !alertPlaying && !dialogueController.IsDialogueActive)
            {
                StartCoroutine(PlayAlert());
                return;
            }

            if (reminderPlaying || alertPlaying || dialogueController.IsDialogueActive)
                return;

            inactiveTime += Time.deltaTime;

            if (currentObjective.ReminderInterval <= 0f ||
                inactiveTime < currentObjective.ReminderInterval)
            {
                return;
            }

            inactiveTime = 0f;
            StartCoroutine(PlayReminder(currentObjective));
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAllCoroutines();
            reminderPlaying = false;
            alertPlaying = false;
            pendingAlert = null;
        }

        private void HandleCurrentObjectiveChanged(ObjectiveData objective)
        {
            currentObjective = objective;
            inactiveTime = 0f;
        }

        private void HandleObjectiveCompleted(ObjectiveData objective)
        {
            inactiveTime = 0f;
            PlayFeedback(completedAnimationTrigger, completedClip);
        }

        private void HandleActionRejected(ModuleInteractionError error)
        {
            if (error == null || string.IsNullOrWhiteSpace(error.Message))
                return;

            pendingAlert = error.Message;
            inactiveTime = 0f;
        }

        private IEnumerator PlayReminder(ObjectiveData objective)
        {
            if (objective == null || dialogueController.IsDialogueActive)
                yield break;

            string message = string.IsNullOrWhiteSpace(objective.ReminderDialogue)
                ? objective.Description
                : objective.ReminderDialogue;

            if (string.IsNullOrWhiteSpace(message))
                yield break;

            reminderPlaying = true;
            PlayFeedback(reminderAnimationTrigger, reminderClip);

            yield return dialogueController.ShowTransientDialogue(
                message,
                messageDuration);

            reminderPlaying = false;
        }

        private IEnumerator PlayAlert()
        {
            string message = pendingAlert;
            pendingAlert = null;
            alertPlaying = true;

            PlayFeedback(alertAnimationTrigger, alertClip);
            yield return dialogueController.ShowTransientDialogue(
                message,
                messageDuration);

            alertPlaying = false;
        }

        private void PlayFeedback(string animationTrigger, AudioClip clip)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(animationTrigger))
                animator.SetTrigger(animationTrigger);

            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void Unsubscribe()
        {
            if (flow == null)
                return;

            flow.CurrentObjectiveChanged -= HandleCurrentObjectiveChanged;
            flow.ObjectiveCompleted -= HandleObjectiveCompleted;
            flow.ActionValidator.ActionRejected -= HandleActionRejected;
            flow = null;
        }
    }
}
