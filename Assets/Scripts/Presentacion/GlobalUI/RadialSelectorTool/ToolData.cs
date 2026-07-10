using UnityEngine;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    [System.Serializable]
    public class ToolData
    {
        public string name;
        public GameObject prefab; // null = mano vacía
        public Sprite icon;
    }
}