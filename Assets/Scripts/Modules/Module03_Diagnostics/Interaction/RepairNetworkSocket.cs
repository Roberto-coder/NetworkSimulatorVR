using System.Collections;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Shared.Cabling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>El snap XR conserva la unión Connector que observa la simulación de red.</summary>
    public sealed class RepairNetworkSocket : XRSocketInteractor
    {
        public NetworkPort port;
        private Connector selectedPlug;
        private bool Compatible(Transform candidate)
        {
            var plug = candidate.GetComponent<Connector>();
            return port != null && port.Socket != null && plug != null &&
                plug.CableOwner != null && plug.CableOwner.Kind == port.Kind &&
                (plug.ConnectedTo == port.Socket || port.Socket.CanConnect(plug));
        }
        public override bool CanHover(IXRHoverInteractable candidate) => base.CanHover(candidate) && Compatible(candidate.transform);
        public override bool CanSelect(IXRSelectInteractable candidate) => base.CanSelect(candidate) && Compatible(candidate.transform);
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            selectedPlug = args.interactableObject.transform.GetComponent<Connector>();
            StartCoroutine(ConnectAfterSelection(selectedPlug));
        }
        private IEnumerator ConnectAfterSelection(Connector plug)
        {
            // Esperar la restauración del Rigidbody y los callbacks del agarre anterior.
            yield return null;
            if (plug != selectedPlug || !hasSelection || !Compatible(plug.transform)) yield break;
            if (!plug.IsConnected) port.Socket.Connect(plug);
        }
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (selectedPlug != null && selectedPlug.ConnectedTo == port.Socket) selectedPlug.Disconnect();
            selectedPlug = null;
            base.OnSelectExiting(args);
        }
    }
}
