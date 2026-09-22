using System.Collections;
using UnityEngine;

namespace Framework.Interaction.Tools
{
    
    // Si tus LEDs no usan emisión sino simplemente cambian de color,
    // únicamente cambia material.color.
    public class TesterAnimationController : MonoBehaviour
    {
        [SerializeField]
        private Renderer[] ledRenderers;

        [Tooltip("LEDs de Tester Remote en el mismo orden que los principales: 1 a 8.")]
        [SerializeField] private Renderer[] remoteLedRenderers;

        [Header("Approval audio")]
        [SerializeField] private AudioSource approvalAudioSource;
        [SerializeField] private AudioClip approvalClip;
        [Range(0f, 1f)]
        [SerializeField] private float approvalVolume = 0.7f;

        [SerializeField]
        private Color offColor = Color.black;

        [SerializeField]
        private Color onColor = Color.green;

        [SerializeField]
        private float ledDuration = 0.15f;

        private Coroutine animationRoutine;

        // private static readonly int EmissionColor =
        //     Shader.PropertyToID("_EmissionColor");

        [ContextMenu("Play Test Animation")]
        private void DebugPlayAnimation()
        {
            PlayTestAnimation();
        }
        
        private void Awake()
        {
            if (approvalAudioSource == null)
            {
                approvalAudioSource = gameObject.AddComponent<AudioSource>();
                approvalAudioSource.playOnAwake = false;
                approvalAudioSource.spatialBlend = 1f;
            }
            TurnOffAll();
        }

        private void OnDisable()
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);
            animationRoutine = null;
            TurnOffAll();
            if (approvalAudioSource != null)
                approvalAudioSource.Stop();
        }

        public void PlayTestAnimation()
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);

            if (approvalAudioSource != null)
                approvalAudioSource.Stop();

            animationRoutine = StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            TurnOffAll();

            int count = Mathf.Max(ledRenderers?.Length ?? 0, remoteLedRenderers?.Length ?? 0);
            for (int i = 0; i < count; i++)
            {
                SetPair(i, true);

                yield return new WaitForSeconds(ledDuration);

                SetPair(i, false);
            }

            if (count > 0 && approvalAudioSource != null && approvalClip != null)
                approvalAudioSource.PlayOneShot(approvalClip, approvalVolume);

            animationRoutine = null;
        }

        // private void SetLed(Renderer led, bool enabled)
        // {
        //     Material mat = led.material;
        //
        //     if (enabled)
        //     {
        //         mat.EnableKeyword("_EMISSION");
        //         mat.SetColor(EmissionColor, onColor);
        //     }
        //     else
        //     {
        //         mat.SetColor(EmissionColor, offColor);
        //     }
        // }
        private void SetLed(Renderer led, bool enabled)
        {
            if (led == null)
                return;

            Material mat = led.material;

            if (enabled)
                mat.color = onColor;
            else
                mat.color = offColor;
        }

        private void TurnOffAll()
        {
            int count = Mathf.Max(ledRenderers?.Length ?? 0, remoteLedRenderers?.Length ?? 0);
            for (int i = 0; i < count; i++)
                SetPair(i, false);
        }

        private void SetPair(int index, bool enabled)
        {
            if (ledRenderers != null && index < ledRenderers.Length)
                SetLed(ledRenderers[index], enabled);
            if (remoteLedRenderers != null && index < remoteLedRenderers.Length)
                SetLed(remoteLedRenderers[index], enabled);
        }
    }
}
