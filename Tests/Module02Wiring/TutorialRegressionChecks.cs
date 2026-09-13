using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Core.Objectives;
using GameData.Module02;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Presentation.Tutorial;
using Presentacion.Tutorial;

static class TutorialRegressionChecks
{
    sealed class Objective : ObjectiveBase { public Objective(ObjectiveData data) : base(data) { } }
    sealed class Flow : IObjectiveFlow
    {
        public IReadOnlyList<ObjectiveBase> Objectives { get; }
        public ObjectiveData CurrentObjectiveData => Objectives[0].Data;
        private Action<ObjectiveData> completed;
        public int Listeners;
        public event Action<ObjectiveData> ObjectiveCompleted
        { add { completed += value; Listeners++; } remove { completed -= value; Listeners--; } }
        public event Action<ObjectiveData> CurrentObjectiveChanged { add { } remove { } }
        public Flow(Objective objective) { Objectives = new[] { objective }; }
    }
    // Reproduce el anidamiento yield return IEnumerator utilizado por Unity.
    sealed class Runner : IDisposable
    {
        private readonly Stack<IEnumerator> stack = new();
        public Runner(IEnumerator routine) { stack.Push(routine); }
        public bool Tick()
        {
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                if (!top.MoveNext()) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                if (top.Current is IEnumerator child) { stack.Push(child); continue; }
                return true;
            }
            return false;
        }
        public void Dispose() { while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose(); }
    }
    public static int Run()
    {
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new Exception(message); }
        (Objective objective, Flow flow, TutorialDirector director, Runner runner) Scenario()
        {
            var data = new ObjectiveData();
            typeof(ObjectiveData).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(data, "inspect_switch");
            var objective = new Objective(data); objective.Begin();
            var flow = new Flow(objective); var director = new TutorialDirector { FlowController = flow };
            var step = new Module02GuidedTutorialStep(objective, new Module02TutorialObjective { explanation = "Explicación", instruction = "Instrucción" });
            return (objective, flow, director, new Runner(step.Execute(director)));
        }
        var skipped = Scenario(); skipped.objective.Complete();
        Check(!skipped.runner.Tick() && skipped.director.DialogueController.Shown.Count == 0, "Completado: omitir narración");
        var duringExplanation = Scenario();
        Check(duringExplanation.runner.Tick(), "Esperar explicación");
        duringExplanation.objective.Complete();
        Check(!duringExplanation.runner.Tick() && duringExplanation.director.DialogueController.Shown.Count == 1, "Completar durante explicación omite instrucción");
        var active = Scenario(); active.runner.Tick();
        active.director.DialogueController.Confirm = true; active.runner.Tick();
        Check(active.director.DialogueController.Shown.Count == 2 && active.flow.Listeners == 1, "Mostrar instrucción y suscribirse");
        active.director.DialogueController.Confirm = true;
        Check(active.runner.Tick() && !active.objective.IsCompleted, "B no completa objetivo");
        Check(active.runner.Tick(), "Seguir esperando progreso físico");
        active.objective.Complete();
        Check(!active.runner.Tick() && active.flow.Listeners == 0, "Completitud física avanza y libera suscripción");
        var cancelled = Scenario(); cancelled.runner.Tick();
        cancelled.director.DialogueController.Confirm = true; cancelled.runner.Tick(); cancelled.runner.Dispose();
        Check(cancelled.flow.Listeners == 0 && !cancelled.objective.IsCompleted, "Cancelar libera suscripción sin completar");
        return checks;
    }
}
