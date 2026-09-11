using System;
using Shared.Cabling;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    public sealed class Module02SwitchPower : MonoBehaviour
    {
        [SerializeField] private NetworkPort powerPort;
        [SerializeField] private string supplyAddress = "PDU-A/AC01";
        [SerializeField] private Renderer powerLed;
        private readonly SwitchPowerState state = new();
        private MaterialPropertyBlock block;
        private bool previousSupply, previousOn;
        public string DeviceId => powerPort != null ? powerPort.DeviceId : "SW1";
        public bool HasSupply => state.HasSupply;
        public bool IsOn => isActiveAndEnabled && state.IsOn;
        public event Action StateChanged;
        private void OnEnable() { RefreshSupply(); Publish(true); }
        private void OnDisable() { state.SetSupply(false); Publish(true); }
        private void Update() => RefreshSupply();
        public void RefreshSupply()
        {
            var plug = powerPort != null && powerPort.Socket != null ? powerPort.Socket.ConnectedTo : null;
            var cable = plug != null ? plug.CableOwner : null;
            bool supplied = false;
            if (cable != null && powerPort.isActiveAndEnabled && cable.Kind == NetworkPortKind.Power)
            {
                cable.RefreshLink();
                supplied = cable.Connects(supplyAddress, powerPort.Address);
            }
            state.SetSupply(isActiveAndEnabled && supplied); Publish(false);
        }
        [ContextMenu("Toggle power")]
        public void TogglePower()
        {
            if (!isActiveAndEnabled) return;
            RefreshSupply(); state.Toggle(); Publish(false);
        }
        private void Publish(bool force)
        {
            if (!force && previousSupply == state.HasSupply && previousOn == state.IsOn) return;
            previousSupply = state.HasSupply; previousOn = state.IsOn;
            if (powerLed != null)
            {
                block ??= new MaterialPropertyBlock(); powerLed.GetPropertyBlock(block);
                Color color = state.IsOn ? Color.green : new Color(0.025f, 0.035f, 0.025f);
                block.SetColor("_BaseColor", color); block.SetColor("_Color", color);
                block.SetColor("_EmissionColor", state.IsOn ? Color.green : Color.black);
                powerLed.SetPropertyBlock(block);
            }
            StateChanged?.Invoke();
        }
    }
}
