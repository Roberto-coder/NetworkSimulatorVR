using UnityEngine;

namespace Modules.Module03_Diagnostics.Cable_physics.Scripts
{
    public enum CableCondition { Whole, Peeled, Crimped, Tested }

    public class CableEndState : MonoBehaviour
    {
        public CableCondition Condition { get; private set; } = CableCondition.Whole;
        public event System.Action<CableCondition> OnConditionChanged;

        public bool IsPeeled => Condition >= CableCondition.Peeled;
        public bool IsCrimped => Condition >= CableCondition.Crimped;
        public bool IsTested => Condition >= CableCondition.Tested;

        public bool CanAdvanceTo(CableCondition target) => target == Condition + 1;

        public bool TryAdvanceTo(CableCondition target)
        {
            if (!CanAdvanceTo(target)) return false;
            Condition = target;
            OnConditionChanged?.Invoke(Condition);
            return true;
        }
    }
}