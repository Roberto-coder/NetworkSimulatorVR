using UnityEngine;

namespace Presentacion.NPC
{
    public sealed class NPCVoiceController : MonoBehaviour
    {
        [SerializeField] private NPCVoiceCatalog catalog;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            catalog ??= Resources.Load<NPCVoiceCatalog>("NPCVoiceCatalog");
            audioSource ??= GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        public bool Play(string voiceId)
        {
            Stop();
            if (catalog == null || !catalog.TryGetClip(voiceId, out AudioClip clip))
                return false;

            audioSource.clip = clip;
            audioSource.Play();
            return true;
        }

        public void Stop()
        {
            if (audioSource == null)
                return;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
