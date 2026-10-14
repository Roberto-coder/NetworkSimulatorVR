using UnityEngine;

namespace Systems.Save
{
    /// <summary>Disponibilidad de red y simulación exclusiva del Editor.</summary>
    public static class NetworkAvailability
    {
#if UNITY_EDITOR
        public static bool SimulateOffline { get; set; }
#endif
        public static bool IsOffline
        {
            get
            {
#if UNITY_EDITOR
                if (SimulateOffline) return true;
#endif
                return Application.internetReachability == NetworkReachability.NotReachable;
            }
        }
    }
}
