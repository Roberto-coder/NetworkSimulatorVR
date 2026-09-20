using System.Collections;
using UnityEngine;

namespace Presentacion.NPC
{
    /// <summary>
    /// Genera el reposo del robot sin AnimationClips: flotacion senoidal y
    /// rotacion continua de sus helices. Se pausa mientras el Animator ejecuta
    /// una reaccion para no sobrescribir los clips Feliz o Triste.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class NPCProceduralIdle : MonoBehaviour
    {
        [Header("Animator state")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool onlyWhileIdle = true;
        [SerializeField] private int bodyLayerIndex;
        [SerializeField] private string idleStateName = "Idle";

        [Header("Floating")]
        [Tooltip("Transform visual que sube y baja. No uses el objeto que controla el movimiento por waypoints.")]
        [SerializeField] private Transform floatingRoot;
        [SerializeField, Min(0f)] private float floatingAmplitude = 0.05f;
        [SerializeField, Min(0f)] private float floatingFrequency = 0.5f;
        [SerializeField] private float floatingPhase;

        [Header("Propellers")]
        [SerializeField] private Transform rightPropeller;
        [SerializeField] private Vector3 rightLocalAxis = Vector3.forward;
        [SerializeField] private float rightDegreesPerSecond = 720f;
        [SerializeField] private Transform leftPropeller;
        [SerializeField] private Vector3 leftLocalAxis = Vector3.forward;
        [SerializeField] private float leftDegreesPerSecond = -720f;

        [Header("Happy reaction")]
        [SerializeField, Min(0.1f)] private float happyDuration = 1.6f;
        [SerializeField, Min(0f)] private float bounceHeight = 0.18f;
        [Header("Sad reaction")]
        [SerializeField, Min(0.1f)] private float sadDuration = 1.8f;
        [SerializeField, Range(0f, 60f)] private float shakeAngle = 18f;
        [SerializeField, Range(0f, 60f)] private float lookDownAngle = 25f;
        public bool IsReacting { get; private set; }
        private Vector3 reactionPosition;
        private Quaternion reactionRotation;
        private bool restoreAnimator;

        public IEnumerator PlayHappy() => PlayReaction(true);
        public IEnumerator PlaySad() => PlayReaction(false);

        private IEnumerator PlayReaction(bool happy)
        {
            CancelReaction();
            if (floatingRoot == null || !isActiveAndEnabled) yield break;
            reactionPosition = floatingRoot.localPosition;
            reactionRotation = floatingRoot.localRotation;
            restoreAnimator = animator != null && animator.enabled;
            if (restoreAnimator) animator.enabled = false;
            IsReacting = true;
            float duration = Mathf.Max(0.1f, happy ? happyDuration : sadDuration);
            float time = 0f;
            try
            {
                while (time < duration && IsReacting && floatingRoot != null)
                {
                    float t = Mathf.Clamp01(time / duration);
                    float envelope = Mathf.Sin(Mathf.PI * t);
                    if (happy)
                    {
                        float bounce = Mathf.Abs(Mathf.Sin(2f * Mathf.PI * t)) * bounceHeight * (1f - 0.6f * t);
                        floatingRoot.localPosition = reactionPosition + Vector3.up * bounce;
                        floatingRoot.localRotation = reactionRotation * Quaternion.Euler(0f, 360f * Mathf.SmoothStep(0f, 1f, t), 0f);
                    }
                    else
                    {
                        float yaw = Mathf.Sin(6f * Mathf.PI * t) * shakeAngle * envelope;
                        floatingRoot.localRotation = reactionRotation * Quaternion.Euler(lookDownAngle * envelope, yaw, 0f);
                    }
                    time += Time.deltaTime;
                    yield return null;
                }
            }
            finally { CancelReaction(); }
        }

        public void CancelReaction()
        {
            if (!IsReacting) return;
            if (floatingRoot != null)
            {
                floatingRoot.localPosition = reactionPosition;
                floatingRoot.localRotation = reactionRotation;
            }
            if (restoreAnimator && animator != null) animator.enabled = true;
            restoreAnimator = false;
            IsReacting = false;
        }

        private Vector3 floatingBasePosition;
        private Quaternion rightBaseRotation;
        private Quaternion leftBaseRotation;
        private float elapsed;
        private bool initialized;

        private void Awake() => CaptureBasePose();

        private void OnEnable()
        {
            if (!initialized)
                CaptureBasePose();
        }

        private void LateUpdate()
        {
            if (IsReacting || !IsIdleActive())
                return;

            elapsed += Time.deltaTime;

            if (floatingRoot != null)
            {
                float radians = (elapsed * floatingFrequency + floatingPhase) *
                                Mathf.PI * 2f;
                floatingRoot.localPosition = floatingBasePosition +
                                             Vector3.up * (Mathf.Sin(radians) * floatingAmplitude);
            }

            ApplyPropellerRotation(
                rightPropeller,
                rightBaseRotation,
                rightLocalAxis,
                rightDegreesPerSecond);

            ApplyPropellerRotation(
                leftPropeller,
                leftBaseRotation,
                leftLocalAxis,
                leftDegreesPerSecond);
        }

        private void OnDisable() { CancelReaction(); RestoreBasePose(); }

        [ContextMenu("Capture current pose as idle base")]
        private void CaptureBasePose()
        {
            if (floatingRoot != null)
                floatingBasePosition = floatingRoot.localPosition;

            if (rightPropeller != null)
                rightBaseRotation = rightPropeller.localRotation;

            if (leftPropeller != null)
                leftBaseRotation = leftPropeller.localRotation;

            elapsed = 0f;
            initialized = true;
        }

        private bool IsIdleActive()
        {
            if (!onlyWhileIdle || animator == null || !animator.isActiveAndEnabled)
                return true;

            if (bodyLayerIndex < 0 || bodyLayerIndex >= animator.layerCount ||
                animator.IsInTransition(bodyLayerIndex))
            {
                return false;
            }

            return animator
                .GetCurrentAnimatorStateInfo(bodyLayerIndex)
                .IsName(idleStateName);
        }

        private void ApplyPropellerRotation(
            Transform propeller,
            Quaternion baseRotation,
            Vector3 localAxis,
            float degreesPerSecond)
        {
            if (propeller == null || localAxis.sqrMagnitude < 0.0001f)
                return;

            float angle = Mathf.Repeat(elapsed * degreesPerSecond, 360f);
            propeller.localRotation = baseRotation *
                                      Quaternion.AngleAxis(angle, localAxis.normalized);
        }

        private void RestoreBasePose()
        {
            if (!initialized)
                return;

            if (floatingRoot != null)
                floatingRoot.localPosition = floatingBasePosition;

            if (rightPropeller != null)
                rightPropeller.localRotation = rightBaseRotation;

            if (leftPropeller != null)
                leftPropeller.localRotation = leftBaseRotation;
        }
    }
}
