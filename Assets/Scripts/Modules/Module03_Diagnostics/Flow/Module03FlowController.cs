using System;
using System.Collections.Generic;
using Core.Objectives;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module03_Diagnostics.Factories;
namespace Modules.Module03_Diagnostics.Flow
{
    public sealed class Module03FlowController : IObjectiveFlow
    {
        private readonly ObjectiveController controller;
        private bool started;
        public event Action<ObjectiveData> CurrentObjectiveChanged;
        public event Action<ObjectiveData> ObjectiveCompleted;
        public event Action PracticalObjectivesCompleted;
        public IReadOnlyList<ObjectiveBase> Objectives => controller.Objectives;
        public ObjectiveData CurrentObjectiveData => controller.CurrentObjective?.Data;
        public int Index => controller.CurrentObjectiveIndex;
        public bool PracticeCompleted => controller.HasCompletedAllObjectives;
        public Module03FlowController(ModuleDefinition module, Func<int, bool> canComplete)
        {
            if (module == null || (module.Objectives.Count != 4 && module.Objectives.Count != 6))
                throw new ArgumentException("M3 necesita seis objetivos (o cuatro para escenas anteriores).");
            var factory = new Module03ObjectiveFactory();
            var objectives = new List<ObjectiveBase>();
            for (int i = 0; i < module.Objectives.Count; i++)
            { int index = i; objectives.Add(factory.Create(module.Objectives[i], () => canComplete(index))); }
            controller = new ObjectiveController(objectives);
            controller.CurrentObjectiveChanged += o =>
            {
                // La muñeca recibe el nuevo objetivo ya en Running, no todavía en Waiting.
                o?.Begin();
                CurrentObjectiveChanged?.Invoke(o?.Data);
            };
            controller.AllObjectivesCompleted += () => PracticalObjectivesCompleted?.Invoke();
        }
        public void Begin()
        {
            if (started) return;
            started = true;
            controller.Begin();
        }
        public void Evaluate()
        {
            // Solo un avance por evaluación; conservar la secuencia aunque se reparó algo antes.
            if (!started) return;
            var current = controller.CurrentObjective;
            if (current == null) return;
            current.Complete();
            if (current.IsCompleted) ObjectiveCompleted?.Invoke(current.Data);
        }
        public void Reset() { controller.Reset(); started = false; Begin(); }
    }
}
