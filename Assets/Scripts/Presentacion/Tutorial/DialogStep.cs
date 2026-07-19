using System.Collections;
using UnityEngine;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Paso del tutorial que muestra un diálogo en pantalla.
    /// Actualmente espera a que el diálogo finalice por tiempo.
    /// Más adelante podrá esperar confirmación del usuario.
    /// </summary>
    public class DialogueStep : TutorialStep
    {
        private readonly string _message;
        private readonly float _displayTime;
        private readonly string _speaker;

        /// <summary>
        /// Constructor del paso.
        /// </summary>
        /// <param name="_message">Texto a mostrar</param>
        /// <param name="_displayTime">Tiempo que muestra el texto</param>
        /// <param name="_speaker">Nombre del personaje</param> 
        public DialogueStep(
            string message,
            float displayTime = 2f,
            string speaker = "Instructor")
        {
            _message = message;
            _displayTime = displayTime;
            _speaker = speaker;
        }

        public override IEnumerator Execute(TutorialDirector director)
        {
            Debug.Log($"Director: {(director == null ? "NULL" : "OK")}");
            Debug.Log($"DialogueController: {(director?.DialogueController == null ? "NULL" : "OK")}");

            yield return director.DialogueController.ShowDialogue(
                _message,
                _displayTime,
                _speaker);
        }
    }
}