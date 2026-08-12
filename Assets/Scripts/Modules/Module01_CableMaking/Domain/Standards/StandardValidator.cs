using GameData.Standards;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using Modules.Module01_CableMaking.Domain.Validation;
using Modules.Module01_CableMaking.Domain.Wire;
using System.Collections.Generic;

namespace Modules.Module01_CableMaking.Domain.Standards
{
    public class StandardValidator
    {
        public ValidationResult Validate(
            Rj45Connector connector,
            CableStandard standard)
        {
            ValidationResult result = new();

            if (connector == null)
            {
                result.AddError("No hay conector RJ45 asignado.");
                return result;
            }

            if (standard == null)
            {
                result.AddError("No hay estandar de cable asignado.");
                return result;
            }

            const int Rj45SlotCount = 8;
            if (standard.SlotCount != Rj45SlotCount)
            {
                result.AddError($"El estandar debe contener {Rj45SlotCount} posiciones.");
                return result;
            }

            if (connector.Slots == null || connector.Slots.Count != Rj45SlotCount)
            {
                result.AddError($"El conector debe contener {Rj45SlotCount} slots.");
                return result;
            }

            HashSet<int> slotNumbers = new();

            foreach (ConnectorSlot slot in connector.Slots)
            {
                if (slot == null)
                {
                    result.AddError("El conector contiene un slot sin configurar.");
                    continue;
                }

                if (slot.SlotNumber < 1 || slot.SlotNumber > Rj45SlotCount ||
                    !slotNumbers.Add(slot.SlotNumber))
                {
                    result.AddError($"Numero de slot invalido o repetido: {slot.SlotNumber}.");
                    continue;
                }

                WireColor expected = standard.GetExpectedColor(slot.SlotNumber - 1);

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

                result.Add(new SlotValidation(
                    slot.SlotNumber,
                    current == expected,
                    true,
                    expected,
                    current));
            }

            return result;
        }
    }
}
