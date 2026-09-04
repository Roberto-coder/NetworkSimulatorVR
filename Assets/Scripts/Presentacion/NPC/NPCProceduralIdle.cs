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
            if (!IsIdleActive())
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

        private void OnDisable() => RestoreBasePose();

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
