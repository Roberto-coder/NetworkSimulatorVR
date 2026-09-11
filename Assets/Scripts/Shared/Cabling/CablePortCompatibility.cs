using Modules.Module03_Diagnostics.Cable_physics.Scripts;

namespace Shared.Cabling
{
    public static class CablePortCompatibility
    {
        public static bool Allows(Connector first, Connector second)
        {
            if (first == null || second == null) return false;
            var firstPort = first.GetComponentInParent<NetworkPort>();
            var secondPort = second.GetComponentInParent<NetworkPort>();
            var firstCable = first.CableOwner != null ? first.CableOwner : first.GetComponentInParent<PatchCableLink>();
            var secondCable = second.CableOwner != null ? second.CableOwner : second.GetComponentInParent<PatchCableLink>();
            // Legacy cables without port metadata keep their existing rules.
            if (firstPort == null && secondPort == null && firstCable == null && secondCable == null) return true;
            return Matches(firstPort, secondCable) || Matches(secondPort, firstCable);
        }

        private static bool Matches(NetworkPort port, PatchCableLink cable) =>
            port != null && cable != null && port.isActiveAndEnabled && cable.isActiveAndEnabled && port.Kind == cable.Kind;
    }
}
