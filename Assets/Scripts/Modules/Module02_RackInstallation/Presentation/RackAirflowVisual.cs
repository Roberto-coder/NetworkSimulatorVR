using System.Collections.Generic;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Objectives;
using UnityEngine;
using UnityEngine.Rendering;

namespace Modules.Module02_RackInstallation.Presentation
{
    public sealed class RackAirflowVisual : MonoBehaviour
    {
        [Tooltip("Puntos locales de la trayectoria, desde la entrada hasta la salida.")]
        [SerializeField] private Vector3[] pathPoints = {
            new Vector3(0, 0.15f, -0.7f), new Vector3(0, 0.25f, 0),
            new Vector3(0, 1f, 0), new Vector3(0, 1.8f, 0),
            new Vector3(0, 2.1f, 0.25f), new Vector3(0, 2.15f, 0.85f)
        };
        [SerializeField, Range(2, 16)] private int segmentsPerCurve = 6;
        [SerializeField, Range(1, 5)] private int laneCount = 3;
        [SerializeField, Min(0)] private float laneSpacing = 0.18f;
        [SerializeField] private Material airflowMaterial;
        [SerializeField] private bool onlyDuringRackInspection = true;
        [Tooltip("Invierte el sentido del flujo sobre la trayectoria.")]
        [SerializeField] private bool reverseDirection;
        [SerializeField, Min(0.01f)] private float ribbonWidth = 0.07f;

        private GameObject visual;
        private Mesh mesh;
        private Module02FlowController flow;

        private void Start()
        {
            BuildVisual();
            BindFlow();
        }

        private void OnEnable()
        {
            if (visual != null) BindFlow();
        }

        private void BindFlow()
        {
            flow = Module02Manager.Instance?.FlowController;
            if (flow != null)
            {
                flow.CurrentObjectiveChanged -= Refresh;
                flow.CurrentObjectiveChanged += Refresh;
            }
            Refresh(flow?.CurrentObjectiveData);
        }

        private void Refresh(ObjectiveData objective)
        {
            if (visual != null)
                visual.SetActive(!onlyDuringRackInspection || objective?.Id == Module02ObjectiveCatalog.InspectRack);
        }

        private void OnDisable()
        {
            if (flow != null) flow.CurrentObjectiveChanged -= Refresh;
            flow = null;
            if (visual != null) visual.SetActive(false);
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (visual != null) Destroy(visual);
        }

        [ContextMenu("Rebuild Airflow")]
        public void RebuildVisual()
        {
            if (!Application.isPlaying) return;
            if (visual != null) Destroy(visual);
            if (mesh != null) Destroy(mesh);
            BuildVisual();
            Refresh(flow?.CurrentObjectiveData);
        }

        private Vector3 Sample(int segment, float t)
        {
            Vector3 a = pathPoints[Mathf.Max(0, segment - 1)];
            Vector3 b = pathPoints[segment];
            Vector3 c = pathPoints[segment + 1];
            Vector3 d = pathPoints[Mathf.Min(pathPoints.Length - 1, segment + 2)];
            return 0.5f * ((2f * b) + (-a + c) * t +
                (2f * a - 5f * b + 4f * c - d) * t * t +
                (-a + 3f * b - 3f * c + d) * t * t * t);
        }

        private List<Vector3> GetSamples()
        {
            var points = new List<Vector3>();
            if (pathPoints == null || pathPoints.Length < 2) return points;
            int steps = Mathf.Clamp(segmentsPerCurve, 2, 16);
            for (int segment = 0; segment < pathPoints.Length - 1; segment++)
                for (int step = 0; step < steps; step++)
                    points.Add(Sample(segment, step / (float)steps));
            points.Add(pathPoints[pathPoints.Length - 1]);
            if (reverseDirection) points.Reverse();
            return points;
        }

        private void BuildVisual()
        {
            if (airflowMaterial == null) return;
            List<Vector3> points = GetSamples();
            if (points.Count < 2) return;
            var distances = new float[points.Count];
            for (int i = 1; i < points.Count; i++)
                distances[i] = distances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            float length = distances[points.Count - 1];
            if (length < 0.001f) return;
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();
            var indices = new List<int>();
            int lanes = Mathf.Clamp(laneCount, 1, 5);
            for (int lane = 0; lane < lanes; lane++)
            {
                Vector3 offset = Vector3.right * ((lane - (lanes - 1) * 0.5f) * laneSpacing);
                Vector3 side = Vector3.right;
                for (int plane = 0; plane < 2; plane++)
                {
                    int first = vertices.Count;
                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector3 tangent = (points[Mathf.Min(i + 1, points.Count - 1)] -
                            points[Mathf.Max(0, i - 1)]).normalized;
                        if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.up;
                        side = Vector3.ProjectOnPlane(side, tangent).normalized;
                        if (side.sqrMagnitude < 0.001f)
                            side = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.forward).normalized;
                        Vector3 axis = plane == 0 ? side : Vector3.Cross(tangent, side).normalized;
                        Vector3 half = axis * Mathf.Max(0.01f, ribbonWidth) * 0.5f;
                        float progress = distances[i] / length;
                        Color color = Color.Lerp(new Color(0.1f, 0.7f, 1f), new Color(1f, 0.35f, 0.08f), progress);
                        vertices.Add(points[i] + offset - half); vertices.Add(points[i] + offset + half);
                        uv.Add(new Vector2(progress, 0)); uv.Add(new Vector2(progress, 1));
                        colors.Add(color); colors.Add(color);
                        if (i == 0) continue;
                        int n = first + (i - 1) * 2;
                        indices.AddRange(new[] { n, n + 1, n + 2, n + 2, n + 1, n + 3 });
                    }
                }
            }
            mesh = new Mesh { name = "Curved rack airflow", indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0); mesh.RecalculateBounds();
            visual = new GameObject("Airflow_Provisional");
            visual.transform.SetParent(transform, false);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            var output = visual.AddComponent<MeshRenderer>();
            output.sharedMaterial = airflowMaterial;
            output.shadowCastingMode = ShadowCastingMode.Off;
            output.receiveShadows = false;
            output.lightProbeUsage = LightProbeUsage.Off;
            output.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void OnDrawGizmosSelected()
        {
            List<Vector3> points = GetSamples();
            Gizmos.color = Color.cyan;
            for (int i = 1; i < points.Count; i++)
                Gizmos.DrawLine(transform.TransformPoint(points[i - 1]), transform.TransformPoint(points[i]));
        }
    }
}