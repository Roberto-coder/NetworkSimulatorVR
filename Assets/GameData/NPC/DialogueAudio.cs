using System;
using UnityEngine;

namespace GameData.NPC
{
    [Serializable]
    public sealed class DialogueAudio
    {
        public AudioClip clip;
        [Tooltip("Ruta relativa a Resources, sin extensión. El clip directo tiene prioridad.")]
        public string resourcesPath;
        public AudioClip Resolve() => clip != null ? clip :
            string.IsNullOrWhiteSpace(resourcesPath) ? null : Resources.Load<AudioClip>(resourcesPath);
    }
}
