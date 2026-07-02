using System.Collections.Generic;
using System.Linq;

namespace Modules.Module01_CableMaking.Domain.Validation
{
    public class ValidationResult
    {
        private readonly List<SlotValidation> slots = new();

        public IReadOnlyList<SlotValidation> Slots => slots;

        public bool IsValid => slots.All(slot => slot.IsValid);

        public void Add(SlotValidation validation)
        {
            slots.Add(validation);
        }
    }
}