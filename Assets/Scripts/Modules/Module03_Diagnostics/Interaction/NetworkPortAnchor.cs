using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Identidad y posición del puerto; LED opcional de enlace de capa física.</summary>
    [DisallowMultipleComponent]
    public sealed class NetworkPortAnchor : MonoBehaviour
    {
        [SerializeField] private string portId;
        [Header("LED de enlace (opcional)")]
        [Tooltip("Arrastra el MeshRenderer del LED de este puerto, no el modelo completo.")]
        [SerializeField] private Renderer linkLed;
        [SerializeField] private Color disconnectedColor = new Color(.08f, .08f, .08f);
        [SerializeField] private Color connectedColor = Color.green;
        private MaterialPropertyBlock ledProperties;
        private Renderer appliedRenderer;
        private Color? appliedColor;
        public string PortId => portId;
        public void Configure(string id) => portId = id;
        public void SetLinkState(bool connected)
        {
            if (linkLed == null) return;
            Color color = connected && isActiveAndEnabled ? connectedColor : disconnectedColor;
            if (appliedRenderer == linkLed && appliedColor == color) return;
            ledProperties ??= new MaterialPropertyBlock();
            linkLed.GetPropertyBlock(ledProperties);
            ledProperties.SetColor("_BaseColor", color);
            ledProperties.SetColor("_Color", color);
            linkLed.SetPropertyBlock(ledProperties);
            appliedRenderer = linkLed; appliedColor = color;
        }
        private void OnEnable() { appliedColor = null; SetLinkState(false); }
        private void OnDisable() => SetLinkState(false);
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.025f);
        }
    }
}
