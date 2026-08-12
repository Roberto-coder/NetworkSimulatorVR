// Representa un pin del RJ45.
//
//     Número Posición Wire conectado

using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Wire;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Connector
{
    public class ConnectorSlot : MonoBehaviour
    {
        [SerializeField] private int slotNumber;

        [SerializeField] private Transform connectionPoint;

        public int SlotNumber => slotNumber;

        public Transform ConnectionPoint => connectionPoint;

        public Wire.Wire CurrentWire { get; private set; }

        public bool IsOccupied => CurrentWire != null;

        public WireColor CurrentColor => CurrentWire.Color;
        
        public bool TryInsert(Wire.Wire wire)
        {
            if (wire == null)
                return false;

            if (CurrentWire == wire)
                return true;

            if (IsOccupied)
                return false;

            // Un hilo solo puede ocupar un slot a la vez. Esto libera su
            // posicion anterior cuando el alumno corrige el orden.
            wire.Disconnect();

            CurrentWire = wire;

            wire.Connect(this);

            return true;
        }

        public void RemoveWire(Wire.Wire wire)
        {
            if (CurrentWire == wire)
                CurrentWire = null;
        }
    }
    
}
