using System.Collections;
using Core.Objectives;
using GameData.NPC;
using GameData.Objectives;
using Modules.Module01_CableMaking.Flow;
using Modules.Module01_CableMaking.Flow.Validation;
using Presentacion.Tutorial;
using UnityEngine;

namespace Presentacion.NPC
{
    /// <summary>Serializa reacciones y bloquea el próximo step hasta terminar animación y voz.</summary>
    public sealed class NPCReactionController : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private NPCDialogueController dialogueController;
        [SerializeField] private Animator animator;
        // Referencia antigua conservada para detener fuentes que quedaron serializadas en escenas.
        [SerializeField, HideInInspector] private AudioSource audioSource;

        [Header("Procedural reactions")]
        [SerializeField] private bool useProceduralReactions = true;
        [SerializeField] private NPCProceduralIdle proceduralMotion;

        [Header("Reminder")]
        [SerializeField, Min(0f)] private float messageDuration = 4f;
        [SerializeField] private string reminderAnimationTrigger;
        [SerializeField] private AudioClip reminderClip;

        [Header("Objective completed")]
        [SerializeField] private string completedAnimationTrigger = "Feliz";
        [SerializeField] private AudioClip completedClip;
        [SerializeField] private string completedAnimationState = "Feliz";

        [Header("Alert")]
        [SerializeField] private string alertAnimationTrigger = "Triste";
        [SerializeField] private AudioClip alertClip;
        [SerializeField] private string alertAnimationState = "Triste";

        [Header("Animation timing")]
        [SerializeField, Min(0)] private int animationLayer;
        [SerializeField, Min(0.1f)] private float animationStartTimeout = 1f;
        [SerializeField, Min(1f)] private float animationTimeout = 30f;
        [SerializeField] private string idleAnimationState = "Idle";

        private System.Func<string, NPCDialogueLine> findDialogue;
        private IObjectiveFlow flow;
        private ModuleActionValidator actionValidator;
        private TutorialDirector tutorialDirector;
        private NPCVoiceController voice;
        private ObjectiveData currentObjective;
        private NPCDialogueLine pendingNarration;
        private string pendingAlert;
        private int pendingCompletions;
        private float inactiveTime;
        private bool running;
        private bool reminderPlaying;
        private bool ownsDialogue;
        private bool animationFinished = true;
        private int voiceVersion = -1;

        public bool IsBusy => running || pendingCompletions > 0 || !string.IsNullOrWhiteSpace(pendingAlert);

        public void Configure(IObjectiveFlow moduleFlow, TutorialDirector director,
            System.Func<string, NPCDialogueLine> findDialogue = null)
        {
            if (proceduralMotion == null) proceduralMotion = GetComponentInChildren<NPCProceduralIdle>(true);
            CancelReactions();
            Unsubscribe();
            if (tutorialDirector != null) tutorialDirector.ConfigureReactions(null);
            tutorialDirector = director;
            this.findDialogue = findDialogue;
            flow = moduleFlow;
            currentObjective = flow?.CurrentObjectiveData;
            inactiveTime = 0f;
            if (director != null)
            {
                dialogueController = director.DialogueController;
                voice = director.VoiceController;
                if (dialogueController != null) dialogueController.ConfigureVoice(voice);
                director.ConfigureReactions(this);
            }
            if (audioSource != null) audioSource.Stop();
            if (flow == null) return;
            flow.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
            flow.ObjectiveCompleted += HandleObjectiveCompleted;
            actionValidator = (moduleFlow as ModuleFlowController)?.ActionValidator;
            if (actionValidator != null) actionValidator.ActionRejected += HandleActionRejected;
        }

        private void Update()
        {
            if (tutorialDirector == null || !tutorialDirector.IsRunning || dialogueController == null) return;
            TryStartPending();
            if (IsBusy || currentObjective == null || dialogueController.IsDialogueActive) return;
            inactiveTime += Time.deltaTime;
            if (currentObjective.ReminderInterval <= 0f || inactiveTime < currentObjective.ReminderInterval) return;
            inactiveTime = 0f;
            var line = findDialogue?.Invoke("reminder_" + currentObjective.Id);
            string message = line != null ? line.text : string.IsNullOrWhiteSpace(currentObjective.ReminderDialogue)
                ? currentObjective.Description : currentObjective.ReminderDialogue;
            if (string.IsNullOrWhiteSpace(message)) return;
            running = reminderPlaying = true;
            StartCoroutine(PlayReaction(reminderAnimationTrigger, reminderAnimationTrigger,
                line?.audio?.Resolve() ?? reminderClip, message, line?.speaker, line?.legacyVoiceId));
        }

        public IEnumerator WaitForReactions()
        {
            // Un recordatorio no debe retrasar un nuevo paso principal.
            if (reminderPlaying)
            {
                string alert = pendingAlert;
                var narration = pendingNarration;
                CancelReactions();
                pendingAlert = alert;
                pendingNarration = narration;
            }
            while (isActiveAndEnabled && IsBusy)
            {
                TryStartPending();
                yield return null;
            }
        }

        private void TryStartPending()
        {
            if (!isActiveAndEnabled || running || dialogueController == null || dialogueController.IsDialogueActive) return;
            if (pendingCompletions > 0)
            {
                pendingCompletions--;
                running = true;
                StartCoroutine(PlayReaction(completedAnimationTrigger, completedAnimationState, completedClip));
            }
            else if (!string.IsNullOrWhiteSpace(pendingAlert))
            {
                string message = pendingNarration != null ? pendingNarration.text
                    .Replace("{objective}", currentObjective?.Title ?? "")
                    .Replace("{end}", currentObjective != null && currentObjective.Id.Contains("right") ? "derecho" : "izquierdo")
                    : pendingAlert;
                var line = pendingNarration;
                pendingAlert = null;
                pendingNarration = null;
                running = true;
                StartCoroutine(PlayReaction(alertAnimationTrigger, alertAnimationState,
                    line?.audio?.Resolve() ?? alertClip, message, line?.speaker, line?.legacyVoiceId));
            }
        }

        private void HandleCurrentObjectiveChanged(ObjectiveData objective)
        {
            currentObjective = objective;
            inactiveTime = 0f;
            // Un error del objetivo anterior ya no corresponde al nuevo objetivo.
            pendingAlert = null;
            pendingNarration = null;
        }

        private void HandleObjectiveCompleted(ObjectiveData objective)
        {
            if (!isActiveAndEnabled || tutorialDirector == null || !tutorialDirector.IsRunning) return;
            if (reminderPlaying) CancelReactions();
            pendingCompletions++;
            pendingAlert = null;
            pendingNarration = null;
            inactiveTime = 0f;
            // Cerrar primero la instrucción y su voz; la reacción adquiere el canal después.
            if (!running && dialogueController != null) dialogueController.HideImmediate();
            TryStartPending();
        }

        private void HandleActionRejected(ModuleInteractionError error)
        {
            if (error != null) NotifyNarrationRejected(error.Message,
                currentObjective == null ? null : "alert_" + error.Type);
        }

        public void NotifyActionRejected(string message) => NotifyNarrationRejected(message, null);
        public void NotifyNarrationRejected(string message, string dialogueId)
        {
            if (!isActiveAndEnabled || tutorialDirector == null || !tutorialDirector.IsRunning ||
                string.IsNullOrWhiteSpace(message)) return;
            pendingAlert = message;
            pendingNarration = dialogueId == null ? null : findDialogue?.Invoke(dialogueId);
            inactiveTime = 0f;
        }

        private IEnumerator PlayReaction(string trigger, string state, AudioClip clip,
            string message = null, string speaker = null, string legacyVoiceId = null)
        {
            animationFinished = false;
            StartCoroutine(PlayAnimation(trigger, state));
            try
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    ownsDialogue = true;
                    yield return dialogueController.ShowTransientDialogue(message, messageDuration,
                        speaker ?? "Instructor", clip, legacyVoiceId);
                    ownsDialogue = false;
                }
                else
                {
                    if (voice != null) voice.PlayClip(clip);
                    voiceVersion = voice != null ? voice.Version : -1;
                }
                while (!animationFinished || (voice != null && voice.IsPlaying(voiceVersion))) yield return null;
            }
            finally
            {
                if (voice != null) voice.Stop(voiceVersion);
                voiceVersion = -1;
                running = reminderPlaying = false;
            }
        }

        private IEnumerator PlayAnimation(string trigger, string state)
        {
            try
            {
                if (useProceduralReactions)
                {
                    if (proceduralMotion != null && proceduralMotion.isActiveAndEnabled)
                    {
                        if (trigger == completedAnimationTrigger) yield return proceduralMotion.PlayHappy();
                        else if (trigger == alertAnimationTrigger) yield return proceduralMotion.PlaySad();
                    }
                    yield break;
                }
                if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null ||
                    string.IsNullOrWhiteSpace(trigger)) yield break;
                bool hasTrigger = false;
                foreach (var parameter in animator.parameters)
                    if (parameter.name == trigger && parameter.type == AnimatorControllerParameterType.Trigger) hasTrigger = true;
                if (!hasTrigger || animationLayer >= animator.layerCount ||
                    string.IsNullOrWhiteSpace(state) || !animator.HasState(animationLayer, Animator.StringToHash(state)))
                {
                    Debug.LogWarning($"Reacción NPC sin trigger/estado válido: {trigger}/{state}.", this);
                    yield break;
                }
                animator.ResetTrigger(trigger);
                animator.SetTrigger(trigger);
                float elapsed = 0f;
                bool entered = false;
                while (animator.isActiveAndEnabled)
                {
                    bool transitioning = animator.IsInTransition(animationLayer);
                    bool current = animator.GetCurrentAnimatorStateInfo(animationLayer).IsName(state);
                    bool next = transitioning && animator.GetNextAnimatorStateInfo(animationLayer).IsName(state);
                    entered |= current || next;
                    if (entered && !current && !next && !transitioning) yield break;
                    elapsed += Time.deltaTime;
                    if ((!entered && elapsed >= animationStartTimeout) || elapsed >= animationTimeout)
                    {
                        Debug.LogWarning($"La reacción NPC {state} no inició o no regresó a reposo dentro del tiempo límite.", this);
                        ResetAnimation();
                        yield break;
                    }
                    yield return null;
                }
            }
            finally { animationFinished = true; }
        }

        public void CancelReactions()
        {
            bool wasRunning = running;
            StopAllCoroutines();
            if (ownsDialogue && dialogueController != null) dialogueController.HideImmediate();
            if (voice != null) voice.Stop(voiceVersion);
            if (audioSource != null) audioSource.Stop();
            if (proceduralMotion != null) proceduralMotion.CancelReaction();
            if (wasRunning && !useProceduralReactions) ResetAnimation();
            voiceVersion = -1;
            running = reminderPlaying = ownsDialogue = false;
            animationFinished = true;
            pendingCompletions = 0;
            pendingAlert = null;
            pendingNarration = null;
        }

        private void ResetAnimation()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var parameter in animator.parameters)
                if (parameter.type == AnimatorControllerParameterType.Trigger &&
                    (parameter.name == completedAnimationTrigger || parameter.name == alertAnimationTrigger ||
                     parameter.name == reminderAnimationTrigger)) animator.ResetTrigger(parameter.name);
            int idle = Animator.StringToHash(idleAnimationState);
            if (animator.isActiveAndEnabled && animationLayer < animator.layerCount && animator.HasState(animationLayer, idle))
                animator.Play(idle, animationLayer, 0f);
        }

        private void OnDisable()
        {
            CancelReactions();
            Unsubscribe();
            if (tutorialDirector != null) tutorialDirector.ConfigureReactions(null);
        }

        private void Unsubscribe()
        {
            if (flow == null) return;
            flow.CurrentObjectiveChanged -= HandleCurrentObjectiveChanged;
            flow.ObjectiveCompleted -= HandleObjectiveCompleted;
            if (actionValidator != null) actionValidator.ActionRejected -= HandleActionRejected;
            actionValidator = null;
            flow = null;
        }
    }
}
