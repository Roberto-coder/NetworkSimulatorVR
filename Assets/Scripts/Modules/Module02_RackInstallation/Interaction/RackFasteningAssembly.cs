using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>Independent mechanic: seated on rails is distinct from secured by two screws.</summary>
    public sealed class RackFasteningAssembly : MonoBehaviour
    {
        [SerializeField] private RackInsertionSlot slot;
        [SerializeField] private XRGrabInteractable device;
        [SerializeField] private RackScrewZone leftScrew;
        [SerializeField] private RackScrewZone rightScrew;
        [SerializeField] private UnityEvent onSecured;
        private bool notified;

        public bool CanFasten => slot != null && slot.IsInstalled && slot.ActiveGrab == device;
        public int FastenedCount => (leftScrew != null && leftScrew.IsFastened ? 1 : 0) +
                                    (rightScrew != null && rightScrew.IsFastened ? 1 : 0);
        public bool IsSecured => CanFasten && leftScrew != rightScrew && FastenedCount == 2;
        public event Action Secured;

        private void OnEnable()
        {
            if (slot != null) slot.InstallationReset += ResetFastening;
        }
        private void OnDisable()
        {
            if (slot != null) slot.InstallationReset -= ResetFastening;
        }
        internal void NotifyScrewFastened()
        {
            if (!IsSecured || notified) return;
            notified = true;
            onSecured?.Invoke();
            Secured?.Invoke();
        }
        private void ResetFastening()
        {
            notified = false;
            leftScrew?.ResetFastening();
            rightScrew?.ResetFastening();
        }
    }
}
