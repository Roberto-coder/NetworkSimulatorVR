using Shared.Cabling;
using TMPro;
using UnityEngine;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Identidad estable de un cable del inventario. Start/End equivalen a extremos A/B.</summary>
    [RequireComponent(typeof(PatchCableLink))]
    public sealed class RepairPatchCord : MonoBehaviour
    {
        public string cableId;
        public Connector endA, endB;
        public TMP_Text labelA, labelB;
        public PatchCableLink Link => GetComponent<PatchCableLink>();
    }
}
