using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de pelar el extremo derecho del cable.
    /// </summary>
    public sealed class StripRightEndObjective : ObjectiveBase
    {
        public StripRightEndObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
