using System;
using System.Collections;
using Presentation.Tutorial;
using Systems.Input;
using TMPro;
using UnityEngine;

namespace Presentacion.NPC
{
    /// <summary>
    /// Controla la interfaz de diálogo del NPC.
    /// Coordina panel y voz como una única sesión. Reemplazar u ocultar
    /// un diálogo cancela su audio sin afectar a una sesión posterior.
    /// </summary>
    public class NPCDialogueController : MonoBehaviour
    {
        [Header("UI")]

        [SerializeField]
        private GameObject dialoguePanel;

        [SerializeField]
        private TMP_Text speakerName;

        [Header("Animation")]

        [SerializeField]
        private DialogueTextAnimator textAnimator;

        [Header("Player-facing dialogue")]
        [SerializeField] private Transform lookTarget;
        [SerializeField] private bool faceOnlyOnHorizontalAxis = true;

        private bool isAwaitingConfirmation;
        private bool advanceRequested;
        // El prefab conserva OVR por defecto; los módulos XRI usan un adaptador propio.
        [SerializeField] private bool useLegacyInput = true;
        public bool UsesCentralInput => useLegacyInput;
        private int dialogueVersion;
        private Coroutine typingCoroutine;
        private NPCVoiceController voice;
        private int voiceVersion;
        public void ConfigureVoice(NPCVoiceController controller) => voice = controller;
        private void StopVoice() => voice?.Stop(voiceVersion);

        public bool IsDialogueActive { get; private set; }

        private void Awake()
        {
            HideImmediate();
        }

        private void OnEnable()
        {
            SubscribeToInput();
        }

        private void Start()
        {
            SubscribeToInput();

            if (useLegacyInput && VRInputManager.Instance == null)
                Debug.LogError("NPCDialogueController necesita un VRInputManager activo.", this);
        }

        private void OnDisable()
        {
            if (VRInputManager.Instance != null)
                VRInputManager.Instance.ConfirmPressedEvent -= HandleConfirmPressed;

            HideImmediate();
        }

        private void LateUpdate()
        {
            if (!IsDialogueActive || dialoguePanel == null || lookTarget == null)
                return;

            Vector3 direction = dialoguePanel.transform.position - lookTarget.position;
            if (faceOnlyOnHorizontalAxis)
                direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
                dialoguePanel.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        /// <summary>
        /// Mantiene el dialogo visible hasta recibir confirmacion. Una pulsacion
        /// durante la escritura completa el texto; la siguiente permite avanzar.
        /// </summary>
        public IEnumerator ShowDialogueUntilConfirmed(
            string message,
            string speaker = "Instructor",
            Func<bool> externalAdvanceCondition = null,
            GameData.NPC.DialogueAudio audio = null,
            string legacyVoiceId = null)
        {
            int version = BeginDialogue();
            voice?.PlayAudio(audio, legacyVoiceId);
            voiceVersion = voice != null ? voice.Version : 0;
            advanceRequested = false;
            isAwaitingConfirmation = true;

            dialoguePanel.SetActive(true);
            speakerName.text = speaker;

            typingCoroutine = StartCoroutine(textAnimator.Play(message));

            while (version == dialogueVersion && textAnimator.IsPlaying)
            {
                if (externalAdvanceCondition?.Invoke() ?? false)
                {
                    StopVoice();
                    textAnimator.Skip();
                }

                yield return null;
            }

            if (version != dialogueVersion)
                yield break;

            while (version == dialogueVersion && !advanceRequested &&
                   !(externalAdvanceCondition?.Invoke() ?? false))
                yield return null;

            CompleteDialogue(version);
        }

        /// <summary>
        /// Mensaje transitorio: espera el tiempo visible y la voz, o confirmación.
        /// Un diálogo principal posterior puede reemplazarlo sin competir por el panel.
        /// </summary>
        public IEnumerator ShowTransientDialogue(
            string message,
            float visibleTime = 4f,
            string speaker = "Instructor",
            AudioClip clip = null,
            string legacyVoiceId = null)
        {
            if (IsDialogueActive || string.IsNullOrWhiteSpace(message))
                yield break;

            int version = BeginDialogue();
            if (clip != null) voice?.PlayClip(clip);
            else voice?.Play(legacyVoiceId);
            voiceVersion = voice != null ? voice.Version : 0;
            isAwaitingConfirmation = true;
            dialoguePanel.SetActive(true);
            speakerName.text = speaker;
            typingCoroutine = StartCoroutine(textAnimator.Play(message));

            while (version == dialogueVersion && textAnimator.IsPlaying)
                yield return null;

            float elapsed = 0f;
            while (version == dialogueVersion && !advanceRequested &&
                   (elapsed < visibleTime || (voice?.IsPlaying(voiceVersion) ?? false)))
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            CompleteDialogue(version);
        }

        /// <summary>
        /// Oculta inmediatamente el panel.
        /// </summary>
        public void HideImmediate()
        {
            StopVoice();
            dialogueVersion++;
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            isAwaitingConfirmation = false;
            advanceRequested = false;
            IsDialogueActive = false;
            textAnimator.Clear();
            dialoguePanel.SetActive(false);
        }

        public void Confirm() => HandleConfirmPressed();

        private void HandleConfirmPressed()
        {
            if (!isAwaitingConfirmation)
                return;

            StopVoice();
            if (textAnimator.IsPlaying)
            {
                textAnimator.Skip();
                return;
            }

            advanceRequested = true;
        }

        private void SubscribeToInput()
        {
            if (!useLegacyInput || VRInputManager.Instance == null)
                return;

            VRInputManager.Instance.ConfirmPressedEvent -= HandleConfirmPressed;
            VRInputManager.Instance.ConfirmPressedEvent += HandleConfirmPressed;
        }

        private int BeginDialogue()
        {
            // Toda voz del NPC comparte un canal, incluso al reemplazar una reacción.
            voice?.Stop();
            advanceRequested = false;
            isAwaitingConfirmation = false;
            dialogueVersion++;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = null;
            textAnimator.Clear();
            IsDialogueActive = true;
            return dialogueVersion;
        }

        private void CompleteDialogue(int version)
        {
            if (version != dialogueVersion)
                return;

            StopVoice();
            typingCoroutine = null;
            isAwaitingConfirmation = false;
            advanceRequested = false;
            IsDialogueActive = false;
            dialoguePanel.SetActive(false);
        }
    }
}
