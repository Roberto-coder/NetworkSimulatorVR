using UnityEngine;

namespace GameData.Module03
{
    [System.Serializable]
    public sealed class DiagnosticMapNode
    {
        public string deviceId;
        public string displayName;
        public Sprite sprite;
        public Vector2 position;
    }

    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Diagnostic UI", fileName = "DiagnosticUiSettings")]
    public sealed class DiagnosticUiSettings : ScriptableObject
    {
        // Presentación del mapa: independiente de las coordenadas físicas de la oficina.
        public System.Collections.Generic.List<DiagnosticMapNode> mapNodes = new();
        public string laptopDeviceId = "Laptop";
        public string sourcePortId = "Laptop/eth0";
        [Min(0.5f)] public float interactionRange = 2.5f;
        [Min(0.0001f)] public float panelScale = 0.0012f;
        public string focusHint = "A · Abrir diagnóstico";
        public string mapTitle = "Topología documentada · selecciona destino";
        public string[] commands = { "Ping", "ARP", "Configuración local", "Puertos del switch", "Bitácora", "Cerrar" };
    }
}
