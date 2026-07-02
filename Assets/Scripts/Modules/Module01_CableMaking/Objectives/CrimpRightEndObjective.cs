using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de ponchar el extremo derecho del cable.
    /// </summary>
    public sealed class CrimpRightEndObjective : ObjectiveBase
    {
        public CrimpRightEndObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
