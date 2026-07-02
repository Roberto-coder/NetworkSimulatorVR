using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de ponchar el extremo izquierdo del cable.
    /// </summary>
    public sealed class CrimpLeftEndObjective : ObjectiveBase
    {
        public CrimpLeftEndObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
