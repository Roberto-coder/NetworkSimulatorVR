#if UNITY_EDITOR
using Modules.Module02_RackInstallation.Interaction;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Module02TemplateNote))]
public sealed class Module02TemplateNoteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.HelpBox(serializedObject.FindProperty("instructions").stringValue, MessageType.Info);
    }
}
#endif
