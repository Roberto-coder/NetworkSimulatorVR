using UnityEngine;

namespace Modules.Module02_RackInstallation.Presentation
{
    /// <summary>Explicit editor tool; saved transforms are never fitted on enable or on entering Play.</summary>
    [DisallowMultipleComponent]
    public sealed class RackStaticVisualFit : MonoBehaviour
    {
        [SerializeField] private Transform visuals;
        [Tooltip("Solo se utiliza al ejecutar manualmente Ajustar modelo al volumen del dispositivo.")]
        [SerializeField] private Vector3 targetSize = new(0.4826f, 0.04445f, 0.35f);

        [ContextMenu("Ajustar modelo al volumen del dispositivo")]
        public void Fit()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.LogWarning("Ajusta el modelo fuera de Play Mode para guardar sus medidas en la escena.", this);
                return;
            }
#else
            return;
#endif
            if (visuals == null || visuals.parent != transform ||
                targetSize.x <= 0 || targetSize.y <= 0 || targetSize.z <= 0) return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(visuals, "Ajustar visual del dispositivo");
#endif

            // Measure mesh corners in the device frame. Renderer.bounds would
            // enlarge the volume when the rack is rotated in the scene.
            Vector3 originalPosition = visuals.localPosition;
            Vector3 originalScale = visuals.localScale;
            visuals.localPosition = Vector3.zero;
            visuals.localScale = Vector3.one;
            Bounds bounds = default;
            bool found = false;
            foreach (MeshRenderer renderer in visuals.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                Bounds mesh = filter.sharedMesh.bounds;
                Matrix4x4 toDevice = transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 local = mesh.center + Vector3.Scale(mesh.extents, new Vector3(
                        (corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1,
                        (corner & 4) == 0 ? -1 : 1));
                    Vector3 point = toDevice.MultiplyPoint3x4(local);
                    if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                    else bounds.Encapsulate(point);
                }
            }
            if (!found || bounds.size.x < 0.00001f || bounds.size.y < 0.00001f || bounds.size.z < 0.00001f)
            {
                visuals.localPosition = originalPosition;
                visuals.localScale = originalScale;
                return;
            }
            Vector3 scale = new(targetSize.x / bounds.size.x, targetSize.y / bounds.size.y,
                targetSize.z / bounds.size.z);
            visuals.localScale = scale;
            visuals.localPosition = -Vector3.Scale(bounds.center, scale);
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(visuals);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }
    }
}
