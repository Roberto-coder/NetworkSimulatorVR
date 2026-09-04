#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mide la geometría y los colliders de una jerarquía usando metros como unidad física.
/// Las dimensiones se expresan en los ejes del objeto raíz, no en el AABB global.
/// </summary>
public sealed class VRDimensionInspector : EditorWindow
{
    private const float MetresToCentimetres = 100f;
    private const float MetresToInches = 39.37007874f;
    private const float RackUnitMetres = 0.04445f;

    private GameObject target;
    private bool includeInactive = true;
    private Vector2 scroll;

    [MenuItem("Network Simulator/Tools/VR Dimension Inspector")]
    private static void Open()
    {
        var window = GetWindow<VRDimensionInspector>();
        window.titleContent = new GUIContent("VR Dimensions");
        window.minSize = new Vector2(430f, 430f);
        window.SyncSelection();
        window.Show();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        SyncSelection();
    }

    private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

    private void OnSelectionChanged()
    {
        SyncSelection();
        Repaint();
    }

    private void SyncSelection()
    {
        if (Selection.activeGameObject != null)
            target = Selection.activeGameObject;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Medición física para VR", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Unity usa metros: 1 unidad mundial = 1 m. Selecciona una instancia de escena o un prefab. " +
            "Las medidas se calculan en los ejes del objeto seleccionado.", MessageType.Info);

        target = (GameObject)EditorGUILayout.ObjectField("Objeto", target, typeof(GameObject), true);
        includeInactive = EditorGUILayout.Toggle("Incluir hijos inactivos", includeInactive);

        if (target == null)
        {
            EditorGUILayout.HelpBox("Selecciona el objeto raíz que deseas medir.", MessageType.Warning);
            return;
        }

        Measurement visible = MeasureRenderers(target.transform, includeInactive);
        Measurement colliders = MeasureColliders(target.transform, includeInactive);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawTransformWarnings(target.transform);
        DrawMeasurement("Geometría visible", visible, "Medida final que percibirá el usuario.");
        DrawMeasurement("Colliders", colliders, "Volumen utilizado para contacto, ray y agarre.");
        DrawComparison(visible, colliders);
        DrawReferences();
        EditorGUILayout.EndScrollView();

        using (new EditorGUI.DisabledScope(!visible.IsValid && !colliders.IsValid))
        {
            if (GUILayout.Button("Copiar reporte"))
            {
                EditorGUIUtility.systemCopyBuffer = BuildReport(target, visible, colliders);
                ShowNotification(new GUIContent("Reporte copiado"));
            }
        }
    }

    private static void DrawTransformWarnings(Transform root)
    {
        Vector3 scale = root.lossyScale;
        bool rootIsOne = Approximately(root.localScale, Vector3.one);
        bool nonUniform = !Mathf.Approximately(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) ||
                          !Mathf.Approximately(Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        if (!rootIsOne)
            EditorGUILayout.HelpBox(
                $"La raíz tiene escala local {FormatVector(root.localScale)}. Para prefabs físicos se recomienda " +
                "raíz (1, 1, 1) y corregir la escala dentro de un hijo Visuals.", MessageType.Warning);

        if (nonUniform)
            EditorGUILayout.HelpBox(
                $"La escala mundial es no uniforme: {FormatVector(scale)}. Puede deformar colliders y complicar la física.",
                MessageType.Warning);
    }

    private static void DrawMeasurement(string title, Measurement measurement, string description)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(description, EditorStyles.miniLabel);

        if (!measurement.IsValid)
        {
            EditorGUILayout.HelpBox("No se encontraron elementos medibles en esta jerarquía.", MessageType.None);
            return;
        }

        Vector3 metres = measurement.Size;
        DrawRow("Metros", metres, "m");
        DrawRow("Centímetros", metres * MetresToCentimetres, "cm");
        DrawRow("Pulgadas", metres * MetresToInches, "in");

        float rackUnits = metres.y / RackUnitMetres;
        int closest = Mathf.Max(1, Mathf.RoundToInt(rackUnits));
        float expectedHeight = closest * RackUnitMetres;
        float errorMillimetres = Mathf.Abs(metres.y - expectedHeight) * 1000f;
        EditorGUILayout.LabelField("Altura de rack", $"{rackUnits:0.###} U  (más próximo: {closest}U, diferencia {errorMillimetres:0.##} mm)");
        EditorGUILayout.LabelField("Elementos incluidos", measurement.Count.ToString(CultureInfo.InvariantCulture));

        if (errorMillimetres <= 1f)
            EditorGUILayout.HelpBox($"La altura coincide aproximadamente con {closest}U.", MessageType.Info);
    }

    private static void DrawComparison(Measurement visible, Measurement colliders)
    {
        if (!visible.IsValid || !colliders.IsValid)
            return;

        Vector3 differenceCm = (colliders.Size - visible.Size) * MetresToCentimetres;
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Diferencia collider − visual", EditorStyles.boldLabel);
        DrawRow("Centímetros", differenceCm, "cm");

        Vector3 visibleSize = visible.Size;
        Vector3 colliderSize = colliders.Size;
        bool excessive = IsExcessive(visibleSize.x, colliderSize.x) ||
                         IsExcessive(visibleSize.y, colliderSize.y) ||
                         IsExcessive(visibleSize.z, colliderSize.z);
        if (excessive)
            EditorGUILayout.HelpBox(
                "Algún eje del collider difiere más de 10% respecto a la geometría visible. Revisa las zonas de interacción.",
                MessageType.Warning);
    }

    private static void DrawReferences()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Referencias rápidas", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1U", "0.04445 m  |  4.445 cm  |  1.75 in");
        EditorGUILayout.LabelField("2U", "0.08890 m  |  8.890 cm  |  3.50 in");
        EditorGUILayout.LabelField("Frente nominal 19 in", "0.48260 m  |  48.260 cm  |  19.00 in");
        EditorGUILayout.LabelField("Rack 22U (espacio útil)", "0.97790 m  |  97.790 cm  |  38.50 in");
    }

    private static void DrawRow(string label, Vector3 value, string suffix)
    {
        EditorGUILayout.LabelField(label,
            $"X {value.x:0.#####} {suffix}   Y {value.y:0.#####} {suffix}   Z {value.z:0.#####} {suffix}");
    }

    private static Measurement MeasureRenderers(Transform root, bool includeInactive)
    {
        var result = new Measurement();
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive))
        {
            if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
                continue;
            EncapsulateLocalBounds(root, renderer.transform, renderer.localBounds, ref result);
        }
        return result;
    }

    private static Measurement MeasureColliders(Transform root, bool includeInactive)
    {
        var result = new Measurement();
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(includeInactive))
        {
            if (!TryGetColliderLocalBounds(collider, out Bounds bounds))
                continue;
            EncapsulateLocalBounds(root, collider.transform, bounds, ref result);
        }
        return result;
    }

    private static bool TryGetColliderLocalBounds(Collider collider, out Bounds bounds)
    {
        switch (collider)
        {
            case BoxCollider box:
                bounds = new Bounds(box.center, box.size);
                return true;
            case SphereCollider sphere:
                bounds = new Bounds(sphere.center, Vector3.one * sphere.radius * 2f);
                return true;
            case CapsuleCollider capsule:
                Vector3 size = Vector3.one * capsule.radius * 2f;
                size[capsule.direction] = capsule.height;
                bounds = new Bounds(capsule.center, size);
                return true;
            case MeshCollider mesh when mesh.sharedMesh != null:
                bounds = mesh.sharedMesh.bounds;
                return true;
            default:
                bounds = default;
                return false;
        }
    }

    private static void EncapsulateLocalBounds(Transform root, Transform source, Bounds localBounds, ref Measurement result)
    {
        Vector3 min = localBounds.min;
        Vector3 max = localBounds.max;
        Quaternion inverseRootRotation = Quaternion.Inverse(root.rotation);

        for (int x = 0; x < 2; x++)
        for (int y = 0; y < 2; y++)
        for (int z = 0; z < 2; z++)
        {
            Vector3 localCorner = new(
                x == 0 ? min.x : max.x,
                y == 0 ? min.y : max.y,
                z == 0 ? min.z : max.z);
            Vector3 worldCorner = source.TransformPoint(localCorner);
            Vector3 rootOrientedPoint = inverseRootRotation * (worldCorner - root.position);
            result.Encapsulate(rootOrientedPoint);
        }

        result.Count++;
    }

    private static string BuildReport(GameObject selected, Measurement visible, Measurement colliders)
    {
        var text = new StringBuilder();
        text.AppendLine($"VR Dimension Inspector — {selected.name}");
        AppendMeasurement(text, "Geometría visible", visible);
        AppendMeasurement(text, "Colliders", colliders);
        return text.ToString();
    }

    private static void AppendMeasurement(StringBuilder text, string label, Measurement measurement)
    {
        if (!measurement.IsValid)
        {
            text.AppendLine($"{label}: sin elementos medibles");
            return;
        }

        Vector3 size = measurement.Size;
        text.AppendLine(label + ":");
        text.AppendLine($"  m:  {FormatVector(size)}");
        text.AppendLine($"  cm: {FormatVector(size * MetresToCentimetres)}");
        text.AppendLine($"  in: {FormatVector(size * MetresToInches)}");
        text.AppendLine($"  altura: {size.y / RackUnitMetres:0.###} U");
    }

    private static bool IsExcessive(float visual, float collider) =>
        visual > 0.00001f && Mathf.Abs(collider - visual) / visual > 0.1f;

    private static bool Approximately(Vector3 a, Vector3 b) =>
        Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);

    private static string FormatVector(Vector3 value) =>
        $"({value.x:0.#####}, {value.y:0.#####}, {value.z:0.#####})";

    private struct Measurement
    {
        private Bounds bounds;
        public int Count;
        public bool IsValid { get; private set; }
        public Vector3 Size => IsValid ? bounds.size : Vector3.zero;

        public void Encapsulate(Vector3 point)
        {
            if (!IsValid)
            {
                bounds = new Bounds(point, Vector3.zero);
                IsValid = true;
                return;
            }
            bounds.Encapsulate(point);
        }
    }
}
#endif
