using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>
    /// Impide seleccionar por ray lejano sin desactivar el hover que usan las tarjetas.
    /// La distancia se calcula desde la mano/controlador al collider de agarre.
    /// </summary>
    public sealed class NearOnlyGrabSelectFilter : MonoBehaviour, IXRSelectFilter
    {
        [Min(0.05f)] [SerializeField] private float maximumGrabDistance = 0.35f;
        [SerializeField] private XRBaseInteractable interactable;

        public bool canProcess => isActiveAndEnabled;

        private void Awake()
        {
            if (interactable == null)
                interactable = GetComponent<XRBaseInteractable>();
        }

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable target)
        {
            if (interactable == null || interactor is not Component source)
                return false;

            float maximumSqr = maximumGrabDistance * maximumGrabDistance;
            foreach (Collider targetCollider in interactable.colliders)
            {
                if (targetCollider == null || !targetCollider.enabled)
                    continue;
                Vector3 closest = targetCollider.ClosestPoint(source.transform.position);
                if ((closest - source.transform.position).sqrMagnitude <= maximumSqr)
                    return true;
            }
            return false;
        }
    }
}
