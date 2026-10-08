#if UNITY_EDITOR
using System;
using System.Linq;
using Modules.Module02_RackInstallation.Domain;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Presentation;
using Shared.Cabling;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Module02RackIntegration
{
    [MenuItem("Network Simulator/Module 02/Validar rack integrado y segundo switch")]
    public static void ValidateCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 para validar el rack.");
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        var devices = all.Single(t => t.name == "RackDevices" && t.parent != null && t.parent.name == "_Interactuables");
        var secondary = devices.GetComponentsInChildren<DeviceIdentity>(true).Single(d => d.DeviceId == "SW2");
        var port = secondary.GetComponentsInChildren<NetworkPort>(true).Single(p => p.Address == "SW2/Gi04");
        if (port.Socket == null || port.Kind != NetworkPortKind.EthernetRj45)
            throw new InvalidOperationException("SW2/Gi04 necesita su socket Ethernet.");
        var ports = all.SelectMany(t => t.GetComponents<NetworkPort>()).Where(p => p.isActiveAndEnabled).ToArray();
        foreach (string address in Module02ConnectionPlan.Links.SelectMany(l => new[] { l.Origin, l.Destination }).Append("SW1/Console"))
            if (ports.Count(p => p.Address == address) != 1)
                throw new InvalidOperationException($"Puerto ausente o duplicado: {address}");
        if (ports.Any(p => p.DeviceId == "FW1"))
            throw new InvalidOperationException("Queda un puerto del firewall activo.");
        foreach (var fit in devices.GetComponentsInChildren<RackStaticVisualFit>(true))
            if (fit.GetComponent<RackInfoTarget>()?.Information == null)
                throw new InvalidOperationException($"Falta la ficha de {fit.name}.");
        foreach (string name in new[] { "NAS 2U", "UPS 2U", "PDU-A" })
            if (!devices.Cast<Transform>().Any(t => t.name == name))
                throw new InvalidOperationException($"Falta {name} en el rack.");
        Debug.Log("Rack integrado válido: equipos con fichas y enlace SW1/Gi04 ↔ SW2/Gi04; sockets originales conservados.", devices);
    }
}
#endif
