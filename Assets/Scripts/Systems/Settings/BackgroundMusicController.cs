using UnityEngine;
using UnityEngine.Audio;

namespace Systems.Settings
{
    public sealed class BackgroundMusicController : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            audioSource ??= GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.outputAudioMixerGroup = musicMixerGroup;
            audioSource.clip = musicClip;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
        }

        private void Start()
        {
            if (audioSource.clip != null && !audioSource.isPlaying)
                audioSource.Play();
        }
    }
}
