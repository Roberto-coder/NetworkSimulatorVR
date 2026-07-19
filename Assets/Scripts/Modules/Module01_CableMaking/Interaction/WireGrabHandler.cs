using Modules.Module01_CableMaking.Domain.Cable;
using Oculus.Interaction;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    public class WireGrabHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Wire wire;
        [SerializeField] private WireView wireView;
        [SerializeField] private WireSnapSolver snapSolver;
        [SerializeField] private Grabbable grabbable;

        [Header("Drag")]
        [SerializeField] private Transform controller;
        [SerializeField] private LayerMask dragPlaneLayer;

        private bool previousState;

        private bool IsGrabbed => grabbable != null && grabbable.SelectingPointsCount > 0;

        private void Awake()
        {
            if (grabbable == null)
                grabbable = GetComponent<Grabbable>();

            previousState = false;
        }

        private void Update()
        {
            if (!previousState && IsGrabbed)
                BeginGrab();

            if (previousState && !IsGrabbed)
                EndGrab();

            previousState = IsGrabbed;

            if (IsGrabbed)
                Drag();
        }

        private void BeginGrab()
        {
            // Reservado para futuras acciones:
            // sonido
            // vibración
            // ocultar ayuda
            // etc.
        }

        private void EndGrab()
        {
            bool snapped = snapSolver.TrySnap(wire, wireView);

            if (!snapped)
            {
                wire.Disconnect();
                wireView.ResetView();
            }
        }

        private void Drag()
        {
            Ray ray = new Ray(controller.position, controller.forward);

            if (!Physics.Raycast(ray,
                    out RaycastHit hit,
                    5f,
                    dragPlaneLayer))
            {
                return;
            }

            wireView.Drag(hit.point);
        }
    }
}