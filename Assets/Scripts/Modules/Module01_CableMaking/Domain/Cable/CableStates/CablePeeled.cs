using Framework.Interaction.Tools.Interfaces;
using Oculus.Interaction;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable.CableStates
{
    public class CablePeeled : MonoBehaviour, IDesordenable
    {
        [SerializeField] private CableEnd end;

        [SerializeField] private CableStateController stateController;

        [Header("Interaction")]
        [SerializeField] private Grabbable grabbable;

        private bool wasGrabbed;
        
        private void Awake()
        {
            stateController ??= CableStateController.ResolveFor(this);
            grabbable ??= GetComponentInParent<Grabbable>();
        }

        private void OnEnable()
        {
            if (grabbable == null)
            {
                Debug.LogError($"[CablePeeled] No se encontro Grabbable para {end}.", this);
                return;
            }

            grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }

        private void OnDisable()
        {
            if (grabbable != null)
                grabbable.WhenPointerEventRaised -= HandlePointerEvent;

            wasGrabbed = false;
        }

        public bool CanDisorder =>
            stateController != null && stateController.CanDisorder(end);

        public void Disorder()
        {
            if (!CanDisorder)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(end, CableState.Rj45Disordered))
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            CableEvents.RaiseCableDisordered(end);
        }

        private void HandlePointerEvent(PointerEvent pointerEvent)
        {
            if (pointerEvent.Type == PointerEventType.Select)
            {
                wasGrabbed = true;
                Debug.Log($"[CablePeeled] {end} tomado. Sueltalo para preparar el puzzle.", this);
                return;
            }

            if (pointerEvent.Type != PointerEventType.Unselect || !wasGrabbed)
                return;

            wasGrabbed = false;
            Debug.Log($"[CablePeeled] {end} soltado. Preparando puzzle RJ45.", this);
            Disorder();
        }
    }
}
