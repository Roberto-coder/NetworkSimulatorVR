using Modules.Module03_Diagnostics.Domain;
using GameData.Module03;

static class AdministrationChecks
{
    public static void Run(Func<int, NetworkDefinition> office, Action<bool, string> check)
    {
        var n = office(3);
        n.devices.Find(d => d.id == "PC1").kind = DeviceKind.DiagnosticStation;
        n.ports.Add(new PortDefinition { id = "SW/mgmt", deviceId = "SW", ipv4 = "192.168.10.2", mac = "02:00:00:00:01:02" });
        var expected = n.Copy();
        n.ports.Find(p => p.id == "SW/P2").enabled = false;
        n.ports.Find(p => p.id == "PC3/eth").ipv4 = "192.168.10.12";
        n.allowedDuplicatePortIds.AddRange(new[] { "PC2/eth", "PC3/eth" });
        var session = new NetworkSession(n);
        var ui = new DiagnosticWorkspace(session, new NetworkSimulationService(session), expected, "PC1/eth");
        void Rejected(Action action, string reason)
        {
            bool rejected = false;
            try { action(); } catch (InvalidOperationException) { rejected = true; }
            check(rejected, reason);
        }
        ui.OpenLocal("PC1"); ui.SelectDestination("PC2");
        check(ui.EditablePorts().Count == 0, "Laptop no edita PC seleccionada remotamente");
        Rejected(() => ui.ConfigureLocalAddress("PC2/eth", "192.168.10.14", 24), "Dirección requiere presencia local");
        ui.PingSelected();
        check(!ui.HasCurrentSuccessfulProbe("PC2") && ui.Observation("PC2") == "Responde otro equipo", "Respuesta de IP duplicada aislada no verifica identidad");
        ui.SelectDestination("SW");
        check(ui.EditablePorts().All(p => p.deviceId == "SW"), "Laptop administra solo switch seleccionado");
        check(ui.ConfigureSwitchPort("SW/P2", true), "Habilitar puerto con gestión alcanzable");
        ui.SelectDestination("PC2");
        check(ui.PingSelected().Status == ProbeStatus.AddressConflict, "Puerto habilitado expone conflicto ARP");
        ui.OpenLocal("PC3"); ui.SelectDestination("PC2");
        check(ui.EditablePorts().Single().id == "PC3/eth", "Minimapa no cambia contexto de edición");
        Rejected(() => ui.ConfigureLocalAddress("PC2/eth", "192.168.10.14", 24), "PC no puede editar otra PC");
        Rejected(() => ui.ConfigureSwitchPort("SW/P2", false), "PC no puede administrar switch");
        long revision = session.Revision;
        check(!ui.ConfigureLocalAddress("PC3/eth", "invalid", 24) && session.Revision == revision, "IP inválida no muta red");
        check(!ui.ConfigureLocalAddress("PC3/eth", "192.168.10.13", 33), "Prefijo inválido rechazado");
        check(ui.ConfigureLocalAddress("PC3/eth", "192.168.10.13", 24), "Corregir IP local");
        check(!ui.HasCurrentSuccessfulProbe("PC2"), "Cambios requieren nueva evidencia");
        ui.SelectDestination("PC2"); ui.PingSelected();
        ui.SelectDestination("PC3"); ui.PingSelected();
        check(ui.HasCurrentSuccessfulProbe("PC2") && ui.HasCurrentSuccessfulProbe("PC3"), "Ambas verificaciones coexisten en misma revisión");
        ui.OpenLocal("PC1"); ui.SelectDestination("SW");
        revision = session.Revision;
        ui.ConfigureSwitchPort("SW/P2", true);
        check(session.Revision == revision && ui.HasCurrentSuccessfulProbe("PC3"), "Escritura idempotente conserva evidencia");
        ui.ConfigureSwitchPort("SW/P1", false);
        Rejected(() => ui.ConfigureSwitchPort("SW/P1", true), "Sin uplink no hay escritura remota");
        session.Reset();
        check(!ui.HasCurrentSuccessfulProbe("PC3"), "Reset invalida evidencia de reparación");

        ui.OpenLocal("PC3");
        session.SetAddress("PC3/eth", "192.168.10.13", 16);
        check(ui.ConfigureLocalAddress("PC3/eth", "192.168.10.13") && session.Snapshot().ports.Find(p => p.id == "PC3/eth").prefixLength == 24,
            "Edición de IP restaura el prefijo documentado sin selector");

        // Las reglas finales alimentan el inventario, pero no reparan el asset inicial.
        var initial = new NetworkInitialStateAsset();
        var targets = new NetworkTargetRulesAsset();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(NetworkInitialStateAsset).GetField("definition", flags).SetValue(initial, n);
        typeof(NetworkTargetRulesAsset).GetField("definition", flags).SetValue(targets, new NetworkTargetDefinition
        {
            rules = new() { new() { id = "ip3", kind = NetworkRuleKind.Address, portA = "PC3/eth", ipv4 = "192.168.10.13", prefixLength = 24 } }
        });
        check(targets.DocumentedInventory(initial).ports.Find(p => p.id == "PC3/eth").ipv4 == "192.168.10.13" &&
            initial.CopyDefinition().ports.Find(p => p.id == "PC3/eth").ipv4 == "192.168.10.12", "Inventario documentado independiente de falla inicial");
    }
}
