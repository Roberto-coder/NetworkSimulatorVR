using Framework.Interaction.Tools.Interfaces;
using Framework.Interaction.Tools;
using Modules.Module01_CableMaking.Flow.Validation;
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
            ModuleActionValidator validator =
                SimulationManager.Instance?.FlowController?.ActionValidator;

            if (validator != null &&
                !validator.TryValidate(ModuleActionType.Peel))
            {
                return;
            }

            if (!TryGetTarget(out IPeelable peelable))
            {
                Debug.Log("[StripperTool] No se encontró un objetivo IPeelable bajo el puntero.", this);
                return;
            }

            Debug.Log($"[StripperTool] Intentando pelar: {((Component)peelable).name}", this);

            peelable.Peel();
        }


    }
}
