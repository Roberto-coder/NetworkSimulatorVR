using System;
using HPhysic;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using UnityEngine;

namespace Shared.Cabling
{
    /// <summary>
    /// Traduce las uniones físicas de PhysicCable a un enlace lógico entre dos NetworkPort.
    /// No mueve el cable ni crea joints: sólo observa y publica cambios de topología.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicCable))]
    public sealed class PatchCableLink : MonoBehaviour
    {
        [SerializeField] private PhysicCable physicalCable;
        [SerializeField] private NetworkPortKind kind = NetworkPortKind.EthernetRj45;

        public NetworkPort StartPort { get; private set; }
        public NetworkPort EndPort { get; private set; }
        public NetworkPortKind Kind => kind;
        public bool HasCompleteLink => StartPort != null && EndPort != null && StartPort != EndPort;

        public event Action<PatchCableLink> LinkChanged;

        private void Awake()
        {
            if (physicalCable == null)
                physicalCable = GetComponent<PhysicCable>();
        }

        private void Update()
        {
            // Los conectores del cable apuntan al Connector hembra. Desde ese componente
            // se asciende hasta el NetworkPort que contiene la identidad del socket.
            NetworkPort newStart = ResolvePort(physicalCable != null ? physicalCable.StartConnector : null);
            NetworkPort newEnd = ResolvePort(physicalCable != null ? physicalCable.EndConnector : null);
            if (newStart == StartPort && newEnd == EndPort)
                return;

            StartPort = newStart;
            EndPort = newEnd;
            LinkChanged?.Invoke(this);
        }

        public bool Connects(string firstAddress, string secondAddress)
        {
            // Un cable Ethernet no tiene dirección fija, por eso se aceptan ambos órdenes.
            if (!HasCompleteLink)
                return false;
            return Matches(StartPort, firstAddress) && Matches(EndPort, secondAddress) ||
                   Matches(StartPort, secondAddress) && Matches(EndPort, firstAddress);
        }

        private static bool Matches(NetworkPort port, string address) =>
            port != null && string.Equals(port.Address, address, StringComparison.OrdinalIgnoreCase);

        private static NetworkPort ResolvePort(Connector cableEnd)
        {
            Connector target = cableEnd != null ? cableEnd.ConnectedTo : null;
            return target != null ? target.GetComponentInParent<NetworkPort>() : null;
        }
    }
}
