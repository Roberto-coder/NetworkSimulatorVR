using Framework.Interaction.Tools.Interfaces;
using Modules.Module01_CableMaking.Presentation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class CableWhole : MonoBehaviour, IPeelable
    {
        private CableStateController stateController;

        private void Awake()
        {
            stateController = GetComponent<CableStateController>();
        }
        public bool CanPeel => stateController.CanPeel;

        public void Peel()
        {
            Debug.Log($"Peeling {stateController.CurrentState}");
            Debug.Log($"Estado: {stateController.CurrentState}");
            if (!CanPeel)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(CableState.Peeled))
                return;

            CableEvents.RaiseCableStripped();

            //CableEvents.RaiseCableStripped();
        }
    }
}