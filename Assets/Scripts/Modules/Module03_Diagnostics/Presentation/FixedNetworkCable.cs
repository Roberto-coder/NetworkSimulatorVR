using System;
using UnityEngine;
using UnityEngine.Splines;
using Modules.Module03_Diagnostics.Interaction;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Ruta fija muestreada por distancia. No usa física ni decide conectividad.</summary>
    [RequireComponent(typeof(SplineContainer), typeof(LineRenderer))]
    public sealed class FixedNetworkCable : MonoBehaviour
    {
        [SerializeField] private string cableId;
        [SerializeField] private NetworkPortAnchor endpointA, endpointB;
        [SerializeField, Range(8, 256)] private int samples = 64;
        private Vector3[] points;
        private float[] distances;
        public string CableId => cableId;
        public NetworkPortAnchor EndpointA => endpointA;
        public NetworkPortAnchor EndpointB => endpointB;
        public float Length => distances == null ? 0 : distances[distances.Length - 1];

        public void Configure(string id, NetworkPortAnchor a, NetworkPortAnchor b, int sampleCount)
        { cableId = id; endpointA = a; endpointB = b; samples = Mathf.Clamp(sampleCount, 8, 256); }

        private void Awake() => Rebuild();

        // Explícito en Editor, una vez al iniciar en Play. No reconstruimos cada frame.
        public void Rebuild()
        {
            var container = GetComponent<SplineContainer>();
            if (container.Spline == null || container.Spline.Count < 2)
                throw new InvalidOperationException($"Ruta incompleta: {cableId}");
            points = new Vector3[samples + 1]; distances = new float[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                // Cache local: mover la raíz conserva la validez del recorrido con escala unitaria.
                points[i] = transform.InverseTransformPoint((Vector3)container.EvaluatePosition(i / (float)samples));
                if (i > 0) distances[i] = distances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
            var line = GetComponent<LineRenderer>();
            line.useWorldSpace = false; line.positionCount = points.Length; line.SetPositions(points);
            // Extrude aporta volumen al cable. El LineRenderer queda como representación de respaldo.
            var extrude = GetComponentInChildren<SplineExtrude>();
            if (extrude != null) { extrude.Rebuild(); line.enabled = false; }
        }

        public Vector3 EvaluateDistance(float distance)
        {
            if (points == null) Rebuild();
            distance = Mathf.Clamp(distance, 0, Length);
            int index = Array.BinarySearch(distances, distance);
            if (index >= 0) return transform.TransformPoint(points[index]);
            index = ~index;
            if (index == 0) return transform.TransformPoint(points[0]);
            float span = distances[index] - distances[index - 1];
            return transform.TransformPoint(Vector3.Lerp(points[index - 1], points[index],
                span > 0 ? (distance - distances[index - 1]) / span : 0));
        }
    }
}
