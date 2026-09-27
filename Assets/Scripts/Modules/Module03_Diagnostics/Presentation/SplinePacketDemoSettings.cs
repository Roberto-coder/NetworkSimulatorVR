using UnityEngine;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>
    /// Configuración editable guardada como asset en Assets/GameData/Module03.
    /// No ejecuta animación ni almacena el progreso de un ping.
    /// </summary>
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Spline demo settings")]
    public sealed class SplinePacketDemoSettings : ScriptableObject
    {
        // Velocidad visual en unidades de mundo por segundo (metros con escala 1:1).
        [Min(0.1f)] public float packetSpeed = 2f;
        // Dimensiones de la línea y de las esferas que representan paquetes.
        [Min(0.02f)] public float cableWidth = 0.06f;
        [Min(0.04f)] public float packetDiameter = 0.22f;
        // Más segmentos aproximan mejor la curva y su longitud, a mayor costo.
        [Range(32, 512)] public int samples = 192;
        // Colores independientes para cable, solicitud, respuesta e interrupción.
        public Color cableColor = new Color(0.12f, 0.4f, 0.55f);
        public Color requestColor = Color.cyan;
        public Color replyColor = Color.green;
        public Color failureColor = new Color(1f, 0.2f, 0.15f);
    }
}
