using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de ordenar los conductores del extremo derecho segun T568B.
    /// </summary>
    public sealed class OrderRightT568BObjective : ObjectiveBase
    {
        public OrderRightT568BObjective(ObjectiveData data)
            : base(data)
        {
        }
    }
}
