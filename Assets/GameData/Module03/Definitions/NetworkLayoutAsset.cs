using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData.Module03
{
    [Serializable]
    public sealed class DevicePlacement
    {
        public string deviceId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public Vector2 mapPosition;
    }

    [Serializable]
    public sealed class PortPlacement
    {
        public string portId;
        public Vector3 localPosition;
        public Vector3 localRotation;
    }

    [Serializable]
    public sealed class FixedRouteLayout
    {
        public string cableId;
        // Coordenadas locales a la raíz del módulo; no dependen del origen de la escena.
        public List<UnityEngine.Splines.BezierKnot> knots = new();
    }

    [CreateAssetMenu(menuName = "Network Simulator/Module 03/Layout", fileName = "OfficeLayout")]
    public sealed class NetworkLayoutAsset : ScriptableObject
    {
        public string scenarioId = "office";
        public List<DevicePlacement> devices = new();
        public List<PortPlacement> ports = new();
        public List<FixedRouteLayout> fixedRoutes = new();
        [Min(0.001f)] public float cableRadius = 0.008f;
        [Range(8, 256)] public int routeSamples = 64;
        public Material cableMaterial;
    }
}
