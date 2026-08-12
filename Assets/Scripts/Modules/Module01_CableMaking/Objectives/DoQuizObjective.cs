using Core.Objectives;
using GameData.Objectives;
using Modules.Module01_CableMaking.Domain.Cable;
using UnityEngine;

namespace Modules.Module01_CableMaking.Objectives
{
    public sealed class DoQuizObjective: ObjectiveBase
    {
        public DoQuizObjective(ObjectiveData data)
            : base(data)
        {
            
        }
        
        public override void Begin()
        {
            base.Begin();

            CableEvents.CableValidated += CableValidated;
        }

        private void CableValidated()
        {
            CableEvents.CableValidated -= CableValidated;
            Debug.Log("Objetivo completado :)");
            Complete();
        }
        
    }
}
