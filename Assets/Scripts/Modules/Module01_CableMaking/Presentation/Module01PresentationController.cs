using Core.Domain;
using SFX;
using UnityEngine;

namespace Modules.Module01_CableMaking.Presentation
{
    /// <summary>
    /// Controla únicamente la representación visual del módulo.
    ///
    /// Responsabilidades:
    /// - Mostrar u ocultar objetos.
    /// - Reproducir animaciones.
    /// - Activar puzzles.
    /// - Mostrar estaciones de trabajo.
    /// - Gestionar efectos visuales.
    ///
    /// NO debe:
    /// - Validar objetivos.
    /// - Conocer ObjectiveController.
    /// - Conocer ModuleFlowController.
    /// - Contener lógica de interacción.
    /// </summary>
    public class Module01PresentationController : MonoBehaviour
    {
        // Referencias a objetos visuales
        
        // Cable completo
        [SerializeField] GameObject cableWhole;
        // Cable pelado
        [SerializeField] GameObject cablePeeled;
        // Wire Puzzle
        [SerializeField] GameObject wirePuzzle;
        // Tester
        
        // Restos
        [SerializeField] TemporaryDebris debris;
        
        private void OnEnable()
        {
            // Suscribirse a eventos
            CableEvents.CableStripped += HandleCableStripped;
        }

        private void OnDisable()
        {
            // Desuscribirse
            CableEvents.CableStripped -= HandleCableStripped;
        }

        // Cable pelado
        // Cambiar representación visual
        // Mostrar siguiente estación
        private void HandleCableStripped()
        {
            if (cableWhole && cableWhole.TryGetComponent(out TemporaryDebris debris))
                debris.Release();

            if (cablePeeled)
                cablePeeled.SetActive(true);
        }

        // Orden correcto
        // Ocultar puzzle
        // Mostrar cable con RJ45
        private void HandleWireOrderCompleted()
        {

        }

        // Ponchado
        private void HandleCableCrimped()
        {

        }

        // Tester
        private void HandleCableValidated()
        {

        }
    }
}