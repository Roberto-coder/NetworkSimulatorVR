using System.Collections;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Shared.Cabling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>El socket XR conserva las conexiones que observan los objetivos, LEDs y consola.</summary>
    public sealed class Module02NetworkSocket : XRSocketInteractor
    {
        public NetworkPort port;
        private Connector selectedPlug;

        protected override void Awake()
        {
            if (port == null) port = GetComponentInParent<NetworkPort>();
            base.Awake();
        }

        private bool Compatible(Transform candidate)
        {
            var plug = candidate != null ? candidate.GetComponent<Connector>() : null;
            return port != null && port.isActiveAndEnabled && port.Socket != null &&
                plug != null && plug.CableOwner != null && plug.CableOwner.Kind == port.Kind &&
                (plug.ConnectedTo == port.Socket || port.Socket.CanConnect(plug));
        }

        public override bool CanHover(IXRHoverInteractable candidate) =>
            base.CanHover(candidate) && Compatible(candidate.transform);

        public override bool CanSelect(IXRSelectInteractable candidate) =>
            base.CanSelect(candidate) && Compatible(candidate.transform);

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            selectedPlug = args.interactableObject.transform.GetComponent<Connector>();
            StartCoroutine(ConnectAfterSelection(selectedPlug));
        }

        private IEnumerator ConnectAfterSelection(Connector plug)
        {
            // XRI restaura el Rigidbody al terminar el agarre anterior.
            yield return null;
            if (plug == null || plug != selectedPlug || !hasSelection || !Compatible(plug.transform))
                yield break;
            if (!plug.IsConnected)
                port.Socket.Connect(plug);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (selectedPlug != null && port != null && selectedPlug.ConnectedTo == port.Socket)
                selectedPlug.Disconnect();
            selectedPlug = null;
            base.OnSelectExiting(args);
        }
    }
}
