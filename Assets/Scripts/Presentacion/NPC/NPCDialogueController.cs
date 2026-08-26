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
    /// Se encarga únicamente de mostrar y ocultar
    /// el panel, delegando el efecto de escritura
    /// al DialogueTextAnimator.
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
        private int dialogueVersion;
        private Coroutine typingCoroutine;

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

            if (VRInputManager.Instance == null)
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
            Func<bool> externalAdvanceCondition = null)
        {
            int version = BeginDialogue();
            advanceRequested = false;
            isAwaitingConfirmation = true;

            dialoguePanel.SetActive(true);
            speakerName.text = speaker;

            typingCoroutine = StartCoroutine(textAnimator.Play(message));

            while (version == dialogueVersion && textAnimator.IsPlaying)
            {
                if (externalAdvanceCondition?.Invoke() ?? false)
                    textAnimator.Skip();

                yield return null;
            }

            if (version != dialogueVersion)
                yield break;

            while (!advanceRequested &&
                   !(externalAdvanceCondition?.Invoke() ?? false))
                yield return null;

            CompleteDialogue(version);
        }

        /// <summary>
        /// Muestra un mensaje breve que no necesita confirmación. Un diálogo
        /// principal posterior puede reemplazarlo sin competir por el panel.
        /// </summary>
        public IEnumerator ShowTransientDialogue(
            string message,
            float visibleTime = 4f,
            string speaker = "Instructor")
        {
            if (IsDialogueActive || string.IsNullOrWhiteSpace(message))
                yield break;

            int version = BeginDialogue();
            dialoguePanel.SetActive(true);
            speakerName.text = speaker;
            typingCoroutine = StartCoroutine(textAnimator.Play(message));

            while (version == dialogueVersion && textAnimator.IsPlaying)
                yield return null;

            float elapsed = 0f;
            while (version == dialogueVersion && elapsed < visibleTime)
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

        private void HandleConfirmPressed()
        {
            if (!isAwaitingConfirmation)
                return;

            if (textAnimator.IsPlaying)
            {
                textAnimator.Skip();
                return;
            }

            advanceRequested = true;
        }

        private void SubscribeToInput()
        {
            if (VRInputManager.Instance == null)
                return;

            VRInputManager.Instance.ConfirmPressedEvent -= HandleConfirmPressed;
            VRInputManager.Instance.ConfirmPressedEvent += HandleConfirmPressed;
        }

        private int BeginDialogue()
        {
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

            typingCoroutine = null;
            isAwaitingConfirmation = false;
            advanceRequested = false;
            IsDialogueActive = false;
            dialoguePanel.SetActive(false);
        }
    }
}
