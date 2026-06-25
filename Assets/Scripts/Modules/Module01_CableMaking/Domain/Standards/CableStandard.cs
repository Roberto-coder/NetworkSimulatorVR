using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Standards
{
    [CreateAssetMenu(fileName = "CableStandard", menuName = "Scriptable Objects/CableStandard")]


    public class CableStandard : ScriptableObject
    {
        [SerializeField]
        private StandardSlot[] slots  = new StandardSlot[8];



        public StandardSlot GetExpectedColor(int slot)
        {
            return slots [slot];
        }

        public int SlotCount => slots .Length;
    }
}
