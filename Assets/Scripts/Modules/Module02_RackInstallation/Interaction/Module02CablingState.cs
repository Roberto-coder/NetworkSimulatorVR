using System;
using System.Collections.Generic;
using System.Text;
using Shared.Cabling;
using TMPro;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>Live cabling state, independent of objective order and switch power.</summary>
    public sealed class Module02CablingState : MonoBehaviour
    {
        [SerializeField] private List<PatchCableLink> cables = new();
        [SerializeField] private TMP_Text statusText;
        private readonly bool[] connected = new bool[5];
        private string previousDisplay;
        private float nextRefresh;
        public int ConnectedCount { get; private set; }
        public int IncorrectCount { get; private set; }
        public bool IsComplete => ConnectedCount == Module02ConnectionPlan.Links.Count && IncorrectCount == 0;
        public event Action StateChanged;
        public bool IsConnectionPresent(int index) => index >= 0 && index < connected.Length && connected[index];

        // Runs after cable/port Update and also catches disabled or destroyed objects.
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;
            RefreshState();
        }
        private void OnEnable() => RefreshState();

        public void RefreshState()
        {
            Array.Clear(connected, 0, connected.Length);
            ConnectedCount = 0; IncorrectCount = 0;
            var wrong = new StringBuilder();
            foreach (var cable in cables)
            {
                if (cable == null || !cable.isActiveAndEnabled) continue;
                cable.RefreshLink();
                var start = cable.StartPort;
                var end = cable.EndPort;
                if (start == null || end == null || !start.isActiveAndEnabled || !end.isActiveAndEnabled) continue;
                int match = Module02ConnectionPlan.FindMatch(start.Address, end.Address, cable.Kind, start.Kind, end.Kind);
                if (match >= 0 && !connected[match]) { connected[match] = true; ConnectedCount++; }
                else
                {
                    IncorrectCount++;
                    wrong.AppendLine($"Revisar: {start.Address} ↔ {end.Address}");
                }
            }
            var display = new StringBuilder($"Conexiones correctas: {ConnectedCount}/5\n\n");
            for (int i = 0; i < connected.Length; i++)
            {
                var link = Module02ConnectionPlan.Links[i];
                display.AppendLine($"[{(connected[i] ? "OK" : "--")}] {link.Origin} → {link.Destination}");
            }
            if (IncorrectCount > 0) display.AppendLine().Append(wrong);
            string value = display.ToString();
            if (previousDisplay == value) return;
            previousDisplay = value;
            if (statusText != null) statusText.text = value;
            StateChanged?.Invoke();
        }
    }
}
