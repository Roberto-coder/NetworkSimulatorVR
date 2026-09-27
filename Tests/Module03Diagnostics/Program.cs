using Modules.Module03_Diagnostics.Domain;

static class Program
{
    private static int checks;
    private static void Check(bool ok, string reason)
    {
        checks++;
        if (!ok) throw new Exception(reason);
    }
    private static void Invalid(NetworkDefinition n, string reason) => Check(NetworkDefinitionValidator.Validate(n).Count > 0, reason);
    private static NetworkDefinition Office(int count = 3)
    {
        var n = new NetworkDefinition();
        n.devices.Add(new DeviceDefinition { id = "SW", kind = DeviceKind.Switch, prefabKey = "switch" });
        for (int i = 1; i <= count; i++)
        {
            n.devices.Add(new DeviceDefinition { id = $"PC{i}", kind = DeviceKind.Computer, prefabKey = "pc" });
            n.ports.Add(new PortDefinition { id = $"PC{i}/eth", deviceId = $"PC{i}", ipv4 = $"192.168.10.{10 + i}", mac = $"02:00:00:00:00:{i:X2}" });
            n.ports.Add(new PortDefinition { id = $"SW/P{i}", deviceId = "SW" });
            n.cables.Add(new CableDefinition { id = $"C{i}", portA = $"SW/P{i}", portB = $"PC{i}/eth", interactable = true });
        }
        n.cables.Add(new CableDefinition { id = "spare", interactable = true });
        return n;
    }
    private static NetworkTargetDefinition Targets() => new NetworkTargetDefinition
    {
        rules = new List<NetworkTargetRule>
        {
            new() { id = "unique", kind = NetworkRuleKind.UniqueAddresses },
            new() { id = "ip", kind = NetworkRuleKind.Address, portA = "PC3/eth", ipv4 = "192.168.10.13" },
            new() { id = "port", kind = NetworkRuleKind.PortEnabled, portA = "SW/P2" },
            new() { id = "cable", kind = NetworkRuleKind.HealthyCableBetween, portA = "SW/P1", portB = "PC1/eth",
                requireLabels = true, labelAtA = "SW/P1", labelAtB = "PC1/eth" }
        }
    };

    static void Main()
    {
        foreach (var value in new[] { "127.0.0.1", "192.168.10.255", "0.0.0.0" }) Check(NetworkDefinitionValidator.IsIpv4(value), "Sintaxis IPv4 válida");
        foreach (var value in new[] { "127.1", "001.2.3.4", "1.2.3.256", "1.2.3.-1", "1.2.3. 4", "a.b.c.d", "" })
            Check(!NetworkDefinitionValidator.IsIpv4(value), "Rechazar sintaxis ambigua");
        var initial = Office();
        Check(NetworkDefinitionValidator.Validate(initial).Count == 0, "Oficina válida");
        var broken = initial.Copy(); broken.devices.Add(broken.devices[0]); Invalid(broken, "IDs repetidos");
        broken = initial.Copy(); broken.devices[0] = null; Invalid(broken, "Entrada nula");
        broken = initial.Copy(); broken.ports[0].deviceId = "missing"; Invalid(broken, "Dispositivo inexistente");
        broken = initial.Copy(); broken.cables[0].portA = "missing"; Invalid(broken, "Extremo inexistente");
        broken = initial.Copy(); broken.cables[1].portA = broken.cables[0].portA; Invalid(broken, "Puerto ocupado");
        broken = initial.Copy(); broken.cables[0].portB = broken.cables[0].portA; Invalid(broken, "Mismo extremo");
        broken = initial.Copy(); broken.ports[2].mac = broken.ports[0].mac; Invalid(broken, "MAC duplicada");
        broken = initial.Copy(); broken.ports[0].prefixLength = 33; Invalid(broken, "Prefijo inválido");
        broken = initial.Copy(); broken.schemaVersion = 42; Invalid(broken, "Esquema desconocido");
        broken = initial.Copy(); broken.cables[3].interactable = false; Invalid(broken, "Cable fijo sin extremos");
        broken = initial.Copy(); broken.ports[2].ipv4 = broken.ports[0].ipv4; Invalid(broken, "Duplicidad no declarada");
        broken.allowedDuplicatePortIds.AddRange(new[] { "PC1/eth", "PC2/eth" });
        Check(NetworkDefinitionValidator.Validate(broken).Count == 0, "Conflicto didáctico declarado");
        Check(!NetworkTargetValidator.Evaluate(new NetworkSession(broken), Targets())[0].Passed, "Excepción inicial no excusa estado final");
        broken.ports[2].segmentId = "another"; broken.allowedDuplicatePortIds.Clear();
        Check(NetworkDefinitionValidator.Validate(broken).Count == 0, "IP igual en segmento diferente");

        var s = new NetworkSession(initial);
        initial.ports[0].ipv4 = "changed";
        var snap = s.Snapshot(); snap.ports[0].ipv4 = "changed again";
        Check(s.Snapshot().ports[0].ipv4 == "192.168.10.11", "Copias profundas de entrada y salida");
        Check(s.GetPhysicalNeighbours("PC1/eth").SequenceEqual(new[] { "SW/P1" }), "Vecindad derivada");
        Check(!s.ConnectCable("spare", "SW/P1", "PC1/eth"), "No pisar puertos ocupados");
        Check(s.Revision == 0 && !s.History[0].Accepted, "Rechazo registrado sin mutación");
        Check(s.SetPortEnabled("SW/P2", false), "Inhabilitar puerto");
        long revision = s.Revision;
        s.SetPortEnabled("SW/P2", false);
        Check(s.Revision == revision, "Operación repetida no invalida evidencia");
        Check(s.SetAddress("PC3/eth", "192.168.10.12", 24), "Puede introducirse conflicto");
        var results = NetworkTargetValidator.Evaluate(s, Targets());
        Check(results.All(r => !r.Passed), "Cuatro reglas pendientes con observaciones");
        Check(!s.SetAddress("PC3/eth", "not-ip", 24), "Configuración inválida rechazada");
        s.SetAddress("PC3/eth", "192.168.10.13", 24); s.SetPortEnabled("SW/P2", true);
        Check(s.ConnectCable("C1", "", ""), "Retirar cable");
        Check(s.GetPhysicalNeighbours("PC1/eth").Count == 0, "Desconexión actualiza vecindad");
        Check(s.ConnectCable("spare", "PC1/eth", "SW/P1"), "Repuesto con extremos invertidos");
        s.LabelCable("spare", "SW/P1", "PC1/eth");
        Check(!NetworkTargetValidator.Evaluate(s, Targets())[3].Passed, "Etiquetas dependen del extremo");
        s.LabelCable("spare", "PC1/eth", "SW/P1");
        Check(NetworkTargetValidator.Evaluate(s, Targets()).All(r => r.Passed), "Reparación con ID nuevo");
        revision = s.Revision; s.Reset();
        Check(s.Revision > revision && s.Snapshot().cables[0].portA == "SW/P1", "Reset restaura y obsoleta evidencia");
        Check(s.History.Select(h => h.Sequence).SequenceEqual(Enumerable.Range(1, s.History.Count).Select(i => (long)i)), "Secuencia de bitácora");

        var bigger = Office(12); var large = new NetworkSession(bigger);
        large.SetAddress("PC12/eth", "192.168.10.11", 24);
        Check(!NetworkTargetValidator.Evaluate(large, Targets())[0].Passed, "Regla global incluye nuevas PCs");
        bigger.devices.Reverse(); bigger.ports.Reverse(); bigger.cables.Reverse();
        Check(NetworkDefinitionValidator.Validate(bigger).Count == 0, "Orden de listas independiente");
        var invalidTargets = Targets(); invalidTargets.rules[1].portA = "missing";
        Check(NetworkTargetValidator.ValidateConfiguration(Office(), invalidTargets).Count > 0, "Regla con referencia inexistente");
        invalidTargets = Targets(); invalidTargets.rules[1].kind = (NetworkRuleKind)999;
        Check(NetworkTargetValidator.ValidateConfiguration(Office(), invalidTargets).Count > 0, "Regla desconocida");
        invalidTargets = Targets(); invalidTargets.rules.Add(invalidTargets.rules[0]);
        Check(NetworkTargetValidator.ValidateConfiguration(Office(), invalidTargets).Count > 0, "ID de regla repetido");
        Check(NetworkTargetValidator.ValidateConfiguration(Office(), new NetworkTargetDefinition()).Count > 0, "Evitar éxito vacío");

        var passive = Office();
        passive.devices.Add(new DeviceDefinition { id = "R", kind = DeviceKind.Passive, prefabKey = "rosette" });
        passive.ports.Add(new PortDefinition { id = "R/in", deviceId = "R" });
        passive.ports.Add(new PortDefinition { id = "R/out", deviceId = "R" });
        passive.passiveConnections.Add(new PassiveConnection { portA = "R/in", portB = "R/out" });
        var ps = new NetworkSession(passive);
        Check(ps.GetPhysicalNeighbours("R/in").Contains("R/out"), "Continuidad de roseta explícita");
        Check(!ps.SetAddress("R/in", "192.168.10.50", 24), "Pasivo sin dirección");
        Check(!ps.SetPortEnabled("R/in", false), "Pasivo sin administración");
        passive.ports.Find(p => p.id == "R/in").deviceId = null;
        Invalid(passive, "Dispositivo nulo con continuidad no lanza excepción");
        passive.ports.Find(p => p.id == "R/in").deviceId = "R";
        passive.passiveConnections[0].portB = "PC1/eth"; Invalid(passive, "Continuidad entre dispositivos rechazada");

        // Serializar DTOs no arrastra GameObjects ni referencias a los assets.
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
        var serialized = System.Text.Json.JsonSerializer.Serialize(Office(), jsonOptions);
        var restored = System.Text.Json.JsonSerializer.Deserialize<NetworkDefinition>(serialized, jsonOptions);
        Check(NetworkDefinitionValidator.Validate(restored).Count == 0 && restored.ports.Count == 6, "DTO serializable con ida/vuelta JSON");
        var initialAsset = new GameData.Module03.NetworkInitialStateAsset();
        typeof(GameData.Module03.NetworkInitialStateAsset).GetField("definition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(initialAsset, Office());
        var firstSession = initialAsset.CreateSession(); var secondSession = initialAsset.CreateSession();
        firstSession.SetPortEnabled("SW/P1", false);
        Check(secondSession.Snapshot().ports.Find(p => p.id == "SW/P1").enabled &&
            initialAsset.CopyDefinition().ports.Find(p => p.id == "SW/P1").enabled, "Sesiones independientes del mismo asset");
        SimulationChecks.Run(Office, Check);
        WorkspaceChecks.Run(Office, Check);
        Console.WriteLine($"Module03Diagnostics: {checks} comprobaciones correctas.");
    }
}
