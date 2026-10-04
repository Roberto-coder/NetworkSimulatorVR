using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Systems.Input
{
    [DefaultExecutionOrder(-100)]
    public class VRInputManager : MonoBehaviour
    {
        public static VRInputManager Instance { get; private set; }

        public enum InputBackend { MetaOVR, OpenXR }
        [SerializeField] private InputBackend backend;
        [Header("OpenXR bindings")]
        [SerializeField] private string pauseBinding = "<XRController>{LeftHand}/primaryButton";
        [SerializeField] private string toolBinding = "<XRController>{LeftHand}/secondaryButton";
        [SerializeField] private string confirmBinding = "<XRController>{RightHand}/secondaryButton";
        [SerializeField] private string triggerBinding = "<XRController>{RightHand}/trigger";
        [SerializeField] private string crouchBinding = "<XRController>{RightHand}/primary2DAxisClick";
        // A se comparte entre módulos; B mantiene su función de confirmar diálogos.
        [SerializeField] private string interactBinding = "<XRController>{RightHand}/primaryButton";
        private InputAction interact;
        public bool InteractPressed => UsesOpenXR
            ? interact != null && interact.WasPressedThisFrame()
            : OVRInput.GetDown(interactButton, rightController);
        private InputAction pause, tool, confirm, trigger, crouch;
        public bool UsesOpenXR => backend == InputBackend.OpenXR;
        public bool CrouchPressed => UsesOpenXR && crouch != null && crouch.WasPressedThisFrame();
        public bool TriggerPressed => UsesOpenXR && trigger != null && trigger.WasPressedThisFrame();

        [Header("Meta OVR buttons")]
        public OVRInput.Button pauseButton = OVRInput.Button.Three;
        public OVRInput.Button toolSelectorButton = OVRInput.Button.Four;
        public OVRInput.Button confirmButton = OVRInput.Button.Two;
        public OVRInput.Button interactButton = OVRInput.Button.One;

        [Header("Controllers")]
        public OVRInput.Controller rightController =
            OVRInput.Controller.RTouch;

        public OVRInput.Controller leftController =
            OVRInput.Controller.LTouch;

        public event Action<float> RightTriggerChanged;

        public event Action RightTriggerPressed;

        public event Action RightTriggerReleased;

        public event Action ConfirmPressedEvent;

        public float RightTrigger { get; private set; }

        private bool previousPressed;

        /// <summary>
        /// Reproduce un pulso haptico corto en el mando indicado.
        /// </summary>
        public void PlayHaptic(
            OVRInput.Controller controller,
            float amplitude = 0.7f,
            float duration = 0.12f,
            float frequency = 1f)
        {
            if (controller == OVRInput.Controller.None || duration <= 0f)
                return;

            if (UsesOpenXR)
            {
                var node = controller == leftController ? XRNode.LeftHand : XRNode.RightHand;
                var device = InputDevices.GetDeviceAtXRNode(node);
                if (device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
                    device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), duration);
                return;
            }
            StartCoroutine(HapticRoutine(
                controller,
                Mathf.Clamp01(amplitude),
                duration,
                Mathf.Clamp01(frequency)));
        }

        public bool PausePressed =>
            UsesOpenXR ? (pause != null && pause.WasPressedThisFrame()) : OVRInput.GetDown(pauseButton);

        public bool ToolPressed =>
            UsesOpenXR ? (tool != null && tool.WasPressedThisFrame()) : OVRInput.GetDown(toolSelectorButton);

        public bool ToolHeld =>
            UsesOpenXR ? (tool != null && tool.IsPressed()) : OVRInput.Get(toolSelectorButton);

        public bool ToolReleased =>
            UsesOpenXR ? (tool != null && tool.WasReleasedThisFrame()) : OVRInput.GetUp(toolSelectorButton);

        public bool ConfirmPressed =>
            UsesOpenXR ? (confirm != null && confirm.WasPressedThisFrame()) : OVRInput.GetDown(confirmButton);

        // Names match the shared button sprites. Unknown bindings use readable text.
        public string GetControlIcon(string action)
        {
            if (!UsesOpenXR)
            {
                if (action == "usar") return rightController == OVRInput.Controller.LTouch ? "L2" : "R2";
                var button = action == "confirmar" ? confirmButton : action == "pausa" ? pauseButton : action == "interactuar" ? interactButton : toolSelectorButton;
                if (button == OVRInput.Button.One) return "A";
                if (button == OVRInput.Button.Two) return "B";
                if (button == OVRInput.Button.Three) return "X";
                if (button == OVRInput.Button.Four) return "Y";
                return button.ToString();
            }
            string binding = action == "confirmar" ? confirmBinding : action == "pausa" ? pauseBinding : action == "usar" ? triggerBinding : action == "interactuar" ? interactBinding : toolBinding;
            bool left = binding.Contains("{LeftHand}");
            bool right = binding.Contains("{RightHand}");
            if (!left && !right) return binding;
            if (binding.EndsWith("/primaryButton")) return left ? "X" : "A";
            if (binding.EndsWith("/secondaryButton")) return left ? "Y" : "B";
            if (binding.EndsWith("/trigger") || binding.EndsWith("/triggerPressed")) return left ? "L2" : "R2";
            if (binding.EndsWith("/grip") || binding.EndsWith("/gripPressed")) return left ? "L1" : "R1";
            return binding;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Only one active VRInputManager is allowed.", this);
                enabled = false;
                return;
            }
            Instance = this;
            if (!UsesOpenXR) return;
            pause = new InputAction("Pause", InputActionType.Button, pauseBinding);
            tool = new InputAction("ToolSelector", InputActionType.Button, toolBinding);
            confirm = new InputAction("Confirm", InputActionType.Button, confirmBinding);
            interact = new InputAction("Interact", InputActionType.Button, interactBinding);
            trigger = new InputAction("RightTrigger", InputActionType.Value, triggerBinding);
            crouch = new InputAction("Crouch", InputActionType.Button, crouchBinding);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            pause.AddBinding("<Keyboard>/escape");
            confirm.AddBinding("<Keyboard>/enter");
            interact.AddBinding("<Keyboard>/e");
#endif
        }

        private void OnEnable()
        {
            interact?.Enable(); pause?.Enable(); tool?.Enable(); confirm?.Enable(); trigger?.Enable(); crouch?.Enable();
        }

        private void OnDisable()
        {
            interact?.Disable(); pause?.Disable(); tool?.Disable(); confirm?.Disable(); trigger?.Disable(); crouch?.Disable();
            previousPressed = false;
            RightTrigger = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            interact?.Dispose(); pause?.Dispose(); tool?.Dispose(); confirm?.Dispose(); trigger?.Dispose(); crouch?.Dispose();
        }

        private void Update()
        {
            if (ConfirmPressed)
                ConfirmPressedEvent?.Invoke();

            RightTrigger = UsesOpenXR ? trigger.ReadValue<float>() :
                OVRInput.Get(
                    OVRInput.Axis1D.PrimaryIndexTrigger,
                    rightController);

            RightTriggerChanged?.Invoke(RightTrigger);

            bool pressed = RightTrigger >= .95f;

            if (pressed && !previousPressed)
                RightTriggerPressed?.Invoke();

            if (!pressed && previousPressed)
                RightTriggerReleased?.Invoke();

            previousPressed = pressed;
        }

        private static IEnumerator HapticRoutine(
            OVRInput.Controller controller,
            float amplitude,
            float duration,
            float frequency)
        {
            OVRInput.SetControllerVibration(frequency, amplitude, controller);
            yield return new WaitForSecondsRealtime(duration);
            OVRInput.SetControllerVibration(0f, 0f, controller);
        }
    }
}
