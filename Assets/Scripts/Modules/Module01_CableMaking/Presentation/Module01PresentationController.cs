using Modules.Module01_CableMaking.Domain.Cable;
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
        [SerializeField] private CableStateController cableState;
        // Cable completo
        [SerializeField] GameObject cableWhole;
        // Cable pelado
        [SerializeField] GameObject cablePeeled;
        // Cable con entrada RJ45
        [SerializeField] private GameObject cableRJ45;
        // Cable con entrada RJ45 ponchada
        [SerializeField] private GameObject cableRJ45Crimped;
        // Wire Puzzle
        [SerializeField] GameObject wirePuzzle;
        //Referencia al spawner de escombros para el cable pelado
        [SerializeField] private DebrisSpawner debrisSpawner;
        
        private void Start()
        {
            ShowCable(CableState.Whole);
        }
        
        private void OnEnable()
        {
            cableState.StateChanged += HandleCableStateChanged;
        }

        private void OnDisable()
        {
            cableState.StateChanged -= HandleCableStateChanged;
        }

        private void ShowCable(CableState state)
        {
            cableWhole.SetActive(state == CableState.Whole);
            cablePeeled.SetActive(state == CableState.Peeled);
            cableRJ45.SetActive(state == CableState.RJ45);
            cableRJ45Crimped.SetActive(state == CableState.RJ45Crimped);
        }
        
        private void HandleCableStateChanged(CableState state)
        {
            switch (state)
            {
                case CableState.Whole:
                    ShowCable(CableState.Whole);
                    break;

                case CableState.Peeled:

                    debrisSpawner.Spawn();

                    ShowCable(CableState.Peeled);

                    break;

                case CableState.RJ45:

                    ShowCable(CableState.RJ45);

                    wirePuzzle.SetActive(false);

                    break;

                case CableState.RJ45Crimped:

                    ShowCable(CableState.RJ45Crimped);

                    break;
            }
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