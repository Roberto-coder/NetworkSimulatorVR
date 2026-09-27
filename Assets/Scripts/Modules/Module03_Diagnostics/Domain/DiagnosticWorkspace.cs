using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    /// <summary>Contexto local y destino remoto son selecciones distintas: el mapa no abre PCs a distancia.</summary>
    public sealed class DiagnosticWorkspace
    {
        // Solo equipos de usuario tienen pantalla local; infraestructura permanece en el mapa.
        public static bool HasLocalScreen(NetworkDefinition network, string deviceId) => network.devices.Any(d =>
            d.id == deviceId && (d.kind == DeviceKind.Computer || d.kind == DeviceKind.DiagnosticStation));
        private readonly NetworkSession session;
        private readonly NetworkSimulationService diagnostics;
        private readonly NetworkDefinition inventory;
        private readonly Dictionary<string, ProbeResult> observations = new();
        public string LocalDeviceId { get; private set; }
        public string SelectedDeviceId { get; private set; }
        public string SourcePortId { get; }
        public ProbeResult LastProbe { get; private set; }
        public DiagnosticWorkspace(NetworkSession session, NetworkSimulationService diagnostics, NetworkDefinition inventory, string sourcePort)
        {
            this.session = session; this.diagnostics = diagnostics; this.inventory = inventory.Copy(); SourcePortId = sourcePort;
            if (!inventory.ports.Any(p => p.id == sourcePort)) throw new ArgumentException("No existe interfaz de la laptop.");
        }
        public void OpenLocal(string id)
        {
            if (!inventory.devices.Any(d => d.id == id)) throw new ArgumentException("Dispositivo desconocido.");
            LocalDeviceId = id;
        }
        public void SelectDestination(string id)
        {
            if (!inventory.devices.Any(d => d.id == id)) throw new ArgumentException("Destino desconocido.");
            SelectedDeviceId = id;
        }
        public string ExpectedAddress(string id) => inventory.ports.FirstOrDefault(p => p.deviceId == id && !string.IsNullOrEmpty(p.ipv4))?.ipv4;
        public ProbeResult PingSelected()
        {
            string ip = ExpectedAddress(SelectedDeviceId);
            if (string.IsNullOrEmpty(ip)) throw new InvalidOperationException("Selecciona un equipo con IPv4 en el inventario.");
            LastProbe = diagnostics.Ping(SourcePortId, ip);
            observations[SelectedDeviceId] = LastProbe;
            return LastProbe;
        }
        public string Observation(string id)
        {
            if (!observations.TryGetValue(id, out var result)) return "Sin comprobar";
            if (!diagnostics.IsCurrent(result)) return "Obsoleto";
            if (result.Status != ProbeStatus.Success) return result.Status.ToString();
            return inventory.ports.Any(p => p.id == result.ResponderPort && p.deviceId == id) ? "Responde" : "Responde otro equipo";
        }
        public bool LastIsCurrent => diagnostics.IsCurrent(LastProbe);
        public string LocalConfiguration()
        {
            var n = session.Snapshot();
            var device = n.devices.FirstOrDefault(d => d.id == LocalDeviceId);
            if (device == null) return "Abre un dispositivo cercano con A.";
            if (device.kind == DeviceKind.Passive) return device.id + " · Elemento pasivo, sin IP.\n" +
                string.Join("\n", n.ports.Where(p => p.deviceId == device.id).Select(p => p.id));
            if (device.kind == DeviceKind.Switch) return device.id + " · Consulta sus puertos mediante administración desde la laptop.";
            return string.Join("\n", n.ports.Where(p => p.deviceId == LocalDeviceId).Select(p =>
                $"{p.id}\nIP: {(string.IsNullOrEmpty(p.ipv4) ? "No aplica" : p.ipv4 + "/" + p.prefixLength)}  MAC: {p.mac}\nAdministrativo: {(p.enabled ? "habilitado" : "inhabilitado")} · Enlace: {(HasLink(n, p.id) ? "activo" : "caído")}"));
        }
        public string Neighbours() => string.Join("\n", diagnostics.GetNeighbours(SourcePortId).Select(n => $"{n.Ip} → {n.Mac}"));
        public string History() => string.Join("\n", session.History.Skip(Math.Max(0, session.History.Count - 30)).Select(h =>
            $"#{h.Sequence} {h.Action} {h.EntityId}: {h.After} {h.Reason}"));
        public string SwitchPorts(string switchId)
        {
            var n = session.Snapshot();
            if (!n.devices.Any(d => d.id == switchId && d.kind == DeviceKind.Switch)) return "Selecciona un switch.";
            string ip = ExpectedAddress(switchId);
            if (ip == null) return "Sin interfaz de gestión documentada.";
            var access = diagnostics.Ping(SourcePortId, ip, 1);
            if (access.Status != ProbeStatus.Success || !n.ports.Any(p => p.id == access.ResponderPort && p.deviceId == switchId))
                return "Administración inaccesible desde la laptop.";
            return string.Join("\n", n.ports.Where(p => p.deviceId == switchId).Select(p =>
            {
                var cable = n.cables.FirstOrDefault(c => c.portA == p.id || c.portB == p.id);
                // Conexión física no implica integridad ni conectividad IP; no revela daños remotos.
                bool connected = cable != null;
                return $"{p.id}: admin={(p.enabled ? "ON" : "OFF")}; cable local={(connected ? "conectado" : "sin cable")}; enlace={(HasLink(n, p.id) ? "activo" : "caído / no aplica a interfaz lógica")}";
            }));
        }
        // El enlace físico termina en la siguiente interfaz activa. No atraviesa internamente switches.
        private static bool HasLink(NetworkDefinition n, string source)
        {
            var ports = n.ports.ToDictionary(p => p.id); var kinds = n.devices.ToDictionary(d => d.id, d => d.kind);
            var visited = new HashSet<string>(); var queue = new Queue<string>(); queue.Enqueue(source);
            while (queue.Count > 0)
            {
                string id = queue.Dequeue();
                if (!visited.Add(id) || !ports[id].enabled) continue;
                if (id != source && kinds[ports[id].deviceId] != DeviceKind.Passive) return true;
                foreach (var c in n.cables.Where(c => c.intact))
                {
                    if (c.portA == id && !string.IsNullOrEmpty(c.portB)) queue.Enqueue(c.portB);
                    if (c.portB == id && !string.IsNullOrEmpty(c.portA)) queue.Enqueue(c.portA);
                }
                foreach (var link in n.passiveConnections)
                {
                    if (link.portA == id) queue.Enqueue(link.portB);
                    if (link.portB == id) queue.Enqueue(link.portA);
                }
            }
            return false;
        }
    }
}
