using UnityEngine;
using Modules.Module01_CableMaking.Domain.Cable;

namespace Modules.Module01_CableMaking.Domain.Standards
{
    [System.Serializable]
    public class StandardSlot
    {
        [SerializeField]
        private int slotNumber;

        [SerializeField]
        private WireColor expectedColor;

        public int SlotNumber => slotNumber;

        public WireColor ExpectedColor => expectedColor;
    }
}