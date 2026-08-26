using System.Collections.Generic;
using Modules.Module01_CableMaking;
using UnityEngine;

namespace Framework.Interaction.Tools
{
    /// <summary>
    /// Instancia el prefab de la tool seleccionada en tu mano
    /// y gestiona que solo tengas una a la vez
    /// </summary>
    public class ToolManager : MonoBehaviour
    {
        [SerializeField]
        private Transform rightHandAnchor;

        [SerializeField]
        private Vector3 toolPositionOffset;

        [SerializeField]
        private Vector3 toolRotationOffset;
        
        private GameObject currentTool;

        private ToolData currentToolData;

        [Header("Module-independent tools")]
        [SerializeField] private List<ToolData> availableToolsOverride = new();

        public ToolData CurrentTool => currentToolData;

        public IReadOnlyList<ToolData> AvailableTools
        {
            get
            {
                if (availableToolsOverride != null && availableToolsOverride.Count > 0)
                    return availableToolsOverride;

                return SimulationManager.Instance != null
                    ? SimulationManager.Instance.FlowController.AvailableTools
                    : System.Array.Empty<ToolData>();
            }
        }

        public void SetAvailableTools(IEnumerable<ToolData> tools)
        {
            availableToolsOverride = tools != null ? new List<ToolData>(tools) : new List<ToolData>();
        }

        public void EquipTool(ToolData tool)
        {
            UnequipTool();

            if (tool == null)
                return;

            if (tool.prefab == null)
                return;

            currentTool = Instantiate(tool.prefab, rightHandAnchor);

            currentTool.transform.localPosition = Vector3.zero;
            currentTool.transform.localRotation = Quaternion.identity;
            // currentTool.transform.localPosition = toolPositionOffset;
            // currentTool.transform.localRotation = Quaternion.Euler(toolRotationOffset);

            currentToolData = tool;
        }

        public void UnequipTool()
        {
            if (currentTool != null)
                Destroy(currentTool);

            currentTool = null;
            currentToolData = null;
        }
    }
}
