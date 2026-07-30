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

        private GameObject currentTool;

        private ToolData currentToolData;

        public ToolData CurrentTool => currentToolData;

        public IReadOnlyList<ToolData> AvailableTools =>
            SimulationManager.Instance.FlowController.AvailableTools;

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