using System.Collections.Generic;
using UnityEngine;
using Modules.Module01_CableMaking.Domain;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;

namespace Modules.Module01_CableMaking.Interaction
{
    public class WireSnapSolver : MonoBehaviour
    {
        [SerializeField]
        private Rj45Connector connector;

        [SerializeField]
        private float snapDistance = 0.05f;

        public bool TrySnap(Wire wire)
        {
            ConnectorSlot closestSlot = null;
            float closestDistance = float.MaxValue;

            foreach (ConnectorSlot slot in connector.Slots)
            {
                float distance = Vector3.Distance(
                    wire.transform.position,
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

            closestSlot.TryInsert(wire);

            return true;
        }
    }
}