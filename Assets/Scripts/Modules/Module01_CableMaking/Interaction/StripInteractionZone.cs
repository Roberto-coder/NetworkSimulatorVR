using Core.Domain;
using Framework.Interaction;
using SFX;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    public class StripInteractionZone : InteractionZone
    {
        [Header("Strip specific")]
        [SerializeField] private GameObject visualWhole;
        [SerializeField] private GameObject visualPeeled;

        protected override void ExecuteInteraction()
        {
            if (visualWhole && visualWhole.TryGetComponent(out TemporaryDebris debris))
                debris.Release();

            if (visualPeeled)
                visualPeeled.SetActive(true);

            CableEvents.RaiseCableStripped();
        }
    }
}