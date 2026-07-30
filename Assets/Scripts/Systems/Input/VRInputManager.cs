using System;
using UnityEngine;

namespace Systems.Input
{
    public class VRInputManager : MonoBehaviour
    {
        public static VRInputManager Instance { get; private set; }

        [Header("Buttons")]
        public OVRInput.Button pauseButton = OVRInput.Button.Three;
        public OVRInput.Button toolSelectorButton = OVRInput.Button.Four;
        public OVRInput.Button confirmButton = OVRInput.Button.Two;

        [Header("Controllers")]
        public OVRInput.Controller rightController =
            OVRInput.Controller.RTouch;

        public OVRInput.Controller leftController =
            OVRInput.Controller.LTouch;

        public event Action<float> RightTriggerChanged;

        public event Action RightTriggerPressed;

        public event Action RightTriggerReleased;

        public float RightTrigger { get; private set; }

        private bool previousPressed;

        public bool PausePressed =>
            OVRInput.GetDown(pauseButton);

        public bool ToolPressed =>
            OVRInput.GetDown(toolSelectorButton);

        public bool ToolHeld =>
            OVRInput.Get(toolSelectorButton);

        public bool ToolReleased =>
            OVRInput.GetUp(toolSelectorButton);

        public bool ConfirmPressed =>
            OVRInput.GetDown(confirmButton);

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            RightTrigger =
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
    }
}