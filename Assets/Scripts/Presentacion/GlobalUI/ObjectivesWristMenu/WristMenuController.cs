using UnityEngine;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
    /// <summary>Shows the objectives when the wrist display faces the user's head.</summary>
    public sealed class WristMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform wristTransform;
        [SerializeField] private GameObject wristMenuCanvas;

        [Header("Display orientation")]
        [Tooltip("Local wrist axis perpendicular to the visible face of the menu. Use the gizmo to verify it points toward the user's eyes.")]
        [SerializeField] private Vector3 localDisplayNormal = Vector3.down;

        [Header("Comfort angles")]
        [Tooltip("Maximum angle between the display normal and the eyes to open the menu.")]
        [Range(0f, 90f)] [SerializeField] private float openFacingAngle = 60f;
        [Tooltip("Larger closing angle provides hysteresis and prevents flickering.")]
        [Range(0f, 120f)] [SerializeField] private float closeFacingAngle = 75f;
        [Tooltip("How far from the centre of view the wrist may be when opening.")]
        [Range(0f, 90f)] [SerializeField] private float openLookAngle = 45f;
        [Tooltip("Larger angle used once visible so small head movements do not close it.")]
        [Range(0f, 120f)] [SerializeField] private float closeLookAngle = 60f;
        [Tooltip("Condition must remain stable briefly before visibility changes.")]
        [Min(0f)] [SerializeField] private float activationDelay = 0.08f;

        [Header("Diagnostics")]
        [SerializeField] private bool debugLogs;

        private bool isMenuVisible;
        private bool pendingVisibility;
        private float pendingSince;

        private void Awake()
        {
            ResolveRuntimeCanvas();
            SetVisible(false);
        }

        private void Start()
        {
            // Initialize even though its Canvas was disabled in Awake.
            if (wristMenuCanvas != null)
            {
                WristObjectivesPresenter presenter =
                    wristMenuCanvas.GetComponentInChildren<WristObjectivesPresenter>(true);
                if (presenter != null)
                    presenter.Initialize();
            }

            if (debugLogs)
            {
                Debug.Log(
                    $"[WristMenu] head={(headTransform != null)}, wrist={(wristTransform != null)}, " +
                    $"canvas={(wristMenuCanvas != null)}.",
                    this);
            }
        }

        private void OnValidate()
        {
            closeFacingAngle = Mathf.Max(closeFacingAngle, openFacingAngle);
            closeLookAngle = Mathf.Max(closeLookAngle, openLookAngle);
            if (localDisplayNormal.sqrMagnitude < 0.001f)
                localDisplayNormal = Vector3.down;
        }

        private void Update()
        {
            if (headTransform == null || wristTransform == null || wristMenuCanvas == null)
                return;

            bool desired = ShouldBeVisible();
            if (desired == isMenuVisible)
            {
                pendingVisibility = desired;
                pendingSince = Time.unscaledTime;
                return;
            }

            if (desired != pendingVisibility)
            {
                pendingVisibility = desired;
                pendingSince = Time.unscaledTime;
                return;
            }

            if (Time.unscaledTime - pendingSince >= activationDelay)
                SetVisible(desired);
        }

        private bool ShouldBeVisible()
        {
            Vector3 wristToEyes = headTransform.position - wristTransform.position;
            if (wristToEyes.sqrMagnitude < 0.0001f)
                return false;

            wristToEyes.Normalize();
            Vector3 displayNormal = wristTransform.TransformDirection(localDisplayNormal.normalized);
            float facingAngle = Vector3.Angle(displayNormal, wristToEyes);

            Vector3 headToWrist = -wristToEyes;
            float lookAngle = Vector3.Angle(headTransform.forward, headToWrist);

            float facingLimit = isMenuVisible ? closeFacingAngle : openFacingAngle;
            float lookLimit = isMenuVisible ? closeLookAngle : openLookAngle;
            return facingAngle <= facingLimit && lookAngle <= lookLimit;
        }

        private void SetVisible(bool visible)
        {
            isMenuVisible = visible;
            pendingVisibility = visible;
            pendingSince = Time.unscaledTime;
            if (wristMenuCanvas != null)
                wristMenuCanvas.SetActive(visible);
        }

        private void ResolveRuntimeCanvas()
        {
            if (wristMenuCanvas != null && wristMenuCanvas.scene.IsValid())
                return;

            WristObjectivesPresenter[] presenters =
                transform.root.GetComponentsInChildren<WristObjectivesPresenter>(true);
            foreach (WristObjectivesPresenter presenter in presenters)
            {
                if (presenter != null && presenter.gameObject.scene == gameObject.scene)
                {
                    wristMenuCanvas = presenter.gameObject;
                    return;
                }
            }

            Debug.LogError(
                "WristMenuController no encontró una instancia de ObjectivesCanva dentro de PlayerRoot.",
                this);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (wristTransform == null)
                return;

            Vector3 normal = localDisplayNormal.sqrMagnitude > 0.001f
                ? wristTransform.TransformDirection(localDisplayNormal.normalized)
                : -wristTransform.up;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(wristTransform.position, normal * 0.15f);

            if (headTransform != null)
            {
                Gizmos.color = ShouldBeVisible() ? Color.green : Color.red;
                Gizmos.DrawLine(wristTransform.position, headTransform.position);
            }
        }
#endif
    }
}
