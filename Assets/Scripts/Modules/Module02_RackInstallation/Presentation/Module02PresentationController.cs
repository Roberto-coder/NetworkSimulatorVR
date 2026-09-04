using Framework.Interaction.Tools;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Presentation
{
    public sealed class Module02PresentationController : MonoBehaviour
    {
        [SerializeField] private ToolManager toolManager;

        private void Start()
        {
            if (toolManager == null)
                toolManager = FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);
            if (toolManager != null && Module02Manager.Instance?.FlowController != null)
                toolManager.SetAvailableTools(Module02Manager.Instance.FlowController.AvailableTools);
        }
    }
}
