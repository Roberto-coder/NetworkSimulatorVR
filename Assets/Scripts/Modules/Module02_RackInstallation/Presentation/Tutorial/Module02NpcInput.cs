using Presentacion.NPC;
using Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.Module02_RackInstallation.Presentation.Tutorial
{
    /// <summary>Confirma el diálogo con B (XRI); Enter permite comprobarlo en el Editor.</summary>
    public sealed class Module02NpcInput : MonoBehaviour
    {
        [SerializeField] private NPCDialogueController dialogue;
        private InputAction confirm;
        private void Awake()
        {
            confirm = new InputAction("ConfirmNpcDialogue", InputActionType.Button);
            confirm.AddBinding("<XRController>{RightHand}/secondaryButton");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            confirm.AddBinding("<Keyboard>/enter");
#endif
        }
        private void OnEnable() { confirm.performed += OnConfirm; confirm.Enable(); }
        private void OnDisable() { confirm.performed -= OnConfirm; confirm.Disable(); }
        private void OnDestroy() => confirm.Dispose();
        private void Update()
        {
            var input = VRInputManager.Instance;
            if (input != null && input.ConfirmPressed && dialogue != null && !dialogue.UsesCentralInput)
                dialogue.Confirm();
        }
        private void OnConfirm(InputAction.CallbackContext _)
        {
            if (VRInputManager.Instance == null) dialogue?.Confirm();
        }
    }
}
