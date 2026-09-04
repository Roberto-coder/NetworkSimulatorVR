using Presentacion.GlobalUI.RadialSelectorTool;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.Module02_RackInstallation.Presentation
{
    /// <summary>Abre la rueda con el botón secundario izquierdo usando Input System/XRI.</summary>
    public sealed class XriRadialMenuInput : MonoBehaviour
    {
        [SerializeField] private RadialMenuController radialMenu;
        [SerializeField] private InputActionReference menuAction;
        private InputAction fallbackAction;
        private InputAction Action => menuAction != null ? menuAction.action : fallbackAction;

        private void Awake()
        {
            if (radialMenu == null)
                radialMenu = GetComponent<RadialMenuController>();
            if (menuAction == null)
            {
                fallbackAction = new InputAction("ToolMenu", InputActionType.Button);
                fallbackAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
            }
        }

        private void OnEnable()
        {
            if (Action == null) return;
            Action.started += HandleStarted;
            Action.canceled += HandleCanceled;
            Action.Enable();
        }

        private void OnDisable()
        {
            if (Action == null) return;
            Action.started -= HandleStarted;
            Action.canceled -= HandleCanceled;
            if (fallbackAction != null) Action.Disable();
        }

        private void OnDestroy() => fallbackAction?.Dispose();
        private void HandleStarted(InputAction.CallbackContext _) => radialMenu?.ShowMenu();
        private void HandleCanceled(InputAction.CallbackContext _) => radialMenu?.ConfirmSelection();
    }
}
