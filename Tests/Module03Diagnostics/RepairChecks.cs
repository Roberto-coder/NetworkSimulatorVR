using Modules.Module03_Diagnostics.Domain;

static class RepairChecks
{
    public static void Run(Func<int, NetworkDefinition> office, Action<bool,string> check)
    {
        var n = office(3); n.cables[0].intact = false;
        var s = new NetworkSession(n); var sim = new NetworkSimulationService(s);
        var rules = new CableRepairDefinition { faultyCableId="C1", portA="SW/P1", portB="PC1/eth", labelAtA="SW", labelAtB="PC1",
            probeSource="PC2/eth", destinationIp="192.168.10.11", responderPort="PC1/eth" };
        var repair = new CableRepairService(s,sim,rules);
        check(!repair.IsComplete,"No completar sin reparaciÃ³n");
        check(!repair.TestCable("C1",out _),"Tester rechaza cable conectado");
        s.ConnectCable("C1","","PC1/eth"); check(!repair.TestCable("C1",out _),"Tester rechaza un extremo conectado");
        s.ConnectCable("C1","",""); check(repair.TestCable("C1",out var failure) && failure.Contains("Sin continuidad"),"Rotura diagnosticada");
        check(!repair.TestCable("inexistente",out _),"Cable desconocido rechazado");
        check(repair.TestCable("spare",out var healthy) && healthy.Contains("correcta"),"Repuesto comprobado");
        s.ConnectCable("spare","PC1/eth","SW/P1");
        check(!repair.LabelEnd("C1",true,"SW",out _),"No etiquetar cable descartado");
        repair.ObserveProbe(sim.Ping("PC2/eth","192.168.10.11")); check(!repair.IsComplete,"Ping no sustituye etiquetas");
        repair.LabelEnd("spare",true,"incorrecta",out _); repair.LabelEnd("spare",false,"SW",out _);
        check(!repair.IsComplete,"Etiqueta incorrecta no cumple");
        repair.LabelEnd("spare",true,"PC1",out _); check(!repair.IsComplete,"Etiquetado invalida ping anterior");
        repair.ObserveProbe(sim.Ping("PC2/eth","192.168.10.11")); check(repair.IsComplete,"Cable invertido y etiquetas orientadas correctamente cumplen");
        s.SetPortEnabled("SW/P1",false); check(!repair.IsComplete,"RevisiÃ³n nueva invalida verificaciÃ³n");
        s.Reset(); check(!repair.IsComplete,"Reset borra evidencias");
        check(s.History.Any(h=>h.Action=="CableTest" && !h.Accepted),"Tester registra rechazo");
        // Un cable bien conectado y etiquetado tampoco suple la evidencia del tester.
        s.ConnectCable("C1","",""); repair.TestCable("C1",out _);
        s.ConnectCable("spare","SW/P1","PC1/eth");
        repair.LabelEnd("spare",true,"SW",out _); repair.LabelEnd("spare",false,"PC1",out _);
        repair.ObserveProbe(sim.Ping("PC2/eth","192.168.10.11")); check(!repair.IsComplete,"Sin prueba del repuesto no completar");
        s.ConnectCable("spare","",""); repair.TestCable("spare",out _);
        s.ConnectCable("spare","SW/P1","PC1/eth");
        var foreign = new NetworkSimulationService(new NetworkSession(s.Snapshot())).Ping("PC2/eth","192.168.10.11");
        repair.ObserveProbe(foreign); check(!repair.IsComplete,"No aceptar evidencia de otra sesión");
        repair.ObserveProbe(sim.Ping("PC3/eth","192.168.10.11")); check(!repair.IsComplete,"Origen incorrecto no verifica");
        repair.ObserveProbe(sim.Ping("PC2/eth","192.168.10.11")); check(repair.IsComplete,"Flujo completo después de Reset");
        s.ConnectCable("spare","SW/P1",""); check(!repair.IsComplete,"Retirar extremo revoca resolución");
        check(!repair.LabelEnd("spare",true,"SW",out _),"Repuesto a medio conectar no se etiqueta");

        // Los otros incidentes no deben bloquear la reparación física de PC1.
        var concurrent = office(4);
        concurrent.devices.Find(d => d.id == "PC4").kind = DeviceKind.DiagnosticStation;
        concurrent.cables.Find(c => c.id == "C1").intact = false;
        concurrent.ports.Find(p => p.id == "SW/P2").enabled = false;
        concurrent.ports.Find(p => p.id == "PC3/eth").ipv4 = "192.168.10.12";
        concurrent.allowedDuplicatePortIds.AddRange(new[] { "PC2/eth", "PC3/eth" });
        var live = new NetworkSession(concurrent);
        var simulator = new NetworkSimulationService(live);
        rules.probeSource = "PC4/eth";
        var incident = new CableRepairService(live, simulator, rules);
        live.ConnectCable("C1", "", "");
        incident.TestCable("C1", out _);
        incident.TestCable("spare", out _);
        live.ConnectCable("spare", "SW/P1", "PC1/eth");
        incident.LabelEnd("spare", true, "SW", out _);
        incident.LabelEnd("spare", false, "PC1", out _);
        incident.ObserveProbe(simulator.Ping("PC1/eth", "192.168.10.11"));
        check(incident.Progress == CableRepairProgress.VerifyFromLaptop && !incident.IsComplete,
            "Ping a sí misma no prueba reparación del enlace");
        incident.ObserveProbe(simulator.Ping("PC4/eth", "192.168.10.11"));
        check(incident.IsComplete, "Cable se acredita aunque persistan puerto apagado e IP duplicada");
        long before = live.Revision;
        incident.LabelEnd("spare", false, "PC1", out _);
        check(live.Revision == before && incident.IsComplete, "Reaplicar la misma etiqueta conserva evidencia vigente");
        var module = new GameData.Modules.ModuleDefinition();
        for (int i = 0; i < 4; i++) module.Objectives.Add(new GameData.Objectives.ObjectiveData());
        var flow = new Modules.Module03_Diagnostics.Flow.Module03FlowController(module, i => i == 0 && incident.IsComplete);
        flow.Begin(); flow.Evaluate();
        check(flow.Index == 1, "Reparación verificada hace avanzar al objetivo del puerto");

        live.ConnectCable("spare", "", "");
        check(!incident.ActivateLabel("spare", true, out _), "Etiqueta automática requiere conexión del reemplazo");
        live.ConnectCable("spare", "PC1/eth", "SW/P1");
        check(incident.ActivateLabel("spare", true, out _) && incident.ActivateLabel("spare", false, out _), "Activar ambos extremos invertidos");
        var labeled = live.Snapshot().cables.Find(c => c.id == "spare");
        check(labeled.labelA == "PC1" && labeled.labelB == "SW", "Texto automático corresponde al puerto real");
        incident.ObserveProbe(simulator.Ping("PC4/eth", "192.168.10.11"));
        before = live.Revision;
        incident.ActivateLabel("spare", true, out _);
        check(incident.IsComplete && live.Revision == before, "Activar de nuevo no duplica ni invalida evidencia");
        live.Reset();
        check(string.IsNullOrEmpty(live.Snapshot().cables.Find(c => c.id == "spare").labelA), "Reset deja etiquetas del repuesto ocultas");
    }
}
