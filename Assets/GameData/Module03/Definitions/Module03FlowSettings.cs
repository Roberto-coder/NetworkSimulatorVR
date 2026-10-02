using System;
using GameData.Modules;
using GameData.NPC;
using UnityEngine;
namespace GameData.Module03
{
    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Guided flow")]
    public sealed class Module03FlowSettings : ScriptableObject
    {
        public ModuleDefinition module;
        public NetworkTargetRulesAsset targets;
        public string sourcePort = "Laptop/eth0";
        public string switchId = "SW-01";
        public string disabledPort = "SW-01/P02";
        public string[] addressRuleIds = { "address-PC-02", "address-PC-03", "unique-addresses" };
        public string[] addressDevices = { "PC-02", "PC-03" };
        public string[] finalDevices = { "PC-01", "PC-02", "PC-03" };
        public NPCDialogueData tutorial;
        public NPCDialogueLine Find(string id) => tutorial != null ? tutorial.Find(id) : null;
    }
}
