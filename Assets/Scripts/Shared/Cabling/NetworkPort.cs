using System;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using UnityEngine;

namespace Shared.Cabling
{
    /// <summary>Familias de conectores que no deben mezclarse físicamente.</summary>
    public enum NetworkPortKind
    {
        EthernetRj45,
        ConsoleRj45,
        Fiber
    }

    /// <summary>
    /// Da identidad lógica a un socket físico: por ejemplo SW1/Gi01 o PP1/01.
    /// Connector sigue resolviendo el encaje; esta clase reporta el enlace a la simulación.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkPort : MonoBehaviour
    {
        [SerializeField] private string deviceId = "device";
        [SerializeField] private string portId = "port-01";
        [SerializeField] private NetworkPortKind kind = NetworkPortKind.EthernetRj45;
        [SerializeField] private Connector socket;
        [SerializeField] private Renderer linkLed;
        [SerializeField] private Color disconnectedColor = new(0.08f, 0.08f, 0.08f);
        [SerializeField] private Color connectedColor = Color.green;

        // MaterialPropertyBlock evita duplicar el material del LED para cada puerto.
        private MaterialPropertyBlock propertyBlock;

        public string DeviceId => deviceId;
        public string PortId => portId;
        public string Address => $"{deviceId}/{portId}";
        public NetworkPortKind Kind => kind;
        public Connector Socket => socket;
        public bool IsConnected => socket != null && socket.IsConnected;

        public event Action<NetworkPort, bool> LinkStateChanged;

        private bool previousState;

        private void Awake()
        {
            if (socket == null)
                socket = GetComponent<Connector>();
            propertyBlock = new MaterialPropertyBlock();
            previousState = IsConnected;
            RefreshLed(previousState);
        }

        private void Update()
        {
            // Connector no expone eventos, por lo que sólo notificamos cuando cambia
            // el estado observado; no se genera trabajo adicional mientras permanece igual.
            bool current = IsConnected;
            if (current == previousState)
                return;

            previousState = current;
            RefreshLed(current);
            LinkStateChanged?.Invoke(this, current);
        }

        public void Configure(string newDeviceId, string newPortId, NetworkPortKind newKind,
            Connector newSocket = null, Renderer newLinkLed = null)
        {
            deviceId = string.IsNullOrWhiteSpace(newDeviceId) ? "device" : newDeviceId.Trim();
            portId = string.IsNullOrWhiteSpace(newPortId) ? "port-01" : newPortId.Trim();
            kind = newKind;
            if (newSocket != null)
                socket = newSocket;
            if (newLinkLed != null)
                linkLed = newLinkLed;
        }

        private void RefreshLed(bool linked)
        {
            if (linkLed == null)
                return;
            propertyBlock ??= new MaterialPropertyBlock();
            linkLed.GetPropertyBlock(propertyBlock);
            Color color = linked ? connectedColor : disconnectedColor;
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            linkLed.SetPropertyBlock(propertyBlock);
        }
    }
}
