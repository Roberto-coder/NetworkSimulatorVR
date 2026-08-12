using Modules.Module01_CableMaking.Domain.Wire;
using Oculus.Interaction;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    /// <summary>
    /// Usa el SDK de Meta para seleccionar una punta, pero restringe su
    /// movimiento al plano del puzzle. No depende de CenterEyeAnchor.
    /// </summary>
    public class WireGrabHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Wire wire;
        [SerializeField] private WireView wireView;
        [SerializeField] private WireSnapSolver snapSolver;
        [SerializeField] private Grabbable grabbable;

        private RayInteractor activeRayInteractor;

        private void Awake()
        {
            if (grabbable == null)
                grabbable = GetComponent<Grabbable>();

            if (grabbable == null)
            {
                Debug.LogError("[WireGrabHandler] Falta Grabbable.", this);
                return;
            }

            WireGrabTransformer transformer =
                grabbable.GetComponent<WireGrabTransformer>();
            if (transformer == null)
                transformer = grabbable.gameObject.AddComponent<WireGrabTransformer>();

            // El SDK mantiene la seleccion y los eventos de la mano, pero no
            // puede mover libremente la punta fuera del plano del puzzle.
            grabbable.InjectOptionalOneGrabTransformer(transformer);
            grabbable.MaxGrabPoints = 1;
            grabbable.InjectOptionalThrowWhenUnselected(false);

            Rigidbody rigidbody = grabbable.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                grabbable.InjectOptionalRigidbody(rigidbody);
                grabbable.InjectOptionalKinematicWhileSelected(true);
            }
        }

        private void OnEnable()
        {
            if (grabbable != null)
                grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }

        private void OnDisable()
        {
            if (grabbable != null)
                grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }

        private void HandlePointerEvent(PointerEvent pointerEvent)
        {
            switch (pointerEvent.Type)
            {
                case PointerEventType.Select:
                    BeginGrab(pointerEvent.Identifier);
                    break;

                case PointerEventType.Move:
                    Drag(pointerEvent.Pose.position);
                    break;

                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    EndGrab();
                    activeRayInteractor = null;
                    break;
            }
        }

        private void BeginGrab(int pointerIdentifier)
        {
            if (wire == null)
                return;

            activeRayInteractor = FindRayInteractor(pointerIdentifier);

            // Si el alumno toma un hilo ya colocado, libera su slot para que
            // pueda corregir la secuencia sin dejar posiciones bloqueadas.
            wire.Disconnect();
        }

        private void Drag(Vector3 handPosition)
        {
            if (wireView == null)
                return;

            if (activeRayInteractor != null &&
                wireView.TryProjectRay(activeRayInteractor.Ray, out Vector3 rayPoint))
            {
                wireView.Drag(rayPoint);
                return;
            }

            // Compatibilidad con interacciones cercanas de mano/control.
            wireView.Drag(handPosition);
        }

        private void EndGrab()
        {
            if (wire == null || wireView == null || snapSolver == null)
                return;

            if (!snapSolver.TrySnap(wire, wireView))
                wireView.ResetView();
        }

        private static RayInteractor FindRayInteractor(int pointerIdentifier)
        {
            RayInteractor[] interactors =
                FindObjectsByType<RayInteractor>(FindObjectsSortMode.None);

            foreach (RayInteractor interactor in interactors)
            {
                if (interactor.Identifier == pointerIdentifier)
                    return interactor;
            }

            return null;
        }
    }
}
