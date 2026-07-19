using System.Collections;
using Presentation.Tutorial;
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

        [SerializeField]
        private TMP_Text dialogueText;

        [Header("Animation")]

        [SerializeField]
        private DialogueTextAnimator textAnimator;

        private void Awake()
        {
            HideImmediate();
        }

        /// <summary>
        /// Muestra un diálogo.
        /// </summary>
        public IEnumerator ShowDialogue(
            string message,
            float visibleTime = 2f,
            string speaker = "Instructor")
        {
            dialoguePanel.SetActive(true);

            speakerName.text = speaker;
            
            yield return textAnimator.Play(message);

            yield return new WaitForSeconds(visibleTime);

            dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// Oculta inmediatamente el panel.
        /// </summary>
        public void HideImmediate()
        {
            textAnimator.Clear();
            dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// Muestra el panel sin escribir texto.
        /// Será útil posteriormente.
        /// </summary>
        public void ShowPanel()
        {
            dialoguePanel.SetActive(true);
        }

        /// <summary>
        /// Oculta el panel.
        /// </summary>
        public void HidePanel()
        {
            textAnimator.Clear();
            dialoguePanel.SetActive(false);
        }
        
        public void SkipTyping()
        {
            textAnimator.Skip();
        }
    }
}