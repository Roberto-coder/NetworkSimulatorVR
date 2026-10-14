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
        [Tooltip("Detectar el giro usando la cara visible del canvas, incluida su rotación local.")]
        [SerializeField] private bool useCanvasOrientation = true;

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

        private CanvasGroup visibilityGroup;
        private bool isMenuVisible;
        public void Configure(Transform head, Transform wrist, GameObject canvas)
        {
            headTransform = head; wristTransform = wrist; wristMenuCanvas = canvas;
        }
        private bool pendingVisibility;
        private float pendingSince;

        private void Awake()
        {
            ResolveRuntimeCanvas();
            ResolveTrackingReferences();
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

        private void LateUpdate()
        {
            if (headTransform == null || wristTransform == null || wristMenuCanvas == null)
                return;

            bool desired = ShouldBeVisible();
            if (desired == isMenuVisible)
            {
                pendingVisibility = desired;
                pendingSince = Time.unscaledTime;
                ApplyVisibility(desired);
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
            Vector3 displayPosition = useCanvasOrientation && wristMenuCanvas != null
                ? wristMenuCanvas.transform.position : wristTransform.position;
            Vector3 wristToEyes = headTransform.position - displayPosition;
            if (wristToEyes.sqrMagnitude < 0.0001f)
                return false;

            wristToEyes.Normalize();
            Vector3 displayNormal = DisplayNormal();
            float facingAngle = Vector3.Angle(displayNormal, wristToEyes);

            Vector3 headToWrist = -wristToEyes;
            float lookAngle = Vector3.Angle(headTransform.forward, headToWrist);

            float facingLimit = isMenuVisible ? closeFacingAngle : openFacingAngle;
            float lookLimit = isMenuVisible ? closeLookAngle : openLookAngle;
            return facingAngle <= facingLimit && lookAngle <= lookLimit;
        }

        private Vector3 DisplayNormal() => useCanvasOrientation && wristMenuCanvas != null
            ? -wristMenuCanvas.transform.forward
            : wristTransform.TransformDirection(localDisplayNormal.normalized);

        private void ResolveTrackingReferences()
        {
            if (headTransform == null)
            {
                Camera camera = transform.root.GetComponentInChildren<Camera>();
                if (camera == null) camera = Camera.main;
                if (camera != null) headTransform = camera.transform;
            }
            if (wristTransform == null && wristMenuCanvas != null)
                wristTransform = wristMenuCanvas.transform;
        }

        private void SetVisible(bool visible)
        {
            isMenuVisible = visible;
            pendingVisibility = visible;
            pendingSince = Time.unscaledTime;
            ApplyVisibility(visible);
        }

        private void ApplyVisibility(bool visible)
        {
            if (wristMenuCanvas == null) return;
            if (transform == wristMenuCanvas.transform || transform.IsChildOf(wristMenuCanvas.transform))
            {
                // No desactivar nuestro propio Update: solo ocultar la presentación.
                if (!wristMenuCanvas.activeSelf) wristMenuCanvas.SetActive(true);
                if (visibilityGroup == null)
                    visibilityGroup = wristMenuCanvas.GetComponent<CanvasGroup>() ?? wristMenuCanvas.AddComponent<CanvasGroup>();
                visibilityGroup.alpha = visible ? 1 : 0;
                visibilityGroup.interactable = visibilityGroup.blocksRaycasts = visible;
                // Mantener el controlador activo, pero apagar el contenido real del panel.
                // Así no dependemos únicamente de alpha para ocultar la UI de Meta.
                Transform panel = wristMenuCanvas.transform.Find("ObjectivesPanel");
                if (panel != null && panel.gameObject.activeSelf != visible)
                    panel.gameObject.SetActive(visible);
            }
            else wristMenuCanvas.SetActive(visible);
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

            Vector3 normal = DisplayNormal();
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
