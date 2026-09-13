using Modules.Module02_RackInstallation.Flow.Validation;
using TMPro;
using UnityEngine;

namespace Shared.Cabling
{
    public sealed class CableEndpointLabel : MonoBehaviour
    {
        [SerializeField] private CableLabelPair owner;
        [SerializeField] private GameObject visual;
        [SerializeField] private TMP_Text text;
        public bool IsLabeled { get; private set; }
        private void Awake() => Clear();
        private void OnDisable() => Clear();
        public bool TryLabel()
        {
            if (!Modules.Module02_RackInstallation.Flow.Module02SequenceCoordinator.Allow(
                Module02Action.Label, true)) return false;
            if (!isActiveAndEnabled || owner == null || !owner.TryGetText(out string value)) return false;
            if (text == null || visual == null) return false;
            text.text = value;
            visual.SetActive(true);
            IsLabeled = true;
            return true;
        }
        public void Clear()
        {
            IsLabeled = false;
            if (visual != null) visual.SetActive(false);
        }
    }
}
