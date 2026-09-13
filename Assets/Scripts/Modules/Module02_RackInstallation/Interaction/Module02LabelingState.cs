using System;
using System.Collections.Generic;
using Shared.Cabling;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    public sealed class Module02LabelingState : MonoBehaviour
    {
        [SerializeField] private List<CableLabelPair> cables = new();
        [SerializeField] private int labeledEndCount;
        [SerializeField] private int fullyLabeledCableCount;
        public int LabeledEndCount => labeledEndCount;
        public int FullyLabeledCableCount => fullyLabeledCableCount;
        public bool IsComplete => cables.Count == 5 && fullyLabeledCableCount == 5;
        public event Action StateChanged;
        public void SetCables(IEnumerable<CableLabelPair> instances)
        {
            cables = new List<CableLabelPair>(new HashSet<CableLabelPair>(instances)); RefreshState();
        }
        private void LateUpdate() => RefreshState();
        public void RefreshState()
        {
            int ends = 0, complete = 0;
            foreach (var cable in cables)
            {
                if (cable == null || !cable.isActiveAndEnabled) continue;
                cable.Refresh();
                ends += cable.LabeledCount;
                if (cable.LabeledCount == 2) complete++;
            }
            if (ends == labeledEndCount && complete == fullyLabeledCableCount) return;
            labeledEndCount = ends; fullyLabeledCableCount = complete; StateChanged?.Invoke();
        }
    }
}
