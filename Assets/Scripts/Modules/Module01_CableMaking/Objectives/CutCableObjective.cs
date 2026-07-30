using Core.Objectives;
using GameData.Objectives;

namespace Modules.Module01_CableMaking.Objectives
{
    /// <summary>
    /// Representa el objetivo de cortar el cable de red.
    /// </summary>
    public sealed class CutCableObjective : ObjectiveBase
    {
        public CutCableObjective(ObjectiveData data)
            : base(data)
        {
            UnityEngine.Debug.Log("Objetivo: Cortar cable");
        }
    }
}
