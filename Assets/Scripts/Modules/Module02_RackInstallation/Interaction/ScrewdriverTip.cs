using Framework.Interaction.Tools;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>Only the equipped screwdriver's small tip can advance a screw.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ScrewdriverTip : MonoBehaviour
    {
        private Tool tool;
        public bool IsUsable => isActiveAndEnabled && tool != null && tool.Type == ToolType.Screwdriver;
        private void Awake() => tool = GetComponentInParent<Tool>();
    }
}
