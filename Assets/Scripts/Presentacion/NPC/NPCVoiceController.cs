using UnityEngine;
using Systems.Settings;

namespace Presentacion.NPC
{
    public sealed class NPCVoiceController : MonoBehaviour
    {
        [SerializeField] private NPCVoiceCatalog catalog;
        [SerializeField] private AudioSource audioSource;

        public int Version { get; private set; }
        public bool IsPlaying(int version) => version == Version && audioSource != null && audioSource.isPlaying;
        public void Stop(int version) { if (version == Version) Stop(); }

        public bool PlayClip(AudioClip clip)
        {
            Stop();
            if (clip == null || audioSource == null) return false;
            audioSource.clip = clip;
            RouteVoice();
            audioSource.Play();
            return true;
        }

        private void Awake()
        {
            if (catalog == null) catalog = Resources.Load<NPCVoiceCatalog>("NPCVoiceCatalog");
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            RouteVoice();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f;
        }

        public bool Play(string voiceId)
        {
            Stop();
            if (catalog == null || !catalog.TryGetClip(voiceId, out AudioClip clip))
                return false;

            audioSource.clip = clip;
            RouteVoice();
            audioSource.Play();
            return true;
        }

        public bool PlayAudio(GameData.NPC.DialogueAudio audio, string legacyVoiceId = null)
        {
            AudioClip clip = audio?.Resolve();
            if (clip == null) return Play(legacyVoiceId);
            Stop();
            audioSource.clip = clip;
            RouteVoice();
            audioSource.Play();
            return true;
        }

        private void RouteVoice()
        {
            if (GlobalSettingsManager.Instance != null)
                GlobalSettingsManager.Instance.RouteVoice(audioSource);
        }

        private void OnDisable() => Stop();

        public void Stop()
        {
            Version++;
            if (audioSource == null)
                return;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
