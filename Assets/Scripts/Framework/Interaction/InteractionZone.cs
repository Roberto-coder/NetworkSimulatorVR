using Framework.Interaction.Tools;
using UnityEngine;
namespace Framework.Interaction
{
    /// <summary>
    /// Clase base abstracta para cualquier zona de interacción del simulador
    /// (cortar, pelar, ponchar, insertar en tester, etc.).
    /// Detecta la presencia de una herramienta válida mediante triggers y expone
    /// un método público para ejecutar la interacción cuando el jugador lo solicite.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class InteractionZone : MonoBehaviour
    {
        [SerializeField]
        private ToolType requiredTool;

        /// <summary>
        /// Herramienta actualmente presente dentro de la zona (null si ninguna).
        /// </summary>
        protected Tool currentTool;

        /// <summary>
        /// Indica si existe una herramienta válida dentro de la zona.
        /// </summary>
        protected bool HasValidTool => currentTool != null && currentTool.Type == requiredTool;

        private void OnTriggerEnter(Collider other)
        {
            Tool tool = other.GetComponentInParent<Tool>();
            if (tool == null)
            {
                return;
            }

            currentTool = tool;
        }

        private void OnTriggerExit(Collider other)
        {
            Tool tool = other.GetComponentInParent<Tool>();
            if (tool == null || tool != currentTool)
            {
                return;
            }

            currentTool = null;
        }

        /// <summary>
        /// Punto de entrada llamado desde el sistema de entrada del jugador.
        /// Ejecuta la interacción únicamente si la herramienta requerida
        /// está presente dentro de la zona.
        /// </summary>
        public void Interact()
        {
            if (!HasValidTool)
            {
                return;
            }

            ExecuteInteraction();
        }

        /// <summary>
        /// Define la acción específica que ocurre al completar la interacción.
        /// Implementada por cada zona derivada.
        /// </summary>
        protected abstract void ExecuteInteraction();
    }
}