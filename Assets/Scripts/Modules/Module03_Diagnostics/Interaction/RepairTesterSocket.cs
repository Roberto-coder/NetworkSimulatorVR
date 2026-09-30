using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Snap XR para un plug libre. No es NetworkPort: no añade un enlace a la LAN.</summary>
    public sealed class RepairTesterSocket : XRSocketInteractor
    {
        public XRCableTester tester;
        public Connector SelectedConnector => firstInteractableSelected == null ? null :
            firstInteractableSelected.transform.GetComponent<Connector>();

        private bool IsFreePatchPlug(Transform candidate)
        {
            var plug = candidate.GetComponent<Connector>();
            return plug != null && plug.ConnectionType == Connector.ConType.Male && !plug.IsConnected &&
                tester != null && tester.Accepts(plug);
        }
        public override bool CanHover(IXRHoverInteractable interactable) =>
            base.CanHover(interactable) && IsFreePatchPlug(interactable.transform);
        public override bool CanSelect(IXRSelectInteractable interactable) =>
            base.CanSelect(interactable) && IsFreePatchPlug(interactable.transform);
    }
}
