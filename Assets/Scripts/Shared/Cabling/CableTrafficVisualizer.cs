using System.Collections.Generic;
using HPhysic;
using UnityEngine;

namespace Shared.Cabling
{
    /// <summary>
    /// Representación didáctica de paquetes desplazándose por la forma actual del cable.
    /// Las esferas no representan electrones ni participan en la física del patch cord.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PatchCableLink))]
    public sealed class CableTrafficVisualizer : MonoBehaviour
    {
        [SerializeField] private PhysicCable physicalCable;
        [SerializeField] private PatchCableLink link;
        [SerializeField, Min(1)] private int poolSize = 8;
        [SerializeField, Min(0.001f)] private float packetDiameter = 0.018f;
        [SerializeField, Min(0.05f)] private float speed = 0.8f;
        [SerializeField, Min(0.01f)] private float packetSpacing = 0.12f;
        [SerializeField] private Color packetColor = new(0.1f, 0.75f, 1f);
        [SerializeField] private bool emitDemoTraffic;
        [SerializeField, Min(0.1f)] private float demoInterval = 1.5f;

        // El pool evita Instantiate/Destroy durante cada transmisión y reduce pausas en VR.
        private readonly List<Packet> packets = new();
        private Material packetMaterial;
        private float nextDemoTime;

        private sealed class Packet
        {
            public Transform Transform;
            public float Distance;
            public bool Reverse;
            public bool Active;
        }

        private void Awake()
        {
            physicalCable ??= GetComponent<PhysicCable>();
            link ??= GetComponent<PatchCableLink>();
            BuildPool();
        }

        private void Update()
        {
            if (emitDemoTraffic && link != null && link.HasCompleteLink && Time.time >= nextDemoTime)
            {
                Transmit(3, false);
                nextDemoTime = Time.time + demoInterval;
            }
            UpdatePackets();
        }

        public void Transmit(int packetCount = 3, bool reverse = false)
        {
            // No mostramos tráfico cuando uno de los extremos está desconectado.
            if (link == null || !link.HasCompleteLink)
                return;

            int emitted = 0;
            foreach (Packet packet in packets)
            {
                if (packet.Active)
                    continue;
                packet.Active = true;
                packet.Reverse = reverse;
                packet.Distance = -emitted * packetSpacing;
                packet.Transform.gameObject.SetActive(true);
                emitted++;
                if (emitted >= packetCount)
                    break;
            }
        }

        private void UpdatePackets()
        {
            if (physicalCable == null || physicalCable.Points == null || physicalCable.Points.Count < 2)
                return;

            float length = CalculateLength();
            foreach (Packet packet in packets)
            {
                if (!packet.Active)
                    continue;
                packet.Distance += speed * Time.deltaTime;
                if (packet.Distance < 0f)
                    continue;
                if (packet.Distance > length || link == null || !link.HasCompleteLink)
                {
                    Deactivate(packet);
                    continue;
                }
                float pathDistance = packet.Reverse ? length - packet.Distance : packet.Distance;
                packet.Transform.position = Evaluate(pathDistance);
            }
        }

        private float CalculateLength()
        {
            float result = 0f;
            for (int i = 1; i < physicalCable.Points.Count; i++)
                result += Vector3.Distance(physicalCable.Points[i - 1].position, physicalCable.Points[i].position);
            return result;
        }

        private Vector3 Evaluate(float distance)
        {
            // Recorre la polilínea formada por los rigidbodies y encuentra el punto
            // correspondiente a una distancia acumulada desde el inicio del cable.
            for (int i = 1; i < physicalCable.Points.Count; i++)
            {
                Vector3 a = physicalCable.Points[i - 1].position;
                Vector3 b = physicalCable.Points[i].position;
                float segment = Vector3.Distance(a, b);
                if (distance <= segment)
                    return Vector3.Lerp(a, b, segment > 0f ? distance / segment : 0f);
                distance -= segment;
            }
            return physicalCable.Points[^1].position;
        }

        private void BuildPool()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            packetMaterial = new Material(shader) { color = packetColor };
            packetMaterial.EnableKeyword("_EMISSION");
            packetMaterial.SetColor("_EmissionColor", packetColor * 1.5f);

            var root = new GameObject("TrafficPackets").transform;
            root.SetParent(transform, false);
            for (int i = 0; i < poolSize; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"Packet_{i + 1:00}";
                sphere.transform.SetParent(root, false);
                sphere.transform.localScale = Vector3.one * packetDiameter;
                Destroy(sphere.GetComponent<Collider>());
                sphere.GetComponent<Renderer>().sharedMaterial = packetMaterial;
                sphere.SetActive(false);
                packets.Add(new Packet { Transform = sphere.transform });
            }
        }

        private static void Deactivate(Packet packet)
        {
            packet.Active = false;
            packet.Transform.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (packetMaterial != null)
                Destroy(packetMaterial);
        }
    }
}
