
using System.Collections.Generic;
using Modules.Module01_CableMaking.Domain;
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
        [SerializeField] private List<ConnectorSlot> pins = new();
        [SerializeField] private float snapDistance = 0.5f;
        [SerializeField] private WireSnapSolver snapSolver;

        private Wire selectedWire;

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                Select();

            if (selectedWire != null)
                Drag();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                Release();
        }

        void Select()
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Wire wire = hit.collider.GetComponent<Wire>();

            if (wire == null)
                return;

            selectedWire = wire;
        }

        void Drag()
        {
            if (selectedWire == null)
                return;

            
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);

                selectedWire.Move(point);
            }
        }

        void Release()
        {
            if (selectedWire == null)
                return;

            bool snapped = snapSolver.TrySnap(selectedWire);

            if (!snapped)
            {
                selectedWire.Disconnect();
            }

            selectedWire = null;
        }
    }
}