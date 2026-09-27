using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Modules.Module03_Diagnostics.Editor
{
    [CustomEditor(typeof(FixedNetworkCable))]
    public sealed class FixedNetworkCableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Edita los knots con las herramientas de SplineContainer. Ajustar extremos conserva puntos intermedios. Reconstruye después de editar; no hay física.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Ajustar extremos a puertos y reconstruir")) Rebuild(true);
                if (GUILayout.Button("Reconstruir visual desde spline")) Rebuild(false);
            }
        }

        private void Rebuild(bool snap)
        {
            var route = (FixedNetworkCable)target;
            var container = route.GetComponent<SplineContainer>();
            if (container.Spline.Count < 2 || (snap && (route.EndpointA == null || route.EndpointB == null)))
            { Debug.LogError("Faltan knots o anclajes.", route); return; }
            Undo.RecordObject(container, "Ajustar ruta"); Undo.RecordObject(route.GetComponent<LineRenderer>(), "Reconstruir visual");
            if (snap)
            {
                var first = container.Spline[0]; var last = container.Spline[container.Spline.Count - 1];
                first.Position = route.transform.InverseTransformPoint(route.EndpointA.transform.position);
                last.Position = route.transform.InverseTransformPoint(route.EndpointB.transform.position);
                container.Spline[0] = first; container.Spline[container.Spline.Count - 1] = last;
            }
            route.Rebuild(); EditorUtility.SetDirty(container); EditorUtility.SetDirty(route.GetComponent<LineRenderer>());
        }
    }
}
