using UnityEngine;

namespace Shared.Cabling
{
    [DisallowMultipleComponent]
    public sealed class DeviceIdentity : MonoBehaviour
    {
        [SerializeField] private string deviceId;
        public string DeviceId => deviceId;
        public void Configure(string value) => deviceId = value;
    }
}
