using Modules.Module03_Diagnostics.Domain;
using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Initial State", fileName = "OfficeInitialState")]
    public sealed class NetworkInitialStateAsset : ScriptableObject
    {
        [SerializeField] private NetworkDefinition definition = new();
        // El asset contiene datos de autoría. Cada llamada crea una sesión aislada.
        public NetworkSession CreateSession() => new NetworkSession(definition);
        public NetworkDefinition CopyDefinition()
        {
            var errors = NetworkDefinitionValidator.Validate(definition);
            if (errors.Count > 0) throw new System.ArgumentException(string.Join("\n", errors));
            return definition.Copy();
        }
    }
}
