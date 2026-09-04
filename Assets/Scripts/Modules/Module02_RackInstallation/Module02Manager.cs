using GameData.Modules;
using Modules.Module02_RackInstallation.Flow;
using UnityEngine;

namespace Modules.Module02_RackInstallation
{
    public sealed class Module02Manager : MonoBehaviour
    {
        public static Module02Manager Instance { get; private set; }
        [SerializeField] private ModuleDefinition moduleDefinition;
        public Module02FlowController FlowController { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (moduleDefinition == null)
            {
                Debug.LogError("Module02Manager necesita una ModuleDefinition.", this);
                enabled = false;
                return;
            }
            FlowController = new Module02FlowController(moduleDefinition);
        }

        private void Start() => FlowController?.Begin();

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
