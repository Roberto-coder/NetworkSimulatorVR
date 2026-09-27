using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData.Module03
{
    [Serializable]
    public sealed class NetworkPrefabEntry
    {
        public string key;
        public GameObject prefab;
    }

    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Prefab Catalog", fileName = "OfficePrefabCatalog")]
    public sealed class NetworkPrefabCatalog : ScriptableObject
    {
        // Solo se usa al crear un dispositivo ausente; nunca reemplaza objetos ya asociados.
        public List<NetworkPrefabEntry> entries = new();
    }
}
