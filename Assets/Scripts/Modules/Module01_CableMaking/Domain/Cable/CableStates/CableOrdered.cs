using Framework.Interaction.Tools.Interfaces;
using Modules.Module01_CableMaking.Flow.Validation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable.CableStates
{
    public class CableOrdered: MonoBehaviour, ICrimpable
    {
        [SerializeField] private CableEnd end;

        [SerializeField] private CableStateController stateController;
        private void Awake()
        {
            stateController ??= CableStateController.ResolveFor(this);
        }
        public bool CanCrimp =>
            stateController != null && stateController.CanCrimp(end);
        
        public void Crimp()
        {
            ModuleActionValidator validator =
                SimulationManager.Instance?.FlowController?.ActionValidator;

            if (validator != null &&
                !validator.TryValidate(ModuleActionType.Crimp, end))
            {
                return;
            }

            if (!CanCrimp)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(end, CableState.Rj45Crimped))
                return;

            CableEvents.RaiseCableCrimped(end);
        }
    }
}
