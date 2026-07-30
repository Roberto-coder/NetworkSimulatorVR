using UnityEngine;

namespace Framework.Interaction.Tools
{
    /// <summary>
    /// Identifica el tipo de herramienta que representa este prefab.
    /// Es utilizada por las InteractionZone para validar si la herramienta
    /// puede ejecutar una determinada interacción.
    /// </summary>
    public class Tool : MonoBehaviour
    {
        [SerializeField]
        private ToolType type;

        /// <summary>
        /// Tipo de herramienta.
        /// </summary>
        public ToolType Type => type;
    }
}