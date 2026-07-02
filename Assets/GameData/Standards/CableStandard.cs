using Modules.Module01_CableMaking.Domain.Cable;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Standards
{
    [CreateAssetMenu(fileName = "CableStandard", menuName = "Scriptable Objects/CableStandard")]


    public class CableStandard : ScriptableObject
    {
        [SerializeField]
        private StandardSlot[] slots  = new StandardSlot[8];

        public int SlotCount => slots .Length;


        public StandardSlot GetSlot(int index)
        {
            return slots[index];
        }
        
        public WireColor GetExpectedColor(int index)
        {
            return slots[index].ExpectedColor;
        }

    }
}
