using System;

namespace Systems.Settings
{
    [Serializable]
    public sealed class UserSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float voiceVolume = 1f;
        public float sfxVolume = 1f;
        public float uiVolume = 1f;
        public bool musicEnabled = true;
    }
}
