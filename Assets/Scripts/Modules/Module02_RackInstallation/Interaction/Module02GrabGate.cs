using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module02_RackInstallation.Flow
{
    /// <summary>Permite manipular el switch durante su inspección y montaje.</summary>
    public sealed class Module02GrabGate : MonoBehaviour, IXRSelectFilter
    {
        private XRGrabInteractable grab;
        public bool canProcess => isActiveAndEnabled;
        private void Awake() => grab = GetComponent<XRGrabInteractable>();
        private void OnEnable() => grab.selectFilters.Add(this);
        private void OnDisable() => grab.selectFilters.Remove(this);
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable target) =>
            Module02SequenceCoordinator.AllowSwitchGrab();
    }
}
