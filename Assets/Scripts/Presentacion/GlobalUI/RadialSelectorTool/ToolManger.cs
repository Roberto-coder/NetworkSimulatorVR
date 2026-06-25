using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public Transform rightHandAnchor;
    private GameObject currentTool;

    public void EquipTool(ToolData tool)
    {
        // Eliminar herramienta actual
        if (currentTool != null)
            Destroy(currentTool);

        // Si no hay prefab → mano vacía
        if (tool == null || tool.prefab == null)
            return;

        // Instanciar nueva herramienta
        currentTool = Instantiate(tool.prefab, rightHandAnchor);
        currentTool.transform.localPosition = Vector3.zero;
        currentTool.transform.localRotation = Quaternion.identity;
    }
}