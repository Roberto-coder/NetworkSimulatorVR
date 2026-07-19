using GameData.Standards;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using Modules.Module01_CableMaking.Domain.Validation;

namespace Modules.Module01_CableMaking.Domain.Standards
{
    public class StandardValidator
    {
        public ValidationResult Validate(
            Rj45Connector connector,
            CableStandard standard)
        {
            ValidationResult result = new();

            foreach (ConnectorSlot slot in connector.Slots)
            {
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