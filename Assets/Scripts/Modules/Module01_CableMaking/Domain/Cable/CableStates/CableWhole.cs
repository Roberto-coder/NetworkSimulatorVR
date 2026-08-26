using Framework.Interaction.Tools.Interfaces;
using Modules.Module01_CableMaking.Flow.Validation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable.CableStates
{
    public class CableWhole : MonoBehaviour, IPeelable
    {
        [SerializeField] private CableEnd end;

        [SerializeField] private CableStateController stateController;

        private void Awake()
        {
            stateController ??= CableStateController.ResolveFor(this);
            Debug.Log(
                $"[CableWhole] Inicializado para {end}. " +
                $"Controlador: {(stateController == null ? "NO ENCONTRADO" : stateController.name)}",
                this);
        }

        public bool CanPeel =>
            stateController != null && stateController.CanPeel(end);

        public void Peel()
        {
            ModuleActionValidator validator =
                SimulationManager.Instance?.FlowController?.ActionValidator;

            if (validator != null &&
                !validator.TryValidate(ModuleActionType.Peel, end))
            {
                return;
            }

            if (stateController == null)
            {
                Debug.LogError($"[CableWhole] No hay CableStateController para {end}.", this);
                return;
            }

            Debug.Log(
                $"[CableWhole] Peel solicitado para {end}. " +
                $"Estado={stateController.GetState(end)}, CanPeel={CanPeel}, " +
                $"Controlador={stateController.name}",
                this);

            if (!CanPeel)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(end, CableState.Peeled))
            {
                Debug.LogWarning($"[CableWhole] No se pudo avanzar {end} a Peeled.", this);
                return;
            }

            Debug.Log($"[CableWhole] {end} pelado correctamente.", this);
            CableEvents.RaiseCablePeeled(end);
        }
    }
}
