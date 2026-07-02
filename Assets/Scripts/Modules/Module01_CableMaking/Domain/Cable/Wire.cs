
//     Representa un hilo.
//
//     Debe saber:
//
//     Color, Origen, Posición, Slot conectado


using Modules.Module01_CableMaking.Domain.Connector;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class Wire : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private WireColor wireColor;

        [Header("References")]
        [SerializeField] private Transform startPoint;
        

        public WireColor Color => wireColor;

        public Transform StartPoint => startPoint;

        public bool IsConnected => ConnectedSlot != null;

        public ConnectorSlot ConnectedSlot { get; private set; }

        public void Move(Vector3 position)
        {
            if (IsConnected)
                return;

            transform.position = position;
        }

        public void Connect(ConnectorSlot slot)
        {
            ConnectedSlot = slot;
            transform.position = slot.ConnectionPoint.position;
        }

        public void Disconnect()
        {
            ConnectedSlot = null;
            transform.position = startPoint.position;
        }
    }
}