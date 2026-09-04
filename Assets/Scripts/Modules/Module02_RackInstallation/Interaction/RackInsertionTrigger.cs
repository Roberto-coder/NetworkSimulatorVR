using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class RackInsertionTrigger : MonoBehaviour
    {
        [SerializeField] private RackInsertionSlot slot;

        private void Reset()
        {
            slot = GetComponentInParent<RackInsertionSlot>();
            Collider trigger = GetComponent<Collider>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) => slot?.Consider(other);
        private void OnTriggerStay(Collider other) => slot?.Consider(other);
    }
}
