using UnityEngine;

namespace Shared.Cabling
{
    [RequireComponent(typeof(PatchCableLink))]
    public sealed class CableLabelPair : MonoBehaviour
    {
        [SerializeField] private CableEndpointLabel startLabel;
        [SerializeField] private CableEndpointLabel endLabel;
        private PatchCableLink link;
        private NetworkPort previousStart, previousEnd;
        private string previousStartId, previousEndId;
        private bool valid;
        public int LabeledCount => valid ? (startLabel != null && startLabel.IsLabeled ? 1 : 0) +
            (endLabel != null && endLabel.IsLabeled ? 1 : 0) : 0;

        private void Awake() => link = GetComponent<PatchCableLink>();
        private void OnEnable()
        {
            if (link == null) link = GetComponent<PatchCableLink>();
            link.LinkChanged += OnLinkChanged;
            Invalidate();
        }
        private void OnDisable()
        {
            if (link != null) link.LinkChanged -= OnLinkChanged;
            Invalidate();
        }
        private void OnLinkChanged(PatchCableLink _) => Invalidate();
        private void LateUpdate() => Refresh();
        private void Invalidate()
        {
            valid = false;
            startLabel?.Clear(); endLabel?.Clear();
        }
        public void Refresh()
        {
            link.RefreshLink();
            var start = link.StartPort; var end = link.EndPort;
            string startId = start != null ? start.Address : null;
            string endId = end != null ? end.Address : null;
            if (!link.HasCompleteLink || start != previousStart || end != previousEnd ||
                startId != previousStartId || endId != previousEndId) Invalidate();
            previousStart = start; previousEnd = end;
            previousStartId = startId; previousEndId = endId;
            valid = link.HasCompleteLink;
        }
        public bool TryGetText(out string value)
        {
            value = null;
            if (!isActiveAndEnabled) return false;
            Refresh();
            if (!valid) return false;
            value = $"{previousStartId}\n↔ {previousEndId}";
            return true;
        }
    }
}
