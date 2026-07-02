using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class Cable: MonoBehaviour
    {
        [SerializeField]
        private List<Wire> wires = new();

        public IReadOnlyList<Wire> Wires => wires;

        public int WireCount => wires.Count;

        public bool IsComplete =>
            wires.All(w => w.IsConnected);

        public Wire GetWire(WireColor color)
        {
            return wires.FirstOrDefault(w => w.Color == color);
        }

        public void ResetCable()
        {
            foreach (Wire wire in wires)
            {
                wire.Disconnect();
            }
        }
    }
}