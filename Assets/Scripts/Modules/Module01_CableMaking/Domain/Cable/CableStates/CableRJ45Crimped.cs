using Framework.Interaction.Tools.Interfaces;
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
            if (!CanTest)
                return;
            CableEvents.RaiseCableValidated();
        }
    }
}
