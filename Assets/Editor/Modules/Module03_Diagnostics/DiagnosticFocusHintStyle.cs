using TMPro;
using UnityEditor;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    public static class DiagnosticFocusHintStyle
    {
        public static TMP_Text Instantiate(Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Dispositivos/Modulo3/DiagnosticFocusHint.prefab");
            if (prefab == null) throw new System.InvalidOperationException("Falta el prefab DiagnosticFocusHint.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            return instance.GetComponentInChildren<TMP_Text>(true);
        }
    }
}
