using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Zona física de lectura local. Elegir un nodo 2D no activa este componente.</summary>
    public sealed class DiagnosticInteractionTarget : MonoBehaviour
    {
        public NetworkDeviceBinding device;
        public Modules.Module03_Diagnostics.Presentation.DiagnosticScreenView screen;
        public TMPro.TMP_Text focusHint;
        public Collider hitZone;
        public Vector3 InteractionPosition => hitZone != null ? hitZone.bounds.center : transform.position;
        public string DeviceId => device != null ? device.DeviceId : null;
    }
}
