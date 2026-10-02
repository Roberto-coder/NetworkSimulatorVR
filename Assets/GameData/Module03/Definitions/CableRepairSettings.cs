using Modules.Module03_Diagnostics.Domain;
using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Cable repair", fileName = "CableRepairSettings")]
    public sealed class CableRepairSettings : ScriptableObject
    {
        [Tooltip("Longitud nominal del patch cord, en metros (1 unidad Unity = 1 m).") ]
        [Min(0.1f)] public float patchCordLength = 4f;
        [Range(0, 0.25f)] public float maximumStretchFraction = 0.08f;
        [Min(0)] public float springDamping = 5f;
        public CableRepairDefinition incident = new();
        public GameObject patchCordPrefab;
        public GameObject socketPrefab;
        public GameObject testerPrefab, labelMakerPrefab;
        [Tooltip("Legado: el tester XR utiliza snaps, no este radio de proximidad.")]
        [Min(0.03f)] public float testerRadius = 0.3f;
        [Min(0.05f)] public float testerLedSeconds = 0.15f;
        public Color testerLedOn = Color.green;
        public Color testerLedOff = Color.black;
        [Min(0.01f)] public float labelRadius = 0.12f;
    }
}
