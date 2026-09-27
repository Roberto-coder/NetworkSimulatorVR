using Modules.Module03_Diagnostics.Domain;

static class SimulationChecks
{
    public static void Run(Func<int, NetworkDefinition> office, Action<bool, string> check)
    {
        var n = office(3);
        n.devices.Add(new DeviceDefinition { id = "R", kind = DeviceKind.Passive, prefabKey = "rosette" });
        n.ports.Add(new PortDefinition { id = "R/back", deviceId = "R" });
        n.ports.Add(new PortDefinition { id = "R/front", deviceId = "R" });
        n.passiveConnections.Add(new PassiveConnection { portA = "R/back", portB = "R/front" });
        n.cables[0].portA = "R/front"; n.cables[0].intact = false;
        n.cables.Add(new CableDefinition { id = "horizontal", portA = "SW/P1", portB = "R/back" });
        n.ports.Add(new PortDefinition { id = "SW/mgmt", deviceId = "SW", ipv4 = "192.168.10.2", mac = "02:00:00:00:01:02" });
        var session = new NetworkSession(n); var service = new NetworkSimulationService(session);
        var broken = service.Ping("PC2/eth", "192.168.10.11");
        check(broken.Status == ProbeStatus.AddressUnresolved, "Rotura impide resolver vecino");
        check(broken.Trace.All(t => t.Phase == TracePhase.ArpRequest && t.LinkId != "C1"), "Sin ICMP ni cruce del cable roto");
        session.ConnectCable("C1", "", ""); session.ConnectCable("spare", "R/front", "PC1/eth");
        var repaired = service.Ping("PC2/eth", "192.168.10.11");
        check(repaired.Status == ProbeStatus.Success && repaired.Received == 4, "Reparación restablece cuatro respuestas");
        check(repaired.Trace.Any(t => t.Kind == TraceLinkKind.PassiveContinuity && t.Phase == TracePhase.EchoReply), "Respuesta atraviesa continuidad pasiva");
        var request = repaired.Trace.Where(t => t.Phase == TracePhase.EchoRequest).Take(5).ToArray();
        var reply = repaired.Trace.Where(t => t.Phase == TracePhase.EchoReply).Take(5).ToArray();
        check(request.Length == 5 && reply.Select(t => t.FromPort).SequenceEqual(request.Reverse().Select(t => t.ToPort)), "Ida/vuelta invierte extremos");
        check(service.GetNeighbours("PC2/eth").Single().Mac == "02:00:00:00:00:01", "ARP aprendido del equipo que respondió");
        var cached = service.Ping("PC2/eth", "192.168.10.11", 1);
        check(cached.Trace.All(t => t.Phase != TracePhase.ArpRequest), "Vecino conocido evita nuevo ARP");
        check(service.IsCurrent(repaired) && !service.IsCurrent(broken), "Revisión de evidencia");
        check(!new NetworkSimulationService(new NetworkSession(n)).IsCurrent(broken), "Evidencia de otra sesión rechazada");
        session.SetPortEnabled("SW/P1", false);
        check(service.GetNeighbours("PC2/eth").Count == 0, "Cambio de red limpia cache");
        var disabled = service.Ping("PC2/eth", "192.168.10.11");
        check(disabled.Status == ProbeStatus.AddressUnresolved && disabled.Trace.All(t => t.ToPort != "SW/P1"), "Puerto inhabilitado no conduce tráfico");
        session.SetPortEnabled("SW/P1", true);
        check(service.Ping("PC2/eth", "192.168.10.11").Status == ProbeStatus.Success, "Habilitar recupera ping");
        session.SetAddress("PC3/eth", "192.168.10.11", 24);
        var conflict = service.Ping("PC2/eth", "192.168.10.11");
        check(conflict.Status == ProbeStatus.AddressConflict && conflict.Received == 0, "Duplicidad alcanzable detectada");
        check(conflict.Trace.All(t => t.Phase != TracePhase.EchoRequest) && service.GetNeighbours("PC2/eth").Count == 0, "Conflicto no aprende vecino arbitrario ni envía ICMP");
        session.SetPortEnabled("SW/P3", false);
        check(service.Ping("PC2/eth", "192.168.10.11").Status == ProbeStatus.Success, "Equipo aislado no responde ARP ni revela duplicidad");
        session.SetPortEnabled("SW/P3", true); session.SetAddress("PC3/eth", "192.168.10.13", 24);
        check(service.Ping("PC2/eth", "192.168.10.13").Status == ProbeStatus.Success, "Corrección de IP restablece respuesta");
        check(service.Ping("PC2/eth", "192.168.10.2").ResponderPort == "SW/mgmt", "Gestión de switch accesible desde puerto físico");
        check(service.Ping("PC2/eth", "10.0.0.1").Status == ProbeStatus.NoRoute, "Sin gateway no cruza subred");
        check(service.Ping("PC2/eth", "192.168.10.250").Status == ProbeStatus.AddressUnresolved, "IP inexistente");
        foreach (string ip in new[] { "bad", "192.168.10.255", "192.168.10.0", "224.0.0.1", "127.0.0.1" })
            check(service.Ping("PC2/eth", ip).Status == ProbeStatus.InvalidDestination, "Destino inválido no genera paquetes");
        check(service.Ping("R/front", "192.168.10.12").Status == ProbeStatus.InvalidSource, "Pasivo no origina ping");
        check(service.Ping("missing", "192.168.10.12").Status == ProbeStatus.InvalidSource, "Origen inexistente");
        var local = service.Ping("PC2/eth", "192.168.10.12");
        check(local.Status == ProbeStatus.Success && local.Trace.Count == 0, "Ping local no prueba cable");
        session.SetAddress("PC1/eth", "192.168.10.12", 24);
        check(service.Ping("PC2/eth", "192.168.10.13").Status == ProbeStatus.AddressConflict, "IP origen duplicada");
        session.SetAddress("PC1/eth", "192.168.10.11", 24);
        session.SetAddress("PC3/eth", "192.168.10.130", 25);
        var asymmetric = service.Ping("PC2/eth", "192.168.10.130");
        check(asymmetric.Status == ProbeStatus.ReplyUnavailable && asymmetric.Trace.Any(t => t.Phase == TracePhase.EchoRequest) &&
            asymmetric.Trace.All(t => t.Phase != TracePhase.EchoReply), "Máscara asimétrica impide retorno");
        session.SetPortEnabled("PC2/eth", false);
        check(service.Ping("PC2/eth", "192.168.10.11").Status == ProbeStatus.SourceDisabled, "Origen deshabilitado");
        session.Reset(); check(!service.IsCurrent(asymmetric) && service.GetNeighbours("PC2/eth").Count == 0, "Reinicio invalida evidencia y ARP");
        check(session.History.Any(h => h.Action == "Ping"), "Consultas registradas en bitácora");
        var extended = office(12); var scalable = new NetworkSimulationService(new NetworkSession(extended));
        check(scalable.Ping("PC1/eth", "192.168.10.22").Status == ProbeStatus.Success, "Doce PCs sin lógica por índice fijo");
        // Un PC con dos interfaces no actúa como router/switch.
        extended.devices.Find(d => d.id == "SW").kind = DeviceKind.Computer;
        check(new NetworkSimulationService(new NetworkSession(extended)).Ping("PC1/eth", "192.168.10.12").Status == ProbeStatus.AddressUnresolved, "Hosts no reenvían tráfico entre interfaces");
        extended = office(3); extended.ports.Find(p => p.id == "SW/P3").segmentId = "isolated";
        check(new NetworkSimulationService(new NetworkSession(extended)).Ping("PC1/eth", "192.168.10.13").Status == ProbeStatus.AddressUnresolved, "No cruza segmentos configurados");
        var cyclic = office(3);
        cyclic.ports.Add(new PortDefinition { id = "SW/loopA", deviceId = "SW" });
        cyclic.ports.Add(new PortDefinition { id = "SW/loopB", deviceId = "SW" });
        cyclic.cables.Add(new CableDefinition { id = "loop", portA = "SW/loopA", portB = "SW/loopB" });
        var cycleResult = new NetworkSimulationService(new NetworkSession(cyclic)).Ping("PC1/eth", "192.168.10.12");
        check(cycleResult.Status == ProbeStatus.Success && cycleResult.Trace.Count < 100, "Ciclo termina con árbol ARP acotado");
        cyclic.ports.Reverse(); cyclic.cables.Reverse(); cyclic.devices.Reverse();
        var reordered = new NetworkSimulationService(new NetworkSession(cyclic)).Ping("PC1/eth", "192.168.10.12");
        check(reordered.Trace.Select(t => t.Phase + t.FromPort + t.ToPort).SequenceEqual(cycleResult.Trace.Select(t => t.Phase + t.FromPort + t.ToPort)), "Traza determinista independiente del orden serializado");
        check(reordered.Trace.Where(t => t.Phase == TracePhase.EchoRequest).Select(t => t.Attempt).Distinct().SequenceEqual(new[] { 1, 2, 3, 4 }), "Identidad de cada intento para animación");
        bool rejected = false;
        try { service.Ping("PC2/eth", "192.168.10.11", 0); } catch (ArgumentOutOfRangeException) { rejected = true; }
        check(rejected, "Cantidad inválida rechazada");
        long before = session.Revision; service.Ping("PC2/eth", "192.168.10.13");
        check(session.Revision == before, "Diagnóstico no muta revisión de red");
    }
}
