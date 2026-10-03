using System.Collections;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Presentation;
using Modules.Module03_Diagnostics.Presentation.Tutorial;
using Presentacion.Tutorial;

// Adaptadores mínimos de Unity: las pruebas ejecutan el Builder y TutorialSequence reales.
namespace Waypoints { public class Waypoint { public string name; public bool reached = true; } }
namespace Presentacion.Tutorial
{
    public class TutorialDirector { }
    public class MoveNpcStep : TutorialStep
    {
        private readonly Waypoints.Waypoint point;
        public MoveNpcStep(Waypoints.Waypoint point) => this.point = point;
        public override IEnumerator Execute(TutorialDirector director)
        { while (!point.reached) yield return null; }
    }
}
namespace Modules.Module03_Diagnostics.Presentation
{
    public class BuilderFlowProbe
    {
        public int StageIndex;
        public bool PhysicalRepairReady = true, verified;
        public bool CanFinishPractice() => verified;
    }
    public class Module03GuidancePresenter
    {
        public BuilderFlowProbe flow = new();
        public Waypoints.Waypoint initialWaypoint = new() { name = "Inicio" }, laptopEntryWaypoint = new() { name = "Entrada" },
            laptopWaypoint = new() { name = "Laptop" }, cableWaypoint = new() { name = "PC01" }, quizWaypoint = new() { name = "Quiz", reached = false };
        public List<string> spoken = new();
        public List<DiagnosticStage> unlocked = new();
        public string status;
        public void SetTutorialStatus(string value) => status = value;
        public void Unlock(DiagnosticStage value) => unlocked.Add(value);
        public IEnumerator Say(string id, Func<bool> completed) { spoken.Add(id); yield break; }
        public IEnumerator WaitFor(Func<bool> condition, string reminder) { while (!condition()) yield return null; }
    }
}
static class TutorialBuilderChecks
{
    public static void Run(Action<bool, string> check)
    {
        var host = new Module03GuidancePresenter();
        var sequence = new Module03TutorialBuilder().Build(host);
        check(sequence.Count > 0 && host.spoken.Count == 0 && host.unlocked.Count == 0, "Construir no ejecuta ni completa el tutorial");
        var frames = Execute(sequence);
        for (int i = 0; i < 100; i++) frames.MoveNext();
        check(host.status.EndsWith("M3R01") && !host.spoken.Contains("M3D05"), "Builder espera apertura real antes de explicar interfaz");
        host.flow.StageIndex = 6;
        for (int i = 0; i < 100; i++) frames.MoveNext();
        check(host.status.EndsWith("M3R07") && !host.spoken.Contains("M3D26"), "Builder espera verificación final antes del cierre");
        host.flow.verified = true;
        for (int i = 0; i < 100; i++) frames.MoveNext();
        check(host.status == "Recorrido: Quiz", "Tutorial espera llegada al quiz después del cierre");
        host.quizWaypoint.reached = true;
        check(!frames.MoveNext(), "Terminar recorrido termina secuencia que ejecuta el director");
        check(host.spoken.SequenceEqual(Enumerable.Range(1, 26).Select(i => "M3D" + i.ToString("00"))), "Conserva orden e IDs de los 26 diálogos");
        check(host.unlocked.SequenceEqual(Enum.GetValues<DiagnosticStage>()), "Habilita las seis etapas en orden");
    }
    private static IEnumerator Execute(TutorialSequence sequence)
    {
        foreach (var step in sequence.Steps)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(step.Execute(new TutorialDirector()));
            while (stack.Count > 0)
            {
                var current = stack.Peek();
                if (!current.MoveNext()) { stack.Pop(); continue; }
                if (current.Current is IEnumerator nested) stack.Push(nested);
                else yield return null;
            }
        }
    }
}
