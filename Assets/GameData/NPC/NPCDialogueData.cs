using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData.NPC
{
    [Serializable]
    public sealed class NPCDialogueLine
    {
        public string id;
        [TextArea(2, 8)] public string text;
        public string speaker = "Instructor";
        public DialogueAudio audio = new();
        [Tooltip("Clave opcional del antiguo NPCVoiceCatalog. Solo se usa si Audio no resuelve un clip. No es el numero de paso.")]
        public string legacyVoiceId;
    }

    [CreateAssetMenu(menuName = "Network Simulator/NPC/Module Dialogues", fileName = "ModuleDialogues")]
    public sealed class NPCDialogueData : ScriptableObject
    {
        [SerializeField] private List<NPCDialogueLine> dialogues = new();
        public NPCDialogueLine Find(string id) => dialogues.Find(line => line != null && line.id == id);
        public NPCDialogueLine Get(string id) => Find(id)
            ?? throw new InvalidOperationException($"Falta el diálogo '{id}' en {name}.");
    }
}
