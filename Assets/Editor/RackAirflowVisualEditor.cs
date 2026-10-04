using Modules.Module02_RackInstallation.Presentation;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RackAirflowVisual))]
public sealed class RackAirflowVisualEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            ((RackAirflowVisual)target).RebuildVisual();
        EditorGUILayout.HelpBox("Mueve los puntos en Scene. Aumenta Path Points para extender la salida. Las coordenadas son locales a este objeto.", MessageType.Info);
    }

    private void OnSceneGUI()
    {
        var effect = (RackAirflowVisual)target;
        serializedObject.Update();
        SerializedProperty points = serializedObject.FindProperty("pathPoints");
        for (int i = 0; i < points.arraySize; i++)
        {
            SerializedProperty point = points.GetArrayElementAtIndex(i);
            Vector3 world = effect.transform.TransformPoint(point.vector3Value);
            Handles.Label(world, i == 0 ? "Entrada" : i == points.arraySize - 1 ? "Salida" : "Flujo " + i);
            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(world, effect.transform.rotation);
            if (!EditorGUI.EndChangeCheck()) continue;
            point.vector3Value = effect.transform.InverseTransformPoint(moved);
            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying) effect.RebuildVisual();
        }
    }
}
