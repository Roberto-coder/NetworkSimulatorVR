using Framework.Interaction.Tools.Interfaces;
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
            if (!CanCrimp)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(end, CableState.Rj45Crimped))
                return;

            CableEvents.RaiseCableCrimped(end);
        }
    }
}
