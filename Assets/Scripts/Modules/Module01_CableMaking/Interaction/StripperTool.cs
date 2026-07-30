using Framework.Interaction.Tools.Interfaces;
using Framework.Interaction.Tools;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    [RequireComponent(typeof(ToolInteractor))]
    public class StripperTool : InteractionTool<IPeelable>
    {
        private ToolInteractor interactor;

        private void Awake()
        {
            interactor = GetComponent<ToolInteractor>();
        }

        private void OnEnable()
        {
            interactor.InteractPressed += TryPeel;
        }

        private void OnDisable()
        {
            if (interactor != null)
                interactor.InteractPressed -= TryPeel;
        }

        private void TryPeel()
        {
            if (!TryGetTarget(out IPeelable peelable))
                return;

            peelable.Peel();
        }


    }
}