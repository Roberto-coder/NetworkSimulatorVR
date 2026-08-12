using Core.Objectives;
using GameData.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// ModuleFlowController lo completa al recibir BothEndsConnected del tester.
    /// </summary>
    public sealed class ConnectTesterObjective : ObjectiveBase
    {
        public ConnectTesterObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
