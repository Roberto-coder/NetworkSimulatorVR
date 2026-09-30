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
    }
}
