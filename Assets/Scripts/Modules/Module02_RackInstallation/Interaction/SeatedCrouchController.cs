using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>
    /// Agachado artificial opcional para pruebas sentadas. Desplaza Camera Offset,
    /// no la cámara rastreada, y alterna postura con el clic del joystick derecho.
    /// </summary>
    public sealed class SeatedCrouchController : MonoBehaviour
    {
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private InputActionReference crouchAction;
        [Min(0.1f)] [SerializeField] private float crouchDepth = 0.55f;
        [Min(0.01f)] [SerializeField] private float transitionDuration = 0.18f;
        [SerializeField] private bool startCrouched;

        private InputAction fallbackAction;
        private Transform offsetTransform;
        private float standingLocalY;
        private bool initialized;
        public bool IsCrouched { get; private set; }
        private InputAction Action => crouchAction != null ? crouchAction.action : fallbackAction;

        private void Awake()
        {
            if (xrOrigin == null)
                xrOrigin = GetComponentInParent<XROrigin>();
            if (crouchAction == null)
            {
                fallbackAction = new InputAction("Seated Crouch R3", InputActionType.Button);
                fallbackAction.AddBinding("<XRController>{RightHand}/primary2DAxisClick");
            }
        }

        private void Start()
        {
            if (xrOrigin == null || xrOrigin.CameraFloorOffsetObject == null)
            {
                Debug.LogError("SeatedCrouchController necesita un XR Origin con Camera Floor Offset GameObject.", this);
                enabled = false;
                return;
            }
            offsetTransform = xrOrigin.CameraFloorOffsetObject.transform;
            standingLocalY = offsetTransform.localPosition.y;
            initialized = true;
            IsCrouched = startCrouched;
        }

        private void OnEnable()
        {
            if (Action == null) return;
            Action.performed += HandlePerformed;
            Action.Enable();
        }

        private void OnDisable()
        {
            if (Action != null)
            {
                Action.performed -= HandlePerformed;
                if (fallbackAction != null) Action.Disable();
            }
            if (initialized && offsetTransform != null)
            {
                Vector3 position = offsetTransform.localPosition;
                position.y = standingLocalY;
                offsetTransform.localPosition = position;
            }
        }

        private void Update()
        {
            if (!initialized || offsetTransform == null) return;
            float targetY = standingLocalY - (IsCrouched ? crouchDepth : 0f);
            float speed = crouchDepth / Mathf.Max(0.01f, transitionDuration);
            Vector3 position = offsetTransform.localPosition;
            position.y = Mathf.MoveTowards(position.y, targetY, speed * Time.unscaledDeltaTime);
            offsetTransform.localPosition = position;
        }

        private void HandlePerformed(InputAction.CallbackContext _) => IsCrouched = !IsCrouched;

        private void OnDestroy() => fallbackAction?.Dispose();
    }
}
