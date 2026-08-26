using System.Collections;
using UnityEngine;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Paso del tutorial que muestra un diálogo en pantalla.
    /// Espera confirmación del usuario antes de avanzar.
    /// </summary>
    public class DialogueStep : TutorialStep
    {
        private readonly string _message;
        private readonly string _speaker;
        private readonly string _voiceId;

        /// <summary>
        /// Constructor del paso.
        /// </summary>
        /// <param name="_message">Texto a mostrar</param>
        /// <param name="_speaker">Nombre del personaje</param> 
        public DialogueStep(
            string message,
            string speaker = "Instructor",
            string voiceId = null)
        {
            _message = message;
            _speaker = speaker;
            _voiceId = voiceId;
        }

        public override IEnumerator Execute(TutorialDirector director)
        {
            Debug.Log($"Director: {(director == null ? "NULL" : "OK")}");
            Debug.Log($"DialogueController: {(director?.DialogueController == null ? "NULL" : "OK")}");

            director.VoiceController?.Play(_voiceId);
            try
            {
                yield return director.DialogueController.ShowDialogueUntilConfirmed(
                    _message,
                    _speaker);
            }
            finally
            {
                director.VoiceController?.Stop();
            }
        }
    }
}
