using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Probe visuals", fileName = "ProbeVisualSettings")]
    public sealed class ProbeVisualSettings : ScriptableObject
    {
        public Material overlayMaterial;
        [Min(0.1f)] public float metresPerSecond = 4;
        [Min(0.05f)] public float minimumStepSeconds = 0.12f;
        [Min(0.1f)] public float resultSeconds = 2;
        [Range(8, 128)] public int samples = 48;
        [Min(0.001f)] public float lineWidth = 0.012f;
        [Min(0.005f)] public float packetSize = 0.045f;
        [Min(0.05f)] public float failureSectionLength = 0.8f;
        [Min(0.001f)] public float failureLineWidth = 0.03f;
        public bool xRayInitially;
        public bool animateMotion = true;
        public Color arp = new Color(1, 0.7f, 0.15f);
        public Color request = new Color(0.1f, 0.6f, 1);
        public Color reply = new Color(0.15f, 1, 0.5f);
        public Color failure = new Color(1, 0.2f, 0.2f);
    }
}
