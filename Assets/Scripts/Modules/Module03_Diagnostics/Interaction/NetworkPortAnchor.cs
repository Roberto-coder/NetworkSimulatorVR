using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Anclaje de autoría: todavía no es un socket agarrable del sprint 6.</summary>
    [DisallowMultipleComponent]
    public sealed class NetworkPortAnchor : MonoBehaviour
    {
        [SerializeField] private string portId;
        public string PortId => portId;
        public void Configure(string id) => portId = id;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.025f);
        }
    }
}
