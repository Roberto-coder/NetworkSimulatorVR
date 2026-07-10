using Core.Domain;
using Core.Objectives;
using GameData.Objectives;
using NetworkVR.Core.Objectives;
using UnityEngine;

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
        
        public override void Begin()
        {
            base.Begin();

            CableEvents.CableStripped += OnCableStripped;
        }

        private void OnCableStripped()
        {
            CableEvents.CableStripped -= OnCableStripped;
            Debug.Log("Objetivo completado :)");
            base.Complete();
            // Complete();
        }
        
    }
    
}
