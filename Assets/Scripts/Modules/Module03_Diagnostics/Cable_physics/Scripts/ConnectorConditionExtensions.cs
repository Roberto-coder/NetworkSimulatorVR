

namespace Modules.Module03_Diagnostics.Cable_physics.Scripts
{
    public static class ConnectorConditionExtensions
    {
        public static bool CanConnectConditioned(this Connector self, Connector target)
        {
            var selfState = self.GetComponent<CableEndState>();
            var targetState = target.GetComponent<CableEndState>();

            bool selfReady = selfState == null || selfState.Condition == CableCondition.Tested;
            bool targetReady = targetState == null || targetState.Condition == CableCondition.Tested;

            return selfReady && targetReady && self.CanConnect(target);
        }
    }
}