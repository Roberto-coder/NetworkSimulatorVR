using System;
using Core.Objectives;
using GameData.Objectives;
namespace Modules.Module03_Diagnostics.Factories
{
    public sealed class Module03ObjectiveFactory
    {
        // Incluso una llamada directa a Complete debe pasar la validación del estado real.
        private sealed class GuardedObjective : ObjectiveBase
        {
            private readonly Func<bool> condition;
            public GuardedObjective(ObjectiveData data, Func<bool> condition) : base(data) { this.condition = condition; }
            public override void Complete() { if (condition()) base.Complete(); }
        }
        public ObjectiveBase Create(ObjectiveData data, Func<bool> condition) => new GuardedObjective(data, condition);
    }
}
