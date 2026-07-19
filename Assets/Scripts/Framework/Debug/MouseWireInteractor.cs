using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using Modules.Module01_CableMaking.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Debug
{
    public class MouseWireInteractor : MonoBehaviour
    {
        [SerializeField] private Camera cam;

        [SerializeField] private Transform dragPlane;

        [SerializeField] private WireSnapSolver snapSolver;

        [SerializeField]
        private LayerMask wireLayer;

        [SerializeField]
        private LayerMask dragPlaneLayer;
        
        private Wire selectedWire;

        private WireView selectedView;

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                Select();

            if (selectedView != null)
                Drag();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                Release();
        }

        private void Select()
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(
                    ray, 
                    out RaycastHit hit,
                    100f,
                    wireLayer
                    ))
                return;

            selectedView = hit.collider.GetComponentInParent<WireView>();

            if (selectedView == null)
                return;

            selectedWire = selectedView.GetComponent<Wire>();
        }

        private void Drag()
        {
            Ray ray = cam.ScreenPointToRay(
                Mouse.current.position.ReadValue());

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f,
                    dragPlaneLayer))
                return;

            selectedView.Drag(hit.point);
        }

        private void Release()
        {
            if (selectedWire == null)
                return;
            
            bool snapped = snapSolver.TrySnap(selectedWire, selectedView);

            if (!snapped)
            {
                selectedWire.Disconnect();
                selectedView.ResetView();
            }

            selectedWire = null;
            selectedView = null;
        }
    }
}