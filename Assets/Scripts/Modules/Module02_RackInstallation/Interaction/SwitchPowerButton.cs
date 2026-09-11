using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Interaction
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class SwitchPowerButton : MonoBehaviour
    {
        [SerializeField] private Module02SwitchPower power;
        private XRSimpleInteractable interaction;
        private void Awake() => interaction = GetComponent<XRSimpleInteractable>();
        private void OnEnable() => interaction.selectEntered.AddListener(Press);
        private void OnDisable() => interaction.selectEntered.RemoveListener(Press);
        private void Press(SelectEnterEventArgs _) => power?.TogglePower();
    }
}
