
//     Representa un hilo.
//
//     Debe saber:
//
//     Color, Origen, Posición, Slot conectado


using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Wire
{
    public class Wire : MonoBehaviour
    {
        [SerializeField] private WireColor wireColor;

        public WireColor Color => wireColor;

        public bool IsConnected => ConnectedSlot != null;

        public ConnectorSlot ConnectedSlot { get; private set; }

        public void Connect(ConnectorSlot slot)
        {
            ConnectedSlot = slot;
        }

        public void Disconnect()
        {
            if (ConnectedSlot != null)
                ConnectedSlot.RemoveWire(this);

            ConnectedSlot = null;
        }
    }
}
