using UnityEngine;

namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Administration", fileName = "NetworkAdministrationSettings")]
    public sealed class NetworkAdministrationSettings : ScriptableObject
    {
        public NetworkTargetRulesAsset targetRules;
        [Tooltip("Opciones didácticas; pueden incluir direcciones incorrectas para diagnosticar.")]
        public string[] addressOptions = { "192.168.10.11", "192.168.10.12", "192.168.10.13", "192.168.10.14" };
        [Tooltip("Reglas de los incidentes de puerto e IP; no incluyen la reparación de cable.")]
        public string[] requiredRuleIds = { "enabled-P02", "address-PC-02", "address-PC-03", "unique-addresses" };
        public string[] verificationDeviceIds = { "PC-02", "PC-03" };
    }
}
