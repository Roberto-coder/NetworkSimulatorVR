#if UNITY_EDITOR
using Systems.Save;
using UnityEditor;
using UnityEngine;

public static class NetworkOfflineSimulation
{
    private const string Path = "Network Simulator/Pruebas/Simular falta de conexión";

    [MenuItem(Path)]
    private static void Toggle()
    {
        NetworkAvailability.SimulateOffline = !NetworkAvailability.SimulateOffline;
        Debug.Log(NetworkAvailability.SimulateOffline
            ? "Simulación sin conexión activada: login, registro y Firebase usarán el aviso. Meta Link no se desconecta."
            : "Simulación sin conexión desactivada: se usa la disponibilidad de red del equipo.");
    }

    [MenuItem(Path, true)]
    private static bool Validate()
    {
        Menu.SetChecked(Path, NetworkAvailability.SimulateOffline);
        return true;
    }
}
#endif
