using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de conectar el cable al tester.
    /// </summary>
    public sealed class ConnectToTesterObjective : ObjectiveBase
    {
        public ConnectToTesterObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
