using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Flow;
using GameData.Modules;
using GameData.Objectives;

static class GuidedFlowChecks
{
    public static void Run(Func<int, NetworkDefinition> office, Action<bool, string> check)
    {
        var n = office(3);
        n.ports.Add(new PortDefinition { id = "SW/mgmt", deviceId = "SW", ipv4 = "192.168.10.2", mac = "02:00:00:00:01:02" });
        var session = new NetworkSession(n);
        var sim = new NetworkSimulationService(session);
        var evidence = new DiagnosticProgressEvidence(session, sim, n, "PC1/eth");
        check(!evidence.Verified(Array.Empty<string>()) && !evidence.Verified(new[] { "PC2" }), "Sin evidencia no hay aprobación vacía");
        evidence.Observe(sim.Ping("PC3/eth", "192.168.10.12"));
        check(!evidence.Verified(new[] { "PC2" }), "Origen incorrecto no acredita la prueba final");
        evidence.Observe(new NetworkSimulationService(new NetworkSession(n)).Ping("PC1/eth", "192.168.10.12"));
        check(!evidence.Verified(new[] { "PC2" }), "Resultado de otra sesión no acredita");
        evidence.Observe(sim.Ping("PC1/eth", "192.168.10.12"));
        evidence.Observe(sim.Ping("PC1/eth", "192.168.10.13"));
        check(evidence.Verified(new[] { "PC2", "PC3" }), "Pings válidos pueden coexistir");
        session.SetPortEnabled("SW/P2", false);
        check(!evidence.Verified(new[] { "PC2", "PC3" }), "Cambio posterior invalida ambas evidencias");
        var ui = new DiagnosticWorkspace(session, sim, n, "PC1/eth");
        check(!evidence.ConsultedSwitch("SW"), "Gestión no se presume consultada");
        ui.OpenLocal("PC1"); ui.SwitchPorts("SW");
        check(evidence.ConsultedSwitch("SW"), "Consulta alcanzable de switch queda registrada");
        session.Reset();
        check(!evidence.ConsultedSwitch("SW") && !evidence.Verified(new[] { "PC3" }), "Reset elimina validez de consultas y pings");
        session.SetAddress("PC2/eth", "192.168.10.20", 24);
        session.SetAddress("PC3/eth", "192.168.10.12", 24);
        evidence.Observe(sim.Ping("PC1/eth", "192.168.10.12"));
        check(!evidence.Verified(new[] { "PC2" }), "Respuesta de otro equipo no acredita reparación");

        var intro = new DiagnosticTutorialProgress();
        var first = sim.Ping("PC1/eth", "192.168.10.11");
        intro.Observe(first, session, "PC1/eth", "192.168.10.11");
        check(!intro.FirstPingExecuted, "Ping anterior a abrir laptop no acredita introducción");
        intro.Open("PC2", "PC1");
        check(!intro.LaptopOpened, "Abrir otra PC no acredita laptop");
        intro.Open("PC1", "PC1");
        intro.Observe(first, new NetworkSession(n), "PC1/eth", "192.168.10.11");
        check(!intro.FirstPingExecuted, "Primer ping exige sesión actual");
        intro.Observe(first, session, "PC3/eth", "192.168.10.11");
        intro.Observe(first, session, "PC1/eth", "192.168.10.12");
        check(!intro.FirstPingExecuted, "Primer ping exige origen y destino documentados");
        session.SetPortEnabled("SW/P2", false);
        intro.Observe(first, session, "PC1/eth", "192.168.10.11");
        check(!intro.FirstPingExecuted, "Primer ping rechaza revisión anterior");
        intro.Observe(sim.Ping("PC1/eth", "192.168.10.99"), session, "PC1/eth", "192.168.10.99");
        check(intro.FirstPingExecuted, "Diagnóstico inicial admite ping fallido");
        intro.Reset();
        check(!intro.LaptopOpened && !intro.FirstPingExecuted, "Reset elimina evidencia de introducción");
        for (int objective = 0; objective <= 6; objective++)
            for (int narrated = -1; narrated <= 5; narrated++)
                foreach (DiagnosticStage action in Enum.GetValues<DiagnosticStage>())
                {
                    check(DiagnosticTutorialProgress.Allows(action, objective, narrated, true) ==
                        (objective >= (int)action && narrated >= (int)action), "Acciones respetan objetivo y narración");
                    check(DiagnosticTutorialProgress.Allows(action, objective, narrated, false) ==
                        (objective >= (int)action), "Sin tutorial no depende de narración");
                }
        var extended = new ModuleDefinition();
        for (int i = 0; i < 6; i++) extended.Objectives.Add(new ObjectiveData());
        int approvedThrough = -1;
        var six = new Module03FlowController(extended, i => i <= approvedThrough);
        six.Begin();
        for (int i = 0; i < 6; i++)
        {
            six.Evaluate(); check(six.Index == i, "Cada objetivo espera su evidencia");
            approvedThrough = i; six.Evaluate(); check(six.Index == i + 1, "Seis objetivos avanzan de uno en uno");
        }
        check(six.PracticeCompleted, "Seis objetivos permiten cierre");
        approvedThrough = -1; six.Reset(); six.Evaluate();
        check(six.Index == 0 && !six.PracticeCompleted, "Reset regresa a abrir terminal");

        var module = new ModuleDefinition();
        for (int i = 0; i < 4; i++) module.Objectives.Add(new ObjectiveData());
        var valid = new bool[4];
        var flow = new Module03FlowController(module, i => valid[i]);
        int events = 0; flow.PracticalObjectivesCompleted += () => events++;
        bool runningWhenNotified = false;
        flow.CurrentObjectiveChanged += data => { if (data != null) runningWhenNotified = flow.Objectives[flow.Index].IsRunning; };
        flow.Begin();
        check(runningWhenNotified, "Muñeca recibe objetivo activo en Running");
        flow.Evaluate(); flow.Objectives[0].Complete();
        check(flow.Index == 0, "Ni evaluación ni Complete directo saltan condición pendiente");
        valid[1] = valid[2] = true; flow.Evaluate();
        check(flow.Index == 0, "Reparaciones anticipadas respetan etapa activa");
        valid[0] = true; flow.Evaluate(); flow.Evaluate(); flow.Evaluate();
        check(flow.Index == 3 && !flow.PracticeCompleted, "Puerto e IP avanzan sin aprobar cierre pendiente");
        flow.Begin();
        check(flow.Index == 3, "Begin repetido no reinicia progreso");
        valid[3] = true; flow.Evaluate(); flow.Evaluate();
        check(flow.PracticeCompleted && events == 1, "Cierre de práctica se emite una vez");
        Array.Fill(valid, false); flow.Reset();
        check(flow.Index == 0 && !flow.PracticeCompleted && !flow.Objectives.Any(o => o.IsCompleted), "Reinicio restaura los cuatro objetivos");
    }
}

