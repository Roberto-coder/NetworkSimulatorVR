using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    public class WireSnapSolver : MonoBehaviour
    {
        [SerializeField]
        private Rj45Connector connector;

        [SerializeField]
        private float snapDistance = 0.5f;

        public bool TrySnap(Wire wire, WireView view)
        {
            if (wire == null)
            {
                Debug.LogError("Wire es NULL.");
                return false;
            }

            if (view == null)
            {
                Debug.LogError($"WireView es NULL para {wire.name}.");
                return false;
            }

            if (connector == null)
            {
                Debug.LogError("WireSnapSolver: Connector no asignado.");
                return false;
            }

            ConnectorSlot closestSlot = null;
            float closestDistance = float.MaxValue;

            foreach (ConnectorSlot slot in connector.Slots)
            {
                if (slot == null)
                    continue;

                if (slot.ConnectionPoint == null)
                {
                    Debug.LogError($"El Slot {slot.SlotNumber} no tiene ConnectionPoint.");
                    continue;
                }

                float distance = Vector3.Distance(
                    view.Tip.position,
                    slot.ConnectionPoint.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }

            if (closestSlot == null)
                return false;

            if (closestDistance > snapDistance)
                return false;

            if (!closestSlot.TryInsert(wire))
                return false;

            view.Snap(closestSlot.ConnectionPoint);

            return true;
        }
    }
}