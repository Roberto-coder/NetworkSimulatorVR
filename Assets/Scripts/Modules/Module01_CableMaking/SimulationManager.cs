using GameData.Modules;
using Modules.Module01_CableMaking.Flow;
using UnityEngine;

namespace Modules.Module01_CableMaking
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [Header("Module")]

        [SerializeField]
        private ModuleDefinition moduleDefinition;

        public ModuleFlowController FlowController { get; private set; }

        private void Awake()
        {
            Instance = this;

            FlowController = new ModuleFlowController(moduleDefinition);
        }

        private void Start()
        {
            FlowController.Begin();
        }
    }
}