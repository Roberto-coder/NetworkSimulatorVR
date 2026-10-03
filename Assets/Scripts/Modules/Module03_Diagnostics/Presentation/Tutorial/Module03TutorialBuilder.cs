using System;
using System.Collections;
using Modules.Module03_Diagnostics.Domain;
using Presentacion.Tutorial;
using Waypoints;

namespace Modules.Module03_Diagnostics.Presentation.Tutorial
{
    /// <summary>Construye el orden del módulo. El director ejecuta los pasos y emite TutorialCompleted.</summary>
    public sealed class Module03TutorialBuilder
    {
        public TutorialSequence Build(Module03GuidancePresenter host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var flow = host.flow;
            var sequence = new TutorialSequence();
            void Add(string label, Func<TutorialDirector, IEnumerator> execute)
                => sequence.AddStep(new NamedStep(host, label, execute));
            void Move(Waypoint point)
                => Add("Recorrido: " + point.name, new MoveNpcStep(point).Execute);
            void Say(string id, Func<bool> completed = null)
                => Add("Diálogo: " + id, _ => host.Say(id, completed));
            void WaitFor(Func<bool> condition, string reminder)
                => Add("Espera de evidencia: " + reminder, _ => host.WaitFor(condition, reminder));
            void Unlock(DiagnosticStage stage)
                => Add("Habilitar: " + stage, _ => UnlockStep(host, stage));

            Move(host.initialWaypoint);
            Say("M3D01"); Say("M3D02");
            Move(host.laptopEntryWaypoint); Move(host.laptopWaypoint);
            Say("M3D03");
            Unlock(DiagnosticStage.OpenLaptop);
            Say("M3D04", () => flow.StageIndex > 0);
            WaitFor(() => flow.StageIndex > 0, "M3R01");
            Say("M3D05"); Say("M3D06");
            Say("M3D07"); Say("M3D08");
            Say("M3D09"); Say("M3D10"); Say("M3D11");
            Unlock(DiagnosticStage.FirstPing);
            Say("M3D12", () => flow.StageIndex > 1);
            WaitFor(() => flow.StageIndex > 1, "M3R02");
            Say("M3D13"); Say("M3D14");
            Move(host.laptopEntryWaypoint); Move(host.cableWaypoint);
            Say("M3D15"); Say("M3D16");
            Unlock(DiagnosticStage.Cable); // El presenter habilita conectores y muestra el repuesto.
            Say("M3D17", () => flow.PhysicalRepairReady);
            WaitFor(() => flow.PhysicalRepairReady, "M3R03");
            Say("M3D18");
            Move(host.laptopEntryWaypoint); Move(host.laptopWaypoint);
            Say("M3D19", () => flow.StageIndex > 2);
            WaitFor(() => flow.StageIndex > 2, "M3R04");
            Say("M3D20"); Say("M3D21");
            Unlock(DiagnosticStage.Ports);
            Say("M3D22", () => flow.StageIndex > 3);
            WaitFor(() => flow.StageIndex > 3, "M3R05");
            Say("M3D23");
            Unlock(DiagnosticStage.Addresses);
            Say("M3D24", () => flow.StageIndex > 4);
            WaitFor(() => flow.StageIndex > 4, "M3R06");
            Unlock(DiagnosticStage.FinalVerification);
            Say("M3D25", flow.CanFinishPractice);
            WaitFor(flow.CanFinishPractice, "M3R07");
            Say("M3D26");
            Move(host.laptopEntryWaypoint); Move(host.quizWaypoint);
            return sequence;
        }
        private static IEnumerator UnlockStep(Module03GuidancePresenter host, DiagnosticStage stage)
        {
            host.Unlock(stage);
            yield break;
        }
        // Adaptador de las esperas específicas al mismo contrato TutorialStep de los otros módulos.
        private sealed class NamedStep : TutorialStep
        {
            private readonly Module03GuidancePresenter host;
            private readonly string label;
            private readonly Func<TutorialDirector, IEnumerator> execute;
            public NamedStep(Module03GuidancePresenter host, string label, Func<TutorialDirector, IEnumerator> execute)
            { this.host = host; this.label = label; this.execute = execute; }
            public override IEnumerator Execute(TutorialDirector director)
            {
                host.SetTutorialStatus(label);
                yield return execute(director);
            }
        }
    }
}
