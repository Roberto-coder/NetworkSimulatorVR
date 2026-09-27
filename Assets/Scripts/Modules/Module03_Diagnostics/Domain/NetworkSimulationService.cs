using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    /// <summary>Simulación LAN determinista sin sockets del SO, física, Internet ni dependencias Unity.</summary>
    public sealed class NetworkSimulationService
    {
        private readonly NetworkSession session;
        private readonly Dictionary<string, Dictionary<string, NeighbourObservation>> neighbours = new();
        private long cacheRevision = -1;
        private sealed class Edge
        {
            public string From, To, Id;
            public TraceLinkKind Kind;
            public ProbeTraceStep Step(TracePhase phase, bool reverse = false, int attempt = 0) =>
                new ProbeTraceStep(phase, Kind, Id, reverse ? To : From, reverse ? From : To, attempt);
        }

        public NetworkSimulationService(NetworkSession session) => this.session = session ?? throw new ArgumentNullException(nameof(session));
        public bool IsCurrent(ProbeResult result) => result != null && result.SessionId == session.Id && result.Revision == session.Revision;
        public IReadOnlyList<NeighbourObservation> GetNeighbours(string sourcePort)
        {
            RefreshCache();
            return sourcePort != null && neighbours.TryGetValue(sourcePort, out var table)
                ? Array.AsReadOnly(table.Values.OrderBy(n => n.Ip, StringComparer.Ordinal).ToArray())
                : Array.Empty<NeighbourObservation>();
        }

        public ProbeResult Ping(string sourcePort, string destinationIp, int count = 4)
        {
            if (count < 1 || count > 10) throw new ArgumentOutOfRangeException(nameof(count), "Entre 1 y 10 intentos.");
            RefreshCache();
            var network = session.Snapshot();
            var trace = new List<ProbeTraceStep>();
            var ports = network.ports.ToDictionary(p => p.id);
            var source = network.ports.Find(p => p.id == sourcePort);
            ProbeResult Finish(ProbeStatus status, string message, string responder = null)
            {
                var result = new ProbeResult(session.Id, session.Revision, sourcePort, destinationIp, responder, status, message, count, trace);
                session.RecordProbe(result);
                return result;
            }
            if (source == null || !UsableAddress(source.ipv4, source.prefixLength) || !NetworkDefinitionValidator.IsMac(source.mac))
                return Finish(ProbeStatus.InvalidSource, "Selecciona una interfaz local con IPv4 y MAC válidas.");
            if (!source.enabled) return Finish(ProbeStatus.SourceDisabled, "La interfaz de origen está deshabilitada.");
            if (!NetworkDefinitionValidator.IsIpv4(destinationIp) || !Unicast(destinationIp))
                return Finish(ProbeStatus.InvalidDestination, "El destino debe ser una IPv4 unicast.");
            if (!SameSubnet(source.ipv4, destinationIp, source.prefixLength))
                return Finish(ProbeStatus.NoRoute, "Destino fuera de la subred local; este escenario no tiene gateway.");
            if (!UsableAddress(destinationIp, source.prefixLength))
                return Finish(ProbeStatus.InvalidDestination, "El destino es una dirección de red o broadcast.");

            // Grafo operacional: cables rotos y puertos inhabilitados no conducen tráfico.
            var graph = ports.Keys.ToDictionary(id => id, _ => new List<Edge>());
            void Link(string a, string b, string id, TraceLinkKind kind)
            {
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return;
                if (!ports[a].enabled || !ports[b].enabled || ports[a].segmentId != ports[b].segmentId) return;
                graph[a].Add(new Edge { From = a, To = b, Id = id, Kind = kind });
                graph[b].Add(new Edge { From = b, To = a, Id = id, Kind = kind });
            }
            foreach (var cable in network.cables.Where(c => c.intact)) Link(cable.portA, cable.portB, cable.id, TraceLinkKind.Cable);
            foreach (var link in network.passiveConnections) Link(link.portA, link.portB, link.portA + "↔" + link.portB, TraceLinkKind.PassiveContinuity);
            foreach (var device in network.devices.Where(d => d.kind == DeviceKind.Switch))
            {
                var switchPorts = network.ports.Where(p => p.deviceId == device.id).OrderBy(p => p.id, StringComparer.Ordinal).ToArray();
                for (int i = 0; i < switchPorts.Length; i++)
                    for (int j = i + 1; j < switchPorts.Length; j++) Link(switchPorts[i].id, switchPorts[j].id, device.id, TraceLinkKind.SwitchForwarding);
            }
            // BFS estable: termina incluso ante ciclos. La inundación ARP se representa por un árbol,
            // no reproduce tormentas, aprendizaje MAC, STP ni cada copia de una trama Ethernet.
            var parents = new Dictionary<string, Edge>();
            var visited = new HashSet<string> { source.id };
            var queue = new Queue<string>(); queue.Enqueue(source.id);
            var flood = new List<Edge>();
            while (queue.Count > 0)
                foreach (var edge in graph[queue.Dequeue()].OrderBy(e => e.To, StringComparer.Ordinal).ThenBy(e => e.Id, StringComparer.Ordinal))
                    if (visited.Add(edge.To)) { parents[edge.To] = edge; flood.Add(edge); queue.Enqueue(edge.To); }
            List<Edge> Path(string to)
            {
                var path = new List<Edge>();
                while (to != source.id) { var edge = parents[to]; path.Add(edge); to = edge.From; }
                path.Reverse(); return path;
            }
            var reachable = network.ports.Where(p => visited.Contains(p.id) && UsableAddress(p.ipv4, p.prefixLength) && NetworkDefinitionValidator.IsMac(p.mac)).ToArray();
            if (reachable.Count(p => p.ipv4 == source.ipv4) > 1)
            {
                trace.AddRange(flood.Select(e => e.Step(TracePhase.ArpRequest)));
                return Finish(ProbeStatus.AddressConflict, "Conflicto observable en la dirección del origen (modelo didáctico).");
            }
            if (destinationIp == source.ipv4) return Finish(ProbeStatus.Success, "Respuesta local; no verifica el cable de red.", source.id);
            if (!neighbours.TryGetValue(source.id, out var cache)) neighbours[source.id] = cache = new Dictionary<string, NeighbourObservation>();
            bool cached = cache.ContainsKey(destinationIp);
            if (!cached) trace.AddRange(flood.Select(e => e.Step(TracePhase.ArpRequest)));
            var candidates = reachable.Where(p => p.ipv4 == destinationIp).ToArray();
            if (candidates.Length == 0)
                return Finish(ProbeStatus.AddressUnresolved, "No se obtuvo respuesta ARP. Consultar enlace, puertos y configuración para identificar la causa.");
            // Mostrar las respuestas contradictorias, pero nunca dibujar ICMP exitoso a un equipo ambiguo.
            if (!cached)
                foreach (var candidate in candidates.OrderBy(p => p.id, StringComparer.Ordinal))
                    trace.AddRange(Path(candidate.id).AsEnumerable().Reverse().Select(e => e.Step(TracePhase.ArpReply, true)));
            if (candidates.Length > 1)
                return Finish(ProbeStatus.AddressConflict, "Varios equipos alcanzables reclaman esa IP (modelo didáctico).");
            var destination = candidates[0];
            cache[destinationIp] = new NeighbourObservation(destination, session.Revision);
            var request = Path(destination.id);
            bool canReply = SameSubnet(destination.ipv4, source.ipv4, destination.prefixLength) && UsableAddress(source.ipv4, destination.prefixLength);
            for (int i = 0; i < count; i++)
            {
                trace.AddRange(request.Select(e => e.Step(TracePhase.EchoRequest, false, i + 1)));
                if (canReply) trace.AddRange(request.AsEnumerable().Reverse().Select(e => e.Step(TracePhase.EchoReply, true, i + 1)));
            }
            return Finish(canReply ? ProbeStatus.Success : ProbeStatus.ReplyUnavailable,
                canReply ? "Solicitudes y respuestas completadas." : "Sin respuesta ICMP; revisar la configuración de retorno.", destination.id);
        }

        private void RefreshCache()
        {
            if (cacheRevision == session.Revision) return;
            neighbours.Clear(); cacheRevision = session.Revision;
        }
        private static uint Address(string ip) => ip.Split('.').Aggregate(0u, (value, part) => (value << 8) | byte.Parse(part));
        private static uint Mask(int prefix) => uint.MaxValue << (32 - prefix);
        private static bool Unicast(string ip)
        {
            uint address = Address(ip); uint first = address >> 24;
            return first > 0 && first < 224 && first != 127;
        }
        private static bool UsableAddress(string ip, int prefix)
        {
            if (!NetworkDefinitionValidator.IsIpv4(ip) || prefix < 1 || prefix > 30 || !Unicast(ip)) return false;
            uint host = Address(ip) & ~Mask(prefix);
            return host != 0 && host != ~Mask(prefix);
        }
        private static bool SameSubnet(string a, string b, int prefix) => (Address(a) & Mask(prefix)) == (Address(b) & Mask(prefix));
    }
}
