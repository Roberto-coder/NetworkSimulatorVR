using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.Input
{
    public class VRInputManager : MonoBehaviour
    {
        public static VRInputManager Instance { get; private set; }
        
        [Header("Buttons")]
        public OVRInput.Button pauseButton = OVRInput.Button.Three;
        public OVRInput.Button toolSelectorButton = OVRInput.Button.Four;
        public OVRInput.Button confirmButton = OVRInput.Button.Two;

        
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
    }
}
