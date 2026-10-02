using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    public sealed class NetworkActionRecord
    {
        public long Sequence { get; }
        public long Revision { get; }
        public DateTime TimestampUtc { get; }
        public string EntityId { get; }
        public string Action { get; }
        public string Before { get; }
        public string After { get; }
        public bool Accepted { get; }
        public string Reason { get; }
        public string Origin { get; }
        internal NetworkActionRecord(long sequence, long revision, string entity, string action,
            string before, string after, bool accepted, string reason)
        {
            Sequence = sequence; Revision = revision; TimestampUtc = DateTime.UtcNow;
            EntityId = entity; Action = action; Before = before; After = after;
            Accepted = accepted; Reason = reason;
            Origin = action == "Reset" ? "scenario" : "user";
        }
    }

    /// <summary>Único punto de escritura. Los snapshots son copias: la UI no puede saltarse el registro.</summary>
    public sealed class NetworkSession
    {
        private readonly NetworkDefinition initial;
        private NetworkDefinition current;
        private readonly List<NetworkActionRecord> history = new();
        public long Revision { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();
        public IReadOnlyList<NetworkActionRecord> History => history.AsReadOnly();
        public NetworkSession(NetworkDefinition definition)
        {
            var errors = NetworkDefinitionValidator.Validate(definition);
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors), nameof(definition));
            initial = definition.Copy(); current = initial.Copy();
        }
        public NetworkDefinition Snapshot() => current.Copy();

        // Consultar no cambia la red ni invalida resultados anteriores de la misma revisión.
        internal void RecordProbe(ProbeResult result) => Record(result.SourcePort, "Ping", "", result.DestinationIp,
            result.Status == ProbeStatus.Success, result.Status + ": " + result.Message);

        internal void RecordSwitchQuery(string id) => Record(id, "SwitchPorts", "", "", true, "Consulta de puertos mediante gestión LAN.");

        internal void RecordArpQuery(string sourcePort) =>
            Record(sourcePort, "Arp", "", "", true, "Consulta de la caché ARP local.");

        internal void RecordCableTest(string cableId, bool accepted, string message) =>
            Record(cableId, "CableTest", "", message, accepted, "Prueba de continuidad con ambos extremos desconectados.");

        // La revisión nunca retrocede: una evidencia previa tampoco vuelve a ser válida tras Reset.
        public void Reset()
        {
            current = initial.Copy(); Revision++;
            Record("scenario", "Reset", "", "initial", true, "Sesión restaurada; evidencia anterior obsoleta.");
        }

        public bool SetAddress(string portId, string ipv4, int prefixLength)
        {
            var port = current.ports.Find(p => p.id == portId);
            if (port == null || current.devices.Find(d => d.id == port.deviceId).kind == DeviceKind.Passive ||
                !NetworkDefinitionValidator.IsMac(port.mac) || !NetworkDefinitionValidator.IsIpv4(ipv4) || prefixLength < 1 || prefixLength > 30)
                return Record(portId, "Address", "", ipv4, false, "Interfaz o dirección inválida.");
            string before = $"{port.ipv4}/{port.prefixLength}";
            if (port.ipv4 != ipv4 || port.prefixLength != prefixLength)
            { port.ipv4 = ipv4; port.prefixLength = prefixLength; Revision++; }
            // Se permite crear un conflicto al configurar: la validación final debe detectarlo.
            return Record(portId, "Address", before, $"{ipv4}/{prefixLength}", true, "");
        }

        public bool SetPortEnabled(string portId, bool enabled)
        {
            var p = current.ports.Find(port => port.id == portId);
            if (p == null || current.devices.Find(d => d.id == p.deviceId).kind == DeviceKind.Passive)
                return Record(portId, "PortEnabled", "", enabled.ToString(), false, "Puerto administrable inexistente.");
            string before = p.enabled.ToString();
            if (p.enabled != enabled) { p.enabled = enabled; Revision++; }
            return Record(portId, "PortEnabled", before, enabled.ToString(), true, "");
        }

        public bool ConnectCable(string cableId, string portA, string portB)
        {
            var c = current.cables.Find(cable => cable.id == cableId);
            if (c == null || !c.interactable || !Available(portA, cableId) || !Available(portB, cableId) ||
                (!string.IsNullOrEmpty(portA) && portA == portB))
                return Record(cableId, "Connect", "", $"{portA}|{portB}", false, "Cable fijo, puerto inválido u ocupado.");
            portA ??= ""; portB ??= "";
            string before = $"{c.portA}|{c.portB}";
            if (c.portA != portA || c.portB != portB) { c.portA = portA; c.portB = portB; Revision++; }
            return Record(cableId, "Connect", before, $"{portA}|{portB}", true, "");
        }

        public bool LabelCable(string cableId, string labelA, string labelB)
        {
            var c = current.cables.Find(cable => cable.id == cableId);
            if (c == null || !c.interactable || labelA == null || labelB == null)
                return Record(cableId, "Label", "", "", false, "Cable o etiquetas inválidos.");
            string before = $"{c.labelA}|{c.labelB}";
            if (c.labelA != labelA || c.labelB != labelB) { c.labelA = labelA; c.labelB = labelB; Revision++; }
            return Record(cableId, "Label", before, $"{labelA}|{labelB}", true, "");
        }

        /// <summary>Vecindad física directa, incluso si el cable está roto; no equivale a alcance IP.</summary>
        public IReadOnlyList<string> GetPhysicalNeighbours(string portId)
        {
            if (!current.ports.Any(p => p.id == portId)) return Array.Empty<string>();
            var result = new HashSet<string>();
            foreach (var cable in current.cables) AddPeer(cable.portA, cable.portB);
            foreach (var link in current.passiveConnections) AddPeer(link.portA, link.portB);
            return result.OrderBy(id => id, StringComparer.Ordinal).ToArray();
            void AddPeer(string a, string b)
            {
                if (a == portId && !string.IsNullOrEmpty(b)) result.Add(b);
                if (b == portId && !string.IsNullOrEmpty(a)) result.Add(a);
            }
        }

        private bool Available(string portId, string cableId) => string.IsNullOrEmpty(portId) ||
            (current.ports.Any(p => p.id == portId) && !current.cables.Any(c => c.id != cableId && (c.portA == portId || c.portB == portId)));

        private bool Record(string entity, string action, string before, string after, bool accepted, string reason)
        {
            history.Add(new NetworkActionRecord(history.Count + 1, Revision, entity, action, before, after, accepted, reason));
            return accepted;
        }
    }
}
