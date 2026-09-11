using UnityEngine;

namespace Presentacion.Quiz
{
    [DisallowMultipleComponent]
    public sealed class AchievementImagePulse : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.25f)] private float amplitude = 0.08f;
        [SerializeField, Min(0.1f)] private float duration = 1.6f;

        private Vector3 baseScale;
        private float elapsed;

        private void OnEnable()
        {
            baseScale = transform.localScale;
            elapsed = 0f;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float wave = Mathf.Sin(elapsed * 2f * Mathf.PI / Mathf.Max(0.1f, duration));
            transform.localScale = baseScale * (1f + amplitude * wave);
        }

        private void OnDisable()
        {
            transform.localScale = baseScale;
        }
    }
}
