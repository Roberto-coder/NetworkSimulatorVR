using Modules.Module01_CableMaking.Domain.Cable;

namespace Modules.Module01_CableMaking.Domain.Validation
{
    public class SlotValidation
    {
        public int SlotNumber { get; }

        public bool IsValid { get; }

        public bool IsOccupied { get; }

        public WireColor? ExpectedColor { get; }

        public WireColor? CurrentColor { get; }

        public SlotValidation(
            int slotNumber,
            bool isValid,
            bool isOccupied,
            WireColor? expectedColor,
            WireColor? currentColor)
        {
            SlotNumber = slotNumber;
            IsValid = isValid;
            IsOccupied = isOccupied;
            ExpectedColor = expectedColor;
            CurrentColor = currentColor;
        }
    }
}