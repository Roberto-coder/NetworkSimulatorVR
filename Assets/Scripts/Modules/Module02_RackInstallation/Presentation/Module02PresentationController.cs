using Framework.Interaction.Tools;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Objectives;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Presentation
{
    public sealed class Module02PresentationController : MonoBehaviour
    {
        [SerializeField] private ToolManager toolManager;
        [SerializeField] private GameObject connectionTable;
        private Module02FlowController flow;

        private void Awake()
        {
            if (connectionTable != null)
                connectionTable.SetActive(false);
        }

        private void OnEnable() => BindFlow();

        private void OnDisable()
        {
            if (flow != null)
                flow.CurrentObjectiveChanged -= RefreshConnectionTable;
            flow = null;
            if (connectionTable != null)
                connectionTable.SetActive(false);
        }

        private void Start()
        {
            BindFlow();
            if (toolManager == null)
                toolManager = FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);
            if (toolManager != null && Module02Manager.Instance?.FlowController != null)
                toolManager.SetAvailableTools(Module02Manager.Instance.FlowController.AvailableTools);
        }

        private void BindFlow()
        {
            if (flow != null)
                return;
            flow = Module02Manager.Instance?.FlowController;
            if (flow == null)
                return;
            flow.CurrentObjectiveChanged += RefreshConnectionTable;
            RefreshConnectionTable(flow.CurrentObjectiveData);
        }

        private void RefreshConnectionTable(ObjectiveData objective)
        {
            if (connectionTable != null)
                connectionTable.SetActive(objective?.Id == Module02ObjectiveCatalog.ConnectLinks);
        }
    }
}
