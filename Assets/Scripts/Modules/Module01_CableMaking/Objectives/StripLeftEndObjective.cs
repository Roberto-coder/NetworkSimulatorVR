using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de pelar el extremo izquierdo del cable.
    /// </summary>
    public sealed class StripLeftEndObjective : ObjectiveBase
    {
        public StripLeftEndObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
