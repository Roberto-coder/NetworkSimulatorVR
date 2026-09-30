using Modules.Module03_Diagnostics.Domain;

static class WorkspaceChecks
{
    public static void Run(Func<int, NetworkDefinition> office, Action<bool, string> check)
    {
        var n = office(3);
        n.ports.Add(new PortDefinition { id = "SW/mgmt", deviceId = "SW", ipv4 = "192.168.10.2", mac = "02:00:00:00:01:02" });
        var session = new NetworkSession(n); var service = new NetworkSimulationService(session);
        var ui = new DiagnosticWorkspace(session, service, n, "PC1/eth");
        check(ui.Observation("PC2") == "Sin comprobar", "Mapa no revela salud inicial");
        ui.OpenLocal("PC1"); ui.SelectDestination("PC2");
        check(ui.LocalDeviceId == "PC1" && ui.SelectedDeviceId == "PC2", "Seleccionar nodo no concede acceso local remoto");
        check(ui.LocalConfiguration().Contains("192.168.10.11") && !ui.LocalConfiguration().Contains("192.168.10.12"), "Consulta local se limita al contexto");
        check(ui.PingSelected().SourcePort == "PC1/eth" && ui.Observation("PC2") == "Responde", "Ping desde estación configurada");
        check(ui.Neighbours().Contains("192.168.10.12"), "ARP presenta datos observados");
        session.SetPortEnabled("SW/P2", false);
        check(ui.Observation("PC2") == "Obsoleto" && !ui.LastIsCurrent, "Mapa invalida observación por revisión");
        ui.PingSelected(); check(ui.Observation("PC2") == "AddressUnresolved", "Síntoma visible sin diagnosticar causa automáticamente");
        check(ui.SwitchPorts("SW").Contains("SW/P2: admin=OFF"), "Gestión alcanzable permite consulta de puertos");
        session.SetPortEnabled("SW/P1", false);
        check(ui.SwitchPorts("SW").Contains("inaccesible"), "Gestión inaccesible no revela estado remoto");
        ui.OpenLocal("SW"); check(ui.SwitchPorts("SW").Contains("inaccesible"), "A en switch no evita comprobación de gestión");
        ui.OpenLocal("PC2"); check(ui.LocalConfiguration().Contains("192.168.10.12"), "Acceso físico conserva consulta de PC aislada");
        check(ui.LocalConfiguration().Contains("caído"), "Puerto remoto deshabilitado baja enlace local");
        session.SetPortEnabled("SW/P1", true); session.SetPortEnabled("SW/P2", true);
        session.SetAddress("PC2/eth", "192.168.10.20", 24); session.SetAddress("PC3/eth", "192.168.10.12", 24);
        ui.SelectDestination("PC2"); ui.PingSelected();
        check(ui.ExpectedAddress("PC2") == "192.168.10.12" && ui.Observation("PC2") == "Responde otro equipo", "IP del inventario no garantiza identidad del respondiente");
        check(ui.History().Contains("Ping"), "Bitácora incluye diagnósticos");
        session.Reset(); check(ui.Observation("PC2") == "Obsoleto", "Reinicio invalida mapa");

        // Abrir una PC cambia el origen real, no solo el título de la consola.
        ui.OpenLocal("PC2"); ui.SelectDestination("PC3");
        check(ui.PingSelected().SourcePort == "PC2/eth", "Ping se origina en la PC abierta");
        check(ui.Neighbours().Contains("192.168.10.13"), "ARP consulta la caché de la PC abierta");
        check(ui.CanEditLocalAddress && !ui.CanManageSwitch, "PC ofrece IP propia y no administración de switch");
        ui.SelectDestination("SW");
        check(!ui.CanManageSwitch, "Seleccionar switch desde PC no habilita administración");
        check(ui.LocalConfiguration().Contains("255.255.255.0"), "Configuración muestra máscara decimal");
        ui.OpenLocal("PC1");
        check(ui.Observation("PC3") == "Sin comprobar", "Mapa no mezcla observaciones de distintos orígenes");
    }
}
