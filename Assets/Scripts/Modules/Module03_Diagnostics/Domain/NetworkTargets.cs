using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    public enum NetworkRuleKind { Address, PortEnabled, HealthyCableBetween, UniqueAddresses }

    [Serializable]
    public sealed class NetworkTargetRule
    {
        public string id;
        public NetworkRuleKind kind;
        public string portA;
        public string portB;
        public string ipv4;
        public int prefixLength = 24;
        public bool enabled = true;
        public bool requireLabels;
        public string labelAtA;
        public string labelAtB;
    }

    [Serializable]
    public sealed class NetworkTargetDefinition
    {
        public int schemaVersion = 1;
        public string scenarioId = "office";
        public List<NetworkTargetRule> rules = new();
    }

    public sealed class NetworkRuleResult
    {
        public string RuleId { get; }
        public bool Passed { get; }
        public string Expected { get; }
        public string Observed { get; }
        internal NetworkRuleResult(string id, bool passed, string expected, string observed)
        { RuleId = id; Passed = passed; Expected = expected; Observed = observed; }
    }

    /// <summary>Comparación semántica: un cable nuevo puede satisfacer el enlace del cable retirado.</summary>
    public static class NetworkTargetValidator
    {
        public static IReadOnlyList<string> ValidateConfiguration(NetworkDefinition network, NetworkTargetDefinition target)
        {
            var errors = new List<string>(NetworkDefinitionValidator.Validate(network));
            if (target == null || target.rules == null || target.rules.Count == 0)
            { errors.Add("Faltan reglas finales."); return errors; }
            if (errors.Count > 0) return errors;
            if (target.schemaVersion != 1 || target.scenarioId != network.scenarioId) errors.Add("Reglas de otra versión o escenario.");
            var ids = new HashSet<string>();
            foreach (var r in target.rules)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.id) || !ids.Add(r.id))
                { errors.Add("Regla nula o ID vacío/repetido."); continue; }
                if (!Enum.IsDefined(typeof(NetworkRuleKind), r.kind)) errors.Add($"Tipo de regla desconocido: {r.id}.");
                if (r.kind == NetworkRuleKind.UniqueAddresses) continue;
                var a = network.ports.Find(p => p.id == r.portA);
                if (a == null) { errors.Add($"Puerto inexistente en {r.id}."); continue; }
                bool passive = network.devices.Find(d => d.id == a.deviceId).kind == DeviceKind.Passive;
                if (r.kind == NetworkRuleKind.Address && (passive || !NetworkDefinitionValidator.IsMac(a.mac) ||
                    !NetworkDefinitionValidator.IsIpv4(r.ipv4) || r.prefixLength < 1 || r.prefixLength > 30)) errors.Add($"Regla IP inválida: {r.id}.");
                if (r.kind == NetworkRuleKind.PortEnabled && passive) errors.Add($"Puerto pasivo no administrable: {r.id}.");
                if (r.kind == NetworkRuleKind.HealthyCableBetween &&
                    (r.portA == r.portB || !network.ports.Any(p => p.id == r.portB) ||
                    (r.requireLabels && (string.IsNullOrWhiteSpace(r.labelAtA) || string.IsNullOrWhiteSpace(r.labelAtB)))))
                    errors.Add($"Extremos o etiquetas inválidos: {r.id}.");
            }
            return errors;
        }

        public static IReadOnlyList<NetworkRuleResult> Evaluate(NetworkSession session, NetworkTargetDefinition target)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var n = session.Snapshot();
            // Las duplicidades actuales son resultados de diagnóstico, no errores de esquema.
            n.allowedDuplicatePortIds = n.ports.Select(p => p.id).ToList();
            var errors = ValidateConfiguration(n, target);
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors), nameof(target));
            var results = new List<NetworkRuleResult>();
            foreach (var r in target.rules)
            {
                var p = n.ports.Find(port => port.id == r.portA);
                switch (r.kind)
                {
                    case NetworkRuleKind.Address:
                        results.Add(new NetworkRuleResult(r.id, p.ipv4 == r.ipv4 && p.prefixLength == r.prefixLength,
                            $"{r.ipv4}/{r.prefixLength}", $"{p.ipv4}/{p.prefixLength}")); break;
                    case NetworkRuleKind.PortEnabled:
                        results.Add(new NetworkRuleResult(r.id, p.enabled == r.enabled, r.enabled.ToString(), p.enabled.ToString())); break;
                    case NetworkRuleKind.UniqueAddresses:
                        var duplicates = n.ports.Where(port => !string.IsNullOrEmpty(port.ipv4))
                            .GroupBy(port => (port.segmentId, port.ipv4)).Where(g => g.Count() > 1)
                            .Select(g => string.Join(", ", g.Select(port => port.id))).ToArray();
                        results.Add(new NetworkRuleResult(r.id, duplicates.Length == 0, "IPv4 únicas por segmento", string.Join("; ", duplicates))); break;
                    case NetworkRuleKind.HealthyCableBetween:
                        var c = n.cables.Find(cable => (cable.portA == r.portA && cable.portB == r.portB) ||
                            (cable.portA == r.portB && cable.portB == r.portA));
                        bool forward = c != null && c.portA == r.portA;
                        bool labels = c != null && (!r.requireLabels ||
                            ((forward ? c.labelA : c.labelB) == r.labelAtA && (forward ? c.labelB : c.labelA) == r.labelAtB));
                        results.Add(new NetworkRuleResult(r.id, c != null && c.intact && labels,
                            $"{r.portA} ↔ {r.portB}: íntegro" + (r.requireLabels ? $", etiquetas {r.labelAtA} / {r.labelAtB}" : ""),
                            c == null ? "Sin cable entre extremos" : $"{c.id}: intacto={c.intact}, etiquetas={c.labelA} / {c.labelB}")); break;
                }
            }
            return results;
        }
    }
}
