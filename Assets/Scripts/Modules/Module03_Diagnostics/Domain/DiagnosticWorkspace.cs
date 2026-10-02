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
        private readonly Dictionary<(string source, string destination), ProbeResult> localObservations = new();
        public string LocalDeviceId { get; private set; }
        public string SelectedDeviceId { get; private set; }
        public string SourcePortId { get; }
        // El origen de las consultas locales sigue a la pantalla abierta.
        public string LocalSourcePortId => inventory.ports.FirstOrDefault(p =>
            p.deviceId == LocalDeviceId && !string.IsNullOrEmpty(p.ipv4))?.id ?? SourcePortId;
        public bool CanEditLocalAddress => inventory.devices.Any(d => d.id == LocalDeviceId && d.kind == DeviceKind.Computer);
        public bool CanManageSwitch => inventory.devices.Any(d => d.id == LocalDeviceId && d.kind == DeviceKind.DiagnosticStation) &&
            inventory.devices.Any(d => d.id == SelectedDeviceId && d.kind == DeviceKind.Switch);
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
            LastProbe = diagnostics.Ping(LocalSourcePortId, ip);
            observations[SelectedDeviceId] = LastProbe;
            localObservations[(LocalSourcePortId, SelectedDeviceId)] = LastProbe;
            return LastProbe;
        }
        public string Observation(string id)
        {
            if (!localObservations.TryGetValue((LocalSourcePortId, id), out var result)) return "Sin comprobar";
            if (!diagnostics.IsCurrent(result)) return "Obsoleto";
            if (result.Status != ProbeStatus.Success) return result.Status.ToString();
            return inventory.ports.Any(p => p.id == result.ResponderPort && p.deviceId == id) ? "Responde" : "Responde otro equipo";
        }
        public bool LastIsCurrent => diagnostics.IsCurrent(LastProbe);

        // La selección del minimapa nunca concede permisos para editar otra PC.
        public IReadOnlyList<PortDefinition> EditablePorts()
        {
            var n = session.Snapshot();
            var local = n.devices.Find(d => d.id == LocalDeviceId);
            string device = local?.kind == DeviceKind.Computer ? LocalDeviceId :
                local?.kind == DeviceKind.DiagnosticStation &&
                n.devices.Any(d => d.id == SelectedDeviceId && d.kind == DeviceKind.Switch) ? SelectedDeviceId : null;
            return n.ports.Where(p => device != null && p.deviceId == device).ToArray();
        }

        // La actividad evalúa direcciones duplicadas; el prefijo procede del inventario documentado.
        public int DocumentedPrefix(string portId) => inventory.ports.First(p => p.id == portId).prefixLength;
        public bool ConfigureLocalAddress(string portId, string ip) =>
            ConfigureLocalAddress(portId, ip, DocumentedPrefix(portId));

        public bool ConfigureLocalAddress(string portId, string ip, int prefix)
        {
            var n = session.Snapshot();
            if (!n.devices.Any(d => d.id == LocalDeviceId && d.kind == DeviceKind.Computer) ||
                !n.ports.Any(p => p.id == portId && p.deviceId == LocalDeviceId))
                throw new InvalidOperationException("La IP se configura presencialmente en la PC correspondiente.");
            return session.SetAddress(portId, ip, prefix);
        }

        public bool ConfigureSwitchPort(string portId, bool enabled)
        {
            var n = session.Snapshot();
            if (!n.devices.Any(d => d.id == LocalDeviceId && d.kind == DeviceKind.DiagnosticStation) ||
                !n.ports.Any(p => p.id == SourcePortId && p.deviceId == LocalDeviceId) ||
                !n.devices.Any(d => d.id == SelectedDeviceId && d.kind == DeviceKind.Switch) ||
                !n.ports.Any(p => p.id == portId && p.deviceId == SelectedDeviceId))
                throw new InvalidOperationException("Selecciona el switch desde la laptop para administrar sus puertos.");
            // Comprobar acceso en cada escritura: apagar el uplink corta también la administración.
            var ip = ExpectedAddress(SelectedDeviceId);
            if (string.IsNullOrEmpty(ip)) throw new InvalidOperationException("Sin IP de gestión documentada.");
            var access = diagnostics.Ping(SourcePortId, ip, 1);
            if (access.Status != ProbeStatus.Success || !n.ports.Any(p => p.id == access.ResponderPort && p.deviceId == SelectedDeviceId))
                throw new InvalidOperationException("Administración inaccesible: revisa la conexión de la laptop.");
            return session.SetPortEnabled(portId, enabled);
        }

        // La evidencia corresponde al equipo esperado, a esta sesión y a la revisión actual.
        public bool HasCurrentSuccessfulProbe(string deviceId) =>
            observations.TryGetValue(deviceId, out var result) && diagnostics.IsCurrent(result) &&
            result.Status == ProbeStatus.Success && inventory.ports.Any(p =>
                p.deviceId == deviceId && p.id == result.ResponderPort && p.ipv4 == result.DestinationIp);

        public string LocalConfiguration()
        {
            var n = session.Snapshot();
            var device = n.devices.FirstOrDefault(d => d.id == LocalDeviceId);
            if (device == null) return "Abre un dispositivo cercano con A.";
            if (device.kind == DeviceKind.Passive) return device.id + " · Elemento pasivo, sin IP.\n" +
                string.Join("\n", n.ports.Where(p => p.deviceId == device.id).Select(p => p.id));
            if (device.kind == DeviceKind.Switch) return device.id + " · Consulta sus puertos mediante administración desde la laptop.";
            return $"{LocalDeviceId}> ipconfig /all\n\n" + string.Join("\n\n", n.ports.Where(p => p.deviceId == LocalDeviceId).Select(p =>
                $"{p.id}\nIP: {(string.IsNullOrEmpty(p.ipv4) ? "No aplica" : p.ipv4 + "/" + p.prefixLength)}\nMáscara: {SubnetMask(p.prefixLength)}  MAC: {p.mac}\nAdministrativo: {(p.enabled ? "habilitado" : "inhabilitado")} · Enlace: {(HasLink(n, p.id) ? "activo" : "caído")}"));
        }
        public string Neighbours()
        {
            var entries = diagnostics.GetNeighbours(LocalSourcePortId);
            session.RecordArpQuery(LocalSourcePortId);
            string header = $"{LocalDeviceId}> arp -a\nInterfaz: {LocalSourcePortId}\n\n";
            if (entries.Count == 0)
                return header + "Sin entradas ARP aprendidas.\nEjecuta ping a otro equipo de la red.\nLa tabla se limpia cuando cambia la red.";
            return header + $"{"Dirección IP",-15}  {"Dirección MAC",-17}  Tipo\n" +
                string.Join("\n", entries.Select(n => $"{n.Ip,-15}  {n.Mac,-17}  dinámica")) +
                "\n\nAprendida mediante ARP; no es el inventario.\nUna entrada no garantiza respuesta ICMP.";
        }

        public string PingOutput(ProbeResult result)
        {
            string response = result.Status == ProbeStatus.Success
                ? string.Join("\n", Enumerable.Range(1, result.Received).Select(i => $"Respuesta de {result.DestinationIp} ({i}/{result.Sent})"))
                : "Sin respuesta.\n" + result.Message;
            int lost = result.Sent - result.Received;
            return $"{LocalDeviceId}> ping {result.DestinationIp}\nOrigen: {result.SourcePort}\n\n{response}\n\n" +
                $"Enviados = {result.Sent}, recibidos = {result.Received}\nPerdidos = {lost} ({lost * 100 / result.Sent}% pérdida)\n" +
                (result.ResponderPort == null ? "" : $"Interfaz que respondió: {result.ResponderPort}\n") +
                (result.Status == ProbeStatus.Success ? result.Message + "\n" : "") +
                "Simulación didáctica: no mide latencia ni TTL.";
        }

        public string History()
        {
            var entries = session.History.Skip(Math.Max(0, session.History.Count - 30));
            return "BITÁCORA DE LA SESIÓN\nÚltimas 30 acciones de la red\n\n" + string.Join("\n\n", entries.Select(h =>
                $"[{h.TimestampUtc:HH:mm:ss} UTC] #{h.Sequence} · {(h.Accepted ? "OK" : "FALLO")}\n" +
                HistoryCommand(h) + (string.IsNullOrEmpty(h.Reason) ? "" : "\n" + h.Reason)));
        }

        private static string HistoryCommand(NetworkActionRecord entry) => entry.Action switch
        {
            "Ping" => $"{entry.EntityId}> ping {entry.After}",
            "Arp" => $"{entry.EntityId}> arp -a",
            "Address" => $"{entry.EntityId} · Configurar IPv4\n{entry.Before} -> {entry.After}",
            "PortEnabled" => $"{entry.EntityId}(config-if)# {(entry.After == "True" ? "no shutdown" : "shutdown")}",
            "Connect" => $"{entry.EntityId} · Conexión física\n{entry.Before} -> {entry.After}",
            "CableTest" => $"{entry.EntityId} · Prueba de continuidad\n{entry.After}",
            "Label" => $"{entry.EntityId} · Etiquetado\n{entry.After}",
            "Reset" => "Reinicio del escenario: las evidencias anteriores dejan de ser válidas.",
            _ => $"{entry.EntityId} · {entry.Action}\n{entry.After}"
        };
        public string SwitchPorts(string switchId)
        {
            var n = session.Snapshot();
            if (!n.devices.Any(d => d.id == switchId && d.kind == DeviceKind.Switch)) return "Selecciona un switch.";
            string ip = ExpectedAddress(switchId);
            if (ip == null) return "Sin interfaz de gestión documentada.";
            var access = diagnostics.Ping(SourcePortId, ip, 1);
            if (access.Status != ProbeStatus.Success || !n.ports.Any(p => p.id == access.ResponderPort && p.deviceId == switchId))
                return "Administración inaccesible desde la laptop.";
            session.RecordSwitchQuery(switchId);
            var management = n.ports.Where(p => p.deviceId == switchId && !string.IsNullOrEmpty(p.ipv4));
            return $"Conectado a {switchId} por gestión LAN\n{switchId}# show ip interface brief\n\n" +
                string.Join("\n", management.Select(p => $"Gestión: {p.ipv4}/{p.prefixLength}\nMáscara: {SubnetMask(p.prefixLength)}  MAC: {p.mac}")) +
                $"\n\n{switchId}# show interfaces status\n\n" +
                string.Join("\n", n.ports.Where(p => p.deviceId == switchId).Select(p =>
            {
                var cable = n.cables.FirstOrDefault(c => c.portA == p.id || c.portB == p.id);
                // Conexión física no implica integridad ni conectividad IP; no revela daños remotos.
                bool connected = cable != null;
                return $"{p.id}: admin={(p.enabled ? "ON" : "OFF")}; cable local={(connected ? "conectado" : "sin cable")}; enlace={(HasLink(n, p.id) ? "activo" : "caído / no aplica a interfaz lógica")}";
            }));
        }
        private static string SubnetMask(int prefix)
        {
            uint mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
            return $"{mask >> 24}.{(mask >> 16) & 255}.{(mask >> 8) & 255}.{mask & 255}";
        }
        // El enlace físico termina en la siguiente interfaz activa. No atraviesa internamente switches.
        public static bool HasLink(NetworkDefinition n, string source)
        {
            var ports = n.ports.ToDictionary(p => p.id); var kinds = n.devices.ToDictionary(d => d.id, d => d.kind);
            if (source == null || !ports.ContainsKey(source)) return false;
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
