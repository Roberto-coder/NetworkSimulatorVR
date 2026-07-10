using System.Collections.Generic;
using System.Linq;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Standards;
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
        
        [SerializeField]
        private CableStandard currentStandard;

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
        public ValidationResult Validate(CableStandard standard)
        {
            ValidationResult result = new();

            for (int i = 0; i < slots.Count; i++)
            {
                ConnectorSlot slot = slots[i];

                WireColor expected = standard.GetExpectedColor(i);

                if (!slot.IsOccupied)
                {
                    result.Add(new SlotValidation(
                        slot.SlotNumber,
                        false,
                        false,
                        expected,
                        null));

                    continue;
                }

                WireColor current = slot.CurrentColor;

                bool correct = current == expected;

                result.Add(new SlotValidation(
                    slot.SlotNumber,
                    correct,
                    true,
                    expected,
                    current));
            }

            return result;
        }
    }
}