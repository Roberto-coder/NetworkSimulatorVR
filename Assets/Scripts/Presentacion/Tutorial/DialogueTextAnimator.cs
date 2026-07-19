using System.Collections;
using TMPro;
using UnityEngine;

namespace Presentation.Tutorial
{
    /// <summary>
    /// Realiza el efecto de escritura utilizando
    /// maxVisibleCharacters de TextMeshPro.
    /// </summary>
    public class DialogueTextAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text dialogueText;

        [Header("Typing")]
        [SerializeField]
        [Min(0.005f)]
        private float characterDelay = 0.03f;

        private bool _skipRequested;

        /// <summary>
        /// Escribe el mensaje carácter por carácter.
        /// La corrutina finaliza cuando el texto terminó de mostrarse.
        /// </summary>
        public IEnumerator Play(string message)
        {
            _skipRequested = false;

            dialogueText.text = message;

            // Obligamos a TMP a generar la geometría antes de consultar
            // la cantidad de caracteres.
            dialogueText.ForceMeshUpdate();

            int totalCharacters = dialogueText.textInfo.characterCount;

            dialogueText.maxVisibleCharacters = 0;

            for (int i = 0; i <= totalCharacters; i++)
            {
                dialogueText.maxVisibleCharacters = i;

                if (_skipRequested)
                    break;

                yield return new WaitForSeconds(characterDelay);
            }

            // Garantiza que todo el texto sea visible al finalizar
            dialogueText.maxVisibleCharacters = totalCharacters;
        }

        /// <summary>
        /// Completa inmediatamente la escritura.
        /// </summary>
        public void Skip()
        {
            _skipRequested = true;
        }

        /// <summary>
        /// Limpia el texto mostrado.
        /// </summary>
        public void Clear()
        {
            _skipRequested = false;

            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
        }
    }
}