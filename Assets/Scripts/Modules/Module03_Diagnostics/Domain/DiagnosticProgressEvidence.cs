using System.Collections.Generic;
using System.Linq;
namespace Modules.Module03_Diagnostics.Domain
{
    /// <summary>Solo almacena pruebas observadas: nunca ejecuta pings para aprobar al alumno.</summary>
    public sealed class DiagnosticProgressEvidence
    {
        private readonly NetworkSession session;
        private readonly NetworkSimulationService simulation;
        private readonly NetworkDefinition inventory;
        private readonly string source;
        private readonly Dictionary<string, ProbeResult> probes = new();
        public DiagnosticProgressEvidence(NetworkSession session, NetworkSimulationService simulation, NetworkDefinition inventory, string source)
        { this.session = session; this.simulation = simulation; this.inventory = inventory.Copy(); this.source = source; }
        public void Observe(ProbeResult result)
        {
            if (result == null || result.SourcePort != source || !simulation.IsCurrent(result)) return;
            // Un intento fallido nuevo sustituye también la evidencia anterior para esa dirección.
            probes[result.DestinationIp] = result;
        }
        public bool Verified(IEnumerable<string> devices)
        {
            var ids = devices?.ToArray();
            return ids != null && ids.Length > 0 && ids.All(id =>
            {
                var port = inventory.ports.FirstOrDefault(p => p.deviceId == id && !string.IsNullOrEmpty(p.ipv4));
                return port != null && probes.TryGetValue(port.ipv4, out var p) && simulation.IsCurrent(p) &&
                    p.Status == ProbeStatus.Success && p.ResponderPort == port.id;
            });
        }
        public bool ConsultedSwitch(string id)
        {
            long reset = session.History.LastOrDefault(h => h.Action == "Reset")?.Sequence ?? 0;
            return session.History.Any(h => h.Sequence > reset && h.Action == "SwitchPorts" && h.EntityId == id && h.Accepted);
        }
        public void Clear() => probes.Clear();
    }
}
