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
        private readonly GameData.NPC.DialogueAudio _audio;
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

        public DialogueStep(GameData.NPC.NPCDialogueLine line)
            : this(line.text, line.speaker, line.legacyVoiceId) { _audio = line.audio; }

        public DialogueStep(string message, GameData.NPC.DialogueAudio audio)
            : this(message) { _audio = audio; }

        public override IEnumerator Execute(TutorialDirector director)
        {
            yield return director.WaitForReactions();
            yield return director.DialogueController.ShowDialogueUntilConfirmed(
                _message, _speaker, audio: _audio, legacyVoiceId: _voiceId);
        }
    }
}
