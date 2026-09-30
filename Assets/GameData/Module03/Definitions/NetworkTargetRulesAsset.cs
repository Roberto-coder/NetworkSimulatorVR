using Modules.Module03_Diagnostics.Domain;
using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Target Rules", fileName = "OfficeTargetRules")]
    public sealed class NetworkTargetRulesAsset : ScriptableObject
    {
        [SerializeField] private NetworkTargetDefinition definition = new();
        // El inventario documentado parte de la topología y toma las IP esperadas de las reglas.
        // Nunca modifica el estado inicial ni elimina los incidentes de arranque.
        public NetworkDefinition DocumentedInventory(NetworkInitialStateAsset initial)
        {
            var copy = initial.CopyDefinition();
            foreach (var rule in definition.rules)
            {
                if (rule.kind != NetworkRuleKind.Address) continue;
                var port = copy.ports.Find(p => p.id == rule.portA);
                if (port == null) throw new System.InvalidOperationException("Regla con puerto inexistente: " + rule.portA);
                port.ipv4 = rule.ipv4; port.prefixLength = rule.prefixLength;
            }
            return copy;
        }
        public System.Collections.Generic.IReadOnlyList<NetworkRuleResult> Evaluate(NetworkSession session) =>
            NetworkTargetValidator.Evaluate(session, definition);
        public System.Collections.Generic.IReadOnlyList<string> ValidateAgainst(NetworkInitialStateAsset initial) =>
            NetworkTargetValidator.ValidateConfiguration(initial.CopyDefinition(), definition);
    }
}
