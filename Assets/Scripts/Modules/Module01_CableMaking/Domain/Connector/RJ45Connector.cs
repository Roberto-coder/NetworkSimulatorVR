using System.Collections.Generic;
using UnityEngine;

// Tengo 8 slots
// Buscar slot
// Conectar hilo
// Saber si estoy completo

namespace Modules.Module01_CableMaking.Domain.Connector
{
    public class RJ45Connector : MonoBehaviour
    {
        [SerializeField]
        private List<ConnectorSlot> slots = new();

        public IReadOnlyList<ConnectorSlot> Slots => slots;
        
        // public bool Validate(CableStandard standard)
        // {
        //     for (int i = 0; i < slots.Count; i++)
        //     {
        //         ConnectorSlot slot = slots[i];
        //
        //         if (!slot.IsOccupied)
        //             return false;
        //
        //         if (slot.CurrentWire.Color != standard.GetExpectedColor(i))
        //             return false;
        //     }
        //
        //     return true;
        // }
    }
}