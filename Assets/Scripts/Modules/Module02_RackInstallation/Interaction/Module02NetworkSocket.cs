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
        private Transform connectionPose;

        protected override void Awake()
        {
            if (port == null) port = GetComponentInParent<NetworkPort>();
            base.Awake();
            connectionPose = new GameObject("ConnectionPose_Runtime").transform;
            connectionPose.SetParent(transform, false);
        }

        public override Transform GetAttachTransform(IXRInteractable interactable)
        {
            var plug = interactable?.transform.GetComponent<Connector>();
            var grab = interactable as XRGrabInteractable;
            if (connectionPose == null || port == null || port.Socket == null || plug == null || grab == null)
                return base.GetAttachTransform(interactable);

            // XRI attaches the grab pivot; Connector attaches the physical tip.
            // Convert the tip pose to that pivot, including the opposite cable end.
            Transform tip = plug.ConnectionTransform;
            Transform grabPoint = grab.GetAttachTransform(this);
            Quaternion inverseRoot = Quaternion.Inverse(plug.transform.rotation);
            Quaternion tipRotation = inverseRoot * tip.rotation;
            Quaternion rootRotation = port.Socket.ConnectionRotation * Quaternion.Inverse(tipRotation);
            Vector3 rootToTip = inverseRoot * (tip.position - plug.transform.position);
            Vector3 rootToGrab = inverseRoot * (grabPoint.position - plug.transform.position);
            Vector3 rootPosition = port.Socket.ConnectionPosition - rootRotation * rootToTip;
            connectionPose.SetPositionAndRotation(rootPosition + rootRotation * rootToGrab,
                rootRotation * (inverseRoot * grabPoint.rotation));
            return connectionPose;
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
