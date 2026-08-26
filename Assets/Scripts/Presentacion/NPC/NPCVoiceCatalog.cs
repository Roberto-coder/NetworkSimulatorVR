using System;
using System.Collections.Generic;
using UnityEngine;

namespace Presentacion.NPC
{
    [CreateAssetMenu(fileName = "NPCVoiceCatalog", menuName = "Network Simulator/NPC Voice Catalog")]
    public sealed class NPCVoiceCatalog : ScriptableObject
    {
        [Serializable]
        private struct VoiceEntry
        {
            public string id;
            public AudioClip clip;
        }

        [SerializeField] private List<VoiceEntry> voices = new();

        public bool TryGetClip(string id, out AudioClip clip)
        {
            clip = null;
            if (string.IsNullOrWhiteSpace(id))
                return false;

            foreach (VoiceEntry voice in voices)
            {
                if (!string.Equals(voice.id, id, StringComparison.OrdinalIgnoreCase))
                    continue;
                clip = voice.clip;
                return clip != null;
            }
            return false;
        }
    }
}
