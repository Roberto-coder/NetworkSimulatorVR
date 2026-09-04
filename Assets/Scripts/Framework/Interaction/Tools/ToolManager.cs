using System.Collections.Generic;
using Modules.Module01_CableMaking;
using Oculus.Interaction.Input.Visuals;
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

        [Header("Controller visuals")]
        [SerializeField]
        [Tooltip("Visual del control derecho de Meta. Si no se asigna, se busca automaticamente en PlayerRoot.")]
        private ControllerVisual rightControllerVisual;

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

        private void Awake()
        {
            ResolveRightControllerVisual();
        }

        private void OnDisable()
        {
            SetRightControllerVisible(true);
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
            SetRightControllerVisible(false);
        }

        public void UnequipTool()
        {
            if (currentTool != null)
                Destroy(currentTool);

            currentTool = null;
            currentToolData = null;
            SetRightControllerVisible(true);
        }

        private void ResolveRightControllerVisual()
        {
            if (rightControllerVisual != null)
                return;

            Transform searchRoot = transform.root;
            ControllerVisual[] controllerVisuals =
                searchRoot.GetComponentsInChildren<ControllerVisual>(true);

            foreach (ControllerVisual candidate in controllerVisuals)
            {
                if (candidate.name == "OVRRightControllerVisual")
                {
                    rightControllerVisual = candidate;
                    return;
                }
            }

            UnityEngine.Debug.LogWarning(
                "[ToolManager] No se encontro OVRRightControllerVisual. " +
                "Las herramientas se equiparan, pero el modelo del control no se ocultara.",
                this);
        }

        private void SetRightControllerVisible(bool visible)
        {
            if (rightControllerVisual == null)
                ResolveRightControllerVisual();

            if (rightControllerVisual != null)
                rightControllerVisual.ForceOffVisibility = !visible;
        }
    }
}
