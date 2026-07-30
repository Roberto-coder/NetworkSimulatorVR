using UnityEngine;

namespace Framework.Interaction.Tools
{
    [System.Serializable]
    public class ToolData
    {
        public string name;
        public GameObject prefab; // null = mano vacía
        public Sprite icon;
    }
}