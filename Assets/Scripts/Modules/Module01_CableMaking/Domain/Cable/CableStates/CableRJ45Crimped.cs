using Framework.Interaction.Tools.Interfaces;
using Modules.Module01_CableMaking.Flow.Validation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable.CableStates
{
    public class CableRJ45Crimped : MonoBehaviour, ITestable
    {
        [SerializeField] private CableStateController stateController;

        private void Awake()
        {
            stateController ??= CableStateController.ResolveFor(this);
        }
        public bool CanTest => stateController != null && stateController.CanTest;

        public void Test()
        {
            ModuleActionValidator validator =
                SimulationManager.Instance?.FlowController?.ActionValidator;

            if (validator != null &&
                !validator.TryValidate(ModuleActionType.ValidateCable))
            {
                return;
            }

            if (!CanTest)
                return;
            CableEvents.RaiseCableValidated();
        }
    }
}
