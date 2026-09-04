using Core.Objectives;
using GameData.Objectives;

namespace Modules.Module02_RackInstallation.Objectives
{
    /// <summary>
    /// Objetivo configurable del módulo 2. Las mecánicas concretas lo completan
    /// mediante Module02FlowController cuando estén implementadas.
    /// </summary>
    public sealed class Module02Objective : ObjectiveBase
    {
        public Module02Objective(ObjectiveData data) : base(data) { }
    }
}
