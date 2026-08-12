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
            TurnOffAll();
        }

        public void PlayTestAnimation()
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);

            animationRoutine = StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            TurnOffAll();

            foreach (Renderer led in ledRenderers)
            {
                SetLed(led, true);

                yield return new WaitForSeconds(ledDuration);

                SetLed(led, false);
            }

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
            Material mat = led.material;

            if (enabled)
                mat.color = onColor;
            else
                mat.color = offColor;
        }

        private void TurnOffAll()
        {
            foreach (Renderer led in ledRenderers)
                SetLed(led, false);
        }
    }
}