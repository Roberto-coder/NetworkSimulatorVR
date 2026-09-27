using Modules.Module03_Diagnostics.Domain;
using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Target Rules", fileName = "OfficeTargetRules")]
    public sealed class NetworkTargetRulesAsset : ScriptableObject
    {
        [SerializeField] private NetworkTargetDefinition definition = new();
        public System.Collections.Generic.IReadOnlyList<NetworkRuleResult> Evaluate(NetworkSession session) =>
            NetworkTargetValidator.Evaluate(session, definition);
        public System.Collections.Generic.IReadOnlyList<string> ValidateAgainst(NetworkInitialStateAsset initial) =>
            NetworkTargetValidator.ValidateConfiguration(initial.CopyDefinition(), definition);
    }
}
