using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    /// <summary>Rechaza referencias ambiguas antes de construir el estado de ejecución.</summary>
    public static class NetworkDefinitionValidator
    {
        public static bool IsIpv4(string value)
        {
            var parts = (value ?? "").Split('.');
            return parts.Length == 4 && parts.All(p => p.Length > 0 && p.Length <= 3 &&
                p.All(c => c >= '0' && c <= '9') && (p.Length == 1 || p[0] != '0') && byte.TryParse(p, out _));
        }

        public static bool IsMac(string value)
        {
            var parts = (value ?? "").Split(':');
            return parts.Length == 6 && parts.All(p => p.Length == 2 && p.All(Uri.IsHexDigit));
        }

        public static IReadOnlyList<string> Validate(NetworkDefinition data)
        {
            var errors = new List<string>();
            if (data == null) { errors.Add("Falta configuración inicial."); return errors; }
            if (data.schemaVersion != 1) errors.Add("Versión de esquema no soportada.");
            if (string.IsNullOrWhiteSpace(data.scenarioId)) errors.Add("Falta ID de escenario.");
            if (data.devices == null || data.ports == null || data.cables == null ||
                data.passiveConnections == null || data.allowedDuplicatePortIds == null)
            { errors.Add("Las colecciones no pueden ser nulas."); return errors; }
            if (data.devices.Count == 0) errors.Add("La red necesita dispositivos.");
            CheckIds(data.devices.Select(d => d?.id), "dispositivo", errors);
            CheckIds(data.ports.Select(p => p?.id), "puerto", errors);
            CheckIds(data.cables.Select(c => c?.id), "cable", errors);
            if (errors.Count > 0) return errors;
            var devices = data.devices.ToDictionary(d => d.id);
            var ports = data.ports.ToDictionary(p => p.id);
            foreach (var d in data.devices)
            {
                if (!Enum.IsDefined(typeof(DeviceKind), d.kind)) errors.Add($"Tipo desconocido: {d.id}.");
                if (string.IsNullOrWhiteSpace(d.prefabKey)) errors.Add($"Falta clave de prefab: {d.id}.");
            }
            foreach (var p in data.ports)
            {
                if (p.deviceId == null || !devices.TryGetValue(p.deviceId, out var device))
                { errors.Add($"Dispositivo inexistente: {p.id}."); continue; }
                if (string.IsNullOrWhiteSpace(p.segmentId)) errors.Add($"Falta segmento: {p.id}.");
                if (p.prefixLength < 1 || p.prefixLength > 30) errors.Add($"Prefijo fuera del alcance LAN /1–/30: {p.id}.");
                if (!string.IsNullOrEmpty(p.ipv4) && !IsIpv4(p.ipv4)) errors.Add($"IPv4 inválida: {p.id}.");
                if ((!string.IsNullOrEmpty(p.ipv4) || !string.IsNullOrEmpty(p.mac)) && !IsMac(p.mac)) errors.Add($"MAC inválida: {p.id}.");
                if (device.kind == DeviceKind.Passive && (!string.IsNullOrEmpty(p.ipv4) || !string.IsNullOrEmpty(p.mac)))
                    errors.Add($"Un elemento pasivo no tiene IP/MAC: {p.id}.");
            }
            foreach (var group in data.ports.Where(p => !string.IsNullOrEmpty(p.mac)).GroupBy(p => p.mac, StringComparer.OrdinalIgnoreCase))
                if (group.Count() > 1) errors.Add($"MAC repetida: {group.Key}.");
            foreach (string id in data.allowedDuplicatePortIds)
                if (id == null || !ports.ContainsKey(id)) errors.Add("Excepción de duplicidad apunta a puerto inexistente.");
            foreach (var group in data.ports.Where(p => !string.IsNullOrEmpty(p.ipv4)).GroupBy(p => (p.segmentId, p.ipv4)))
                if (group.Count() > 1 && group.Any(p => !data.allowedDuplicatePortIds.Contains(p.id)))
                    errors.Add($"IP duplicada no declarada: {group.Key}.");
            var occupied = new HashSet<string>();
            foreach (var c in data.cables)
            {
                foreach (string end in new[] { c.portA, c.portB })
                    if (!string.IsNullOrEmpty(end))
                    {
                        if (!ports.ContainsKey(end)) errors.Add($"Extremo inexistente en {c.id}: {end}.");
                        if (!occupied.Add(end)) errors.Add($"Puerto ocupado dos veces: {end}.");
                    }
                if (!c.interactable && (string.IsNullOrEmpty(c.portA) || string.IsNullOrEmpty(c.portB)))
                    errors.Add($"Enlace fijo incompleto: {c.id}.");
            }
            var passiveEnds = new HashSet<string>();
            foreach (var link in data.passiveConnections)
            {
                if (link == null || link.portA == null || link.portB == null ||
                    !ports.TryGetValue(link.portA, out var a) || !ports.TryGetValue(link.portB, out var b))
                { errors.Add("Continuidad pasiva con extremos inexistentes."); continue; }
                if (a.deviceId == null || a.deviceId != b.deviceId || !devices.TryGetValue(a.deviceId, out var d) || d.kind != DeviceKind.Passive)
                    errors.Add("La continuidad interna debe unir puertos del mismo dispositivo pasivo.");
                if (!passiveEnds.Add(a.id) || !passiveEnds.Add(b.id)) errors.Add("Continuidad pasiva ambigua o duplicada.");
            }
            return errors;
        }

        private static void CheckIds(IEnumerable<string> ids, string type, List<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (string id in ids)
                if (string.IsNullOrWhiteSpace(id) || id.Trim() != id || !seen.Add(id))
                    errors.Add($"ID de {type} vacío, repetido o con espacios externos: {id}.");
        }
    }
}
