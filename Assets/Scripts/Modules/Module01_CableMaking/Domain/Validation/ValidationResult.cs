using System.Collections.Generic;
using System.Linq;

namespace Modules.Module01_CableMaking.Domain.Validation
{
    public class ValidationResult
    {
        private readonly List<SlotValidation> slots = new();
        private readonly List<string> errors = new();

        public IReadOnlyList<SlotValidation> Slots => slots;
        public IReadOnlyList<string> Errors => errors;

        public bool IsValid =>
            errors.Count == 0 &&
            slots.Count == 8 &&
            slots.All(slot => slot.IsValid);

        public void Add(SlotValidation validation)
        {
            slots.Add(validation);
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                errors.Add(error);
        }
    }
}
