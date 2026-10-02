using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using UnityEngine;

namespace Modules.Module03_Diagnostics
{
    public sealed class Module03NetworkScene : MonoBehaviour
    {
        public NetworkInitialStateAsset initialState;
        public NetworkLayoutAsset layout;
        public NetworkPrefabCatalog prefabCatalog;
        public NetworkSession Session { get; private set; }
        public NetworkSimulationService Diagnostics { get; private set; }
        private NetworkPortAnchor[] ledAnchors = Array.Empty<NetworkPortAnchor>();
        private Shared.Cabling.NetworkPort[] physicalPorts = Array.Empty<Shared.Cabling.NetworkPort>();
        private readonly Dictionary<string, bool> linkStates = new();
        private long ledRevision = -1;

        private void Awake()
        {
            // La geometría visual no determina conectividad. Permitir diagnosticar la red
            // mientras se acomoda el escenario, conservando los errores de IDs y anclajes.
            var visualWarnings = new List<string>();
            var errors = ValidateBindings(visualWarnings);
            foreach (var warning in visualWarnings) Debug.LogWarning(warning, this);
            if (errors.Count > 0) { Debug.LogError(string.Join("\n", errors), this); enabled = false; return; }
            Session = initialState.CreateSession();
            Diagnostics = new NetworkSimulationService(Session);
            ledAnchors = GetComponentsInChildren<NetworkPortAnchor>(true);
            physicalPorts = GetComponentsInChildren<Shared.Cabling.NetworkPort>(true);
        }

        private void LateUpdate()
        {
            if (Session == null) return;
            // CableRepairController sincroniza los extremos físicos en Update.
            // Recalcular continuidad sólo cuando cambia la red, no en cada frame.
            if (ledRevision != Session.Revision)
            {
                var snapshot = Session.Snapshot();
                linkStates.Clear();
                foreach (var port in snapshot.ports)
                    linkStates[port.id] = DiagnosticWorkspace.HasLink(snapshot, port.id);
                ledRevision = Session.Revision;
            }
            foreach (var anchor in ledAnchors)
                if (anchor != null) anchor.SetLinkState(linkStates.TryGetValue(anchor.PortId, out bool linked) && linked);
            // También actualizar LEDs ya asignados a sockets NetworkPort de escenas anteriores.
            foreach (var port in physicalPorts)
                if (port != null) port.SetOperationalLink(port.isActiveAndEnabled && linkStates.TryGetValue(port.Address, out bool linked) && linked);
        }

        private void OnDisable()
        {
            ledRevision = -1;
            foreach (var anchor in ledAnchors) if (anchor != null) anchor.SetLinkState(false);
            foreach (var port in physicalPorts) if (port != null) port.SetOperationalLink(false);
        }

        // La raíz delimita el inventario: un demo o una segunda escena no contaminan los IDs.
        // En autoría se conserva la validación estricta para no aprobar rutas mal construidas.
        public IReadOnlyList<string> ValidateBindings() => ValidateBindings(null);

        private IReadOnlyList<string> ValidateBindings(List<string> visualWarnings)
        {
            var errors = new List<string>();
            if (initialState == null || layout == null) { errors.Add("Falta configuración inicial/layout."); return errors; }
            NetworkDefinition network;
            try { network = initialState.CopyDefinition(); }
            catch (ArgumentException e) { errors.Add(e.Message); return errors; }
            if (layout.scenarioId != network.scenarioId) errors.Add("Layout de otro escenario.");
            if ((transform.lossyScale - Vector3.one).sqrMagnitude > 0.0001f) errors.Add("La raíz requiere escala mundial (1,1,1).");
            var devices = GetComponentsInChildren<NetworkDeviceBinding>(true);
            var ports = GetComponentsInChildren<NetworkPortAnchor>(true);
            var routes = GetComponentsInChildren<FixedNetworkCable>(true);
            foreach (var d in network.devices)
                if (devices.Count(b => b.DeviceId == d.id) != 1) errors.Add($"Debe existir un dispositivo: {d.id}.");
            foreach (var d in devices)
                if (!network.devices.Any(n => n.id == d.DeviceId)) errors.Add($"Dispositivo ajeno: {d.DeviceId}.");
            foreach (var p in network.ports)
            {
                var matches = ports.Where(a => a.PortId == p.id).ToArray();
                if (matches.Length != 1) { errors.Add($"Debe existir un anclaje: {p.id}."); continue; }
                if (matches[0].GetComponentInParent<NetworkDeviceBinding>()?.DeviceId != p.deviceId)
                    errors.Add($"Anclaje bajo dispositivo incorrecto: {p.id}.");
            }
            foreach (var p in ports)
                if (!network.ports.Any(n => n.id == p.PortId)) errors.Add($"Puerto ajeno: {p.PortId}.");
            foreach (var c in network.cables.Where(c => !c.interactable))
            {
                var matches = routes.Where(r => r.CableId == c.id).ToArray();
                if (matches.Length != 1) { errors.Add($"Debe existir una ruta fija: {c.id}."); continue; }
                var r = matches[0];
                if (r.EndpointA == null || r.EndpointB == null || r.EndpointA.PortId != c.portA || r.EndpointB.PortId != c.portB)
                    errors.Add($"Extremos incorrectos: {c.id}.");
                else
                {
                    try
                    {
                        r.Rebuild();
                        float startGap = Vector3.Distance(r.EvaluateDistance(0), r.EndpointA.transform.position);
                        float endGap = Vector3.Distance(r.EvaluateDistance(r.Length), r.EndpointB.transform.position);
                        if (r.Length <= 0.001f || startGap > 0.02f || endGap > 0.02f)
                            (visualWarnings ?? errors).Add($"Ruta desalineada o sin longitud: {c.id}. " +
                                $"Longitud: {r.Length:F3} m; separación A: {startGap:F3} m; B: {endGap:F3} m (máximo 0.020 m). " +
                                "Ajustar el primer/último punto del spline a sus anclajes y reconstruir. " +
                                "El trazado visual puede ser incorrecto; la conectividad usa la definición de red.");
                    }
                    catch (InvalidOperationException e) { errors.Add(e.Message); }
                }
            }
            foreach (var r in routes)
                if (!network.cables.Any(c => !c.interactable && c.id == r.CableId)) errors.Add($"Ruta fija ajena o interactuable: {r.CableId}.");
            return errors;
        }
    }
}
