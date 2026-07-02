using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de ordenar los conductores del extremo izquierdo segun T568B.
    /// </summary>
    public sealed class OrderLeftT568BObjective : ObjectiveBase
    {
        public OrderLeftT568BObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
