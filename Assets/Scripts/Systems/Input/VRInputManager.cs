using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.Input
{
    public class VRInputManager : MonoBehaviour
    {
        public static VRInputManager Instance;

        [SerializeField] InputActionReference menuButton;
        [SerializeField] InputActionReference trigger;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            menuButton.action.performed += OnMenu;
            trigger.action.performed += OnTrigger;
        }

        private void OnDisable()
        {
            menuButton.action.performed -= OnMenu;
            trigger.action.performed -= OnTrigger;
        }

        void OnMenu(InputAction.CallbackContext ctx)
        {
            InputEvents.MenuPressed();
        }

        void OnTrigger(InputAction.CallbackContext ctx)
        {
            InputEvents.TriggerPressed();
        }
    }
}