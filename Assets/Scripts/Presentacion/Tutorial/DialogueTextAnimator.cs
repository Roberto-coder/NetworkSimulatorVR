using System.Collections;
using TMPro;
using Systems.Input;
using System.Text.RegularExpressions;
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

        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Escribe el mensaje carácter por carácter.
        /// La corrutina finaliza cuando el texto terminó de mostrarse.
        /// </summary>
        public IEnumerator Play(string message)
        {
            _skipRequested = false;
            IsPlaying = true;

            dialogueText.text = FormatControls(message);

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
            IsPlaying = false;
        }

        /// <summary>
        /// Completa inmediatamente la escritura.
        /// </summary>
        private string FormatControls(string message)
        {
            if (string.IsNullOrEmpty(message) || (!message.Contains("{control:") && !message.Contains("{boton:"))) return message;
            var icons = Resources.Load<TMP_SpriteAsset>("ControlIcons/LobbyControls");
            if (icons != null) { dialogueText.spriteAsset = icons; dialogueText.richText = true; }
            message = Regex.Replace(message, @"\{boton:(A|B|X|Y|L1|L2|R1|R2|SL2|SR2)\}", match =>
                icons != null ? "<sprite name=\"" + match.Groups[1].Value + "\" tint=0>" : match.Groups[1].Value);
            return Regex.Replace(message, @"\{control:(confirmar|pausa|herramientas|usar|interactuar)\}", match =>
            {
                string action = match.Groups[1].Value;
                var input = VRInputManager.Instance;
                if (input == null) return action;
                string icon = input.GetControlIcon(action);
                if (icons != null && Regex.IsMatch(icon, @"^(A|B|X|Y|L1|L2|R1|R2|SL2|SR2)$"))
                    return "<sprite name=\"" + icon + "\" tint=0>";
                return icon.Replace("<", "").Replace(">", "");
            });
        }

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
            IsPlaying = false;

            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
        }
    }
}
