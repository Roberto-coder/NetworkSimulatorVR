using System;
using System.Collections.Generic;

namespace Modules.Module03_Diagnostics.Domain
{
    public enum DeviceKind { Computer, Switch, Passive, DiagnosticStation }

    // DTO serializables: Unity y un futuro importador JSON comparten estos datos.
    // Los IDs de puertos son globales (por ejemplo PC-01/eth0), nunca índices de listas.
    [Serializable]
    public sealed class DeviceDefinition
    {
        public string id;
        public DeviceKind kind;
        public string prefabKey;
    }

    [Serializable]
    public sealed class PortDefinition
    {
        public string id;
        public string deviceId;
        public string segmentId = "office";
        public bool enabled = true;
        public string ipv4 = "";
        public int prefixLength = 24;
        public string mac = "";
    }

    [Serializable]
    public sealed class CableDefinition
    {
        public string id;
        // Un extremo vacío representa un cable desconectado, no un puerto inexistente.
        public string portA = "";
        public string portB = "";
        public bool intact = true;
        public bool interactable;
        public string labelA = "";
        public string labelB = "";
    }

    [Serializable]
    public sealed class PassiveConnection
    {
        public string portA;
        public string portB;
    }

    [Serializable]
    public sealed class NetworkDefinition
    {
        public int schemaVersion = 1;
        public string scenarioId = "office";
        public List<DeviceDefinition> devices = new();
        public List<PortDefinition> ports = new();
        public List<CableDefinition> cables = new();
        public List<PassiveConnection> passiveConnections = new();
        // Solo los conflictos declarados se admiten como estado inicial del ejercicio.
        public List<string> allowedDuplicatePortIds = new();

        public NetworkDefinition Copy()
        {
            var copy = new NetworkDefinition { schemaVersion = schemaVersion, scenarioId = scenarioId };
            foreach (var d in devices) copy.devices.Add(new DeviceDefinition { id = d.id, kind = d.kind, prefabKey = d.prefabKey });
            foreach (var p in ports) copy.ports.Add(new PortDefinition { id = p.id, deviceId = p.deviceId,
                segmentId = p.segmentId, enabled = p.enabled, ipv4 = p.ipv4, prefixLength = p.prefixLength, mac = p.mac });
            foreach (var c in cables) copy.cables.Add(new CableDefinition { id = c.id, portA = c.portA,
                portB = c.portB, intact = c.intact, interactable = c.interactable, labelA = c.labelA, labelB = c.labelB });
            foreach (var p in passiveConnections) copy.passiveConnections.Add(new PassiveConnection { portA = p.portA, portB = p.portB });
            copy.allowedDuplicatePortIds.AddRange(allowedDuplicatePortIds);
            return copy;
        }
    }
}
