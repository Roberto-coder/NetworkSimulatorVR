using UnityEngine;

namespace Framework.Interaction.Tools
{
    /// <summary>
    /// Componente que identifica el tipo de una herramienta dentro del simulador.
    /// Se coloca en el prefab de la herramienta instanciada en la mano del jugador.
    /// </summary>
    public class Tool : MonoBehaviour
    {
        [SerializeField]
        private ToolType type;

        /// <summary>
        /// Tipo de herramienta que representa este componente.
        /// </summary>
        public ToolType Type => type;
    }
}