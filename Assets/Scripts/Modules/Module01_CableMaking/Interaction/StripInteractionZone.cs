using Core.Domain;
using Framework.Interaction;
using SFX;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    public class StripInteractionZone : InteractionZone
    {
        protected override void ExecuteInteraction()
        {
            CableEvents.RaiseCableStripped();
        }
    }
}