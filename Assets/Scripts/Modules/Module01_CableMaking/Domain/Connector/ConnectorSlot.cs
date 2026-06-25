// Representa un pin del RJ45.
//
//     Número Posición Wire conectado

using Modules.Module01_CableMaking.Domain.Cable;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Connector
{
    public class ConnectorSlot : MonoBehaviour
    {
        [SerializeField] private int slotNumber;

        [SerializeField] private Transform connectionPoint;

        public int SlotNumber => slotNumber;

        public Transform ConnectionPoint => connectionPoint;

        public Wire CurrentWire { get; private set; }

        public bool IsOccupied => CurrentWire != null;

        public bool TryInsert(Wire wire)
        {
            if (IsOccupied)
                return false;

            CurrentWire = wire;

            wire.Connect(this);

            return true;
        }

        public void RemoveWire()
        {
            CurrentWire = null;
        }
    }
    
}