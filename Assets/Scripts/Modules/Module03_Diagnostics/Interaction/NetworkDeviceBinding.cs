using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    [DisallowMultipleComponent]
    public sealed class NetworkDeviceBinding : MonoBehaviour
    {
        [SerializeField] private string deviceId;
        public string DeviceId => deviceId;
        public void Configure(string id) => deviceId = id;
    }
}
