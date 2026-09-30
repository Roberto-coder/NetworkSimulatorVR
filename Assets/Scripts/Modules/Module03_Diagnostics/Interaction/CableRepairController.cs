using System;
using System.Collections;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Presentation;
using Shared.Cabling;
using TMPro;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    [DefaultExecutionOrder(-20)]
    public sealed class CableRepairController : MonoBehaviour
    {
        public Module03NetworkScene network;
        public DiagnosticScreenController diagnostics;
        public CableRepairSettings settings;
        public RepairPatchCord[] cables;
        public TMP_Text statusText;
        public CableRepairService Service { get; private set; }
        public bool Ready { get; private set; }
        public event Action<bool> CompletionChanged;
        private bool completed;
        private long resetSequence;
        private IEnumerator Start()
        {
            // PhysicCable inicializa sus conectores en Start; esperar antes de observarlo.
            yield return null;
            if (network == null || network.Session == null || settings == null || diagnostics == null || cables == null)
            { Debug.LogError("M3 reparación: faltan referencias o sesión.", this); enabled = false; yield break; }
            var initial = network.Session.Snapshot();
            if (cables.Any(c => c == null || c.endA == null || c.endB == null) ||
                initial.cables.Where(c => c.interactable).Any(c => cables.Count(b => b.cableId == c.id) != 1) ||
                cables.Any(c => !initial.cables.Any(d => d.id == c.cableId && d.interactable)))
            { Debug.LogError("M3 reparación: cada patch cord interactuable necesita un binding único y ambos conectores.", this); enabled = false; yield break; }
            var sockets = network.GetComponentsInChildren<NetworkPort>(true);
            var required = initial.cables.Where(c => c.interactable).SelectMany(c => new[] { c.portA, c.portB }).Where(id => !string.IsNullOrEmpty(id)).Distinct();
            if (sockets.GroupBy(p => p.Address).Any(g => g.Count() > 1) || required.Any(id => sockets.Count(p => p.Address == id && p.Socket != null) != 1))
            { Debug.LogError("M3 reparación: faltan sockets únicos para los puertos iniciales.", this); enabled = false; yield break; }
            Service = new CableRepairService(network.Session, network.Diagnostics, settings.incident);
            RestoreConnections(); Ready = true;
            diagnostics.ProbeExecuted += Observe;
            diagnostics.CommandExecuting += SyncConnections;
        }
        private void OnDisable()
        {
            if (diagnostics != null)
            { diagnostics.ProbeExecuted -= Observe; diagnostics.CommandExecuting -= SyncConnections; }
            Ready = false;
        }
        private void OnEnable()
        {
            if (Service == null) return;
            Ready = true; diagnostics.ProbeExecuted += Observe; diagnostics.CommandExecuting += SyncConnections;
        }
        private void Observe(ProbeResult result, DiagnosticScreenView _) { SyncConnections(); Service.ObserveProbe(result); RefreshStatus(); }
        private void Update()
        {
            if (!Ready) return;
            long latest = network.Session.History.LastOrDefault(h => h.Action == "Reset")?.Sequence ?? 0;
            if (latest != resetSequence) { resetSequence = latest; RestoreConnections(); }
            SyncConnections(); RefreshStatus();
        }
        public void SyncConnections()
        {
            if (!Ready) return;
            foreach (var cable in cables) cable.Link.RefreshLink();
            // Liberar primero conexiones antiguas evita que el orden de cables deje puertos falsamente ocupados.
            foreach (var cable in cables)
            {
                var state = network.Session.Snapshot().cables.Find(c => c.id == cable.cableId);
                string a = cable.isActiveAndEnabled ? cable.Link.StartPort?.Address ?? "" : "";
                string b = cable.isActiveAndEnabled ? cable.Link.EndPort?.Address ?? "" : "";
                if (state.portA != a || state.portB != b) network.Session.ConnectCable(cable.cableId, "", "");
            }
            foreach (var cable in cables)
            {
                var state = network.Session.Snapshot().cables.Find(c => c.id == cable.cableId);
                string a = cable.isActiveAndEnabled ? cable.Link.StartPort?.Address ?? "" : "";
                string b = cable.isActiveAndEnabled ? cable.Link.EndPort?.Address ?? "" : "";
                if ((state.portA != a || state.portB != b) && !network.Session.ConnectCable(cable.cableId, a, b))
                {
                    // Una conexión física rechazada nunca debe mostrarse como enlace lógico válido.
                    cable.endA.Disconnect(); cable.endB.Disconnect();
                    Debug.LogWarning("M3: conexión rechazada para " + cable.cableId, cable);
                }
                if (cable.labelA != null) cable.labelA.text = state.labelA;
                if (cable.labelB != null) cable.labelB.text = state.labelB;
            }
        }
        private void RestoreConnections()
        {
            var sockets = network.GetComponentsInChildren<NetworkPort>(true).ToDictionary(p => p.Address);
            foreach (var cable in cables) { cable.endA.Disconnect(); cable.endB.Disconnect(); }
            foreach (var cable in cables)
            {
                var state = network.Session.Snapshot().cables.Find(c => c.id == cable.cableId);
                // Sólo al inicio/reset: después el usuario controla todos los encajes.
                if (!string.IsNullOrEmpty(state.portA) && sockets.TryGetValue(state.portA, out var a)) a.Socket.Connect(cable.endA);
                if (!string.IsNullOrEmpty(state.portB) && sockets.TryGetValue(state.portB, out var b)) b.Socket.Connect(cable.endB);
                cable.Link.RefreshLink();
            }
        }
        public void RefreshStatus()
        {
            if (Service == null) return;
            if (statusText != null) statusText.text = Service.Status;
            bool value = Service.IsComplete;
            if (completed == value) return;
            completed = value; CompletionChanged?.Invoke(value);
        }
    }
}
