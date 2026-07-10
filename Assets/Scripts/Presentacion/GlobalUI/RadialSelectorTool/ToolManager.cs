using UnityEngine;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    public class ToolManager : MonoBehaviour
    {
        public Transform rightHandAnchor;
        private GameObject currentTool;

        public void EquipTool(ToolData tool)
        {
            if (currentTool != null)
                Destroy(currentTool);

            if (tool == null || tool.prefab == null || rightHandAnchor == null)
                return;

            currentTool = Instantiate(tool.prefab, rightHandAnchor);
            currentTool.transform.localPosition = Vector3.zero;
            currentTool.transform.localRotation = Quaternion.identity;
        }
    }
}
