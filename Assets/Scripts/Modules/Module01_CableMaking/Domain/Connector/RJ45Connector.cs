using System.Collections.Generic;
using System.Linq;
using GameData.Standards;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Validation;
using UnityEngine;

// Tengo 8 slots
// Buscar slot
// Conectar hilo
// Saber si estoy completo

namespace Modules.Module01_CableMaking.Domain.Connector
{
    public class Rj45Connector : MonoBehaviour
    {
        [SerializeField]
        private List<ConnectorSlot> slots = new();

        public IReadOnlyList<ConnectorSlot> Slots => slots;
        
        public bool IsComplete =>
            slots.All(slot => slot.IsOccupied);
        
        // public bool Validate(CableStandard standard)
        // {
        //     if (standard == null)
        //         return false;
        //
        //     if (slots.Count != standard.SlotCount)
        //         return false;
        //
        //     for (int i = 0; i < slots.Count; i++)
        //     {
        //         ConnectorSlot slot = slots[i];
        //
        //         if (!slot.IsOccupied)
        //             return false;
        //
        //         if (slot.CurrentColor != standard.GetExpectedColor(i))
        //             return false;
        //     }
        //
        //     return true;
        // }
    }
}