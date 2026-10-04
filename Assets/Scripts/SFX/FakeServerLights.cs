using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Decorative server activity. Shared materials animate on the GPU.</summary>
public class FakeServerLights : MonoBehaviour
{
    [Tooltip("Shared Server LEDs material; adjust activity and color on the material.")]
    public Material fakeLightMaterial;
    [Tooltip("Imported object containing the server cabinets.")]
    [SerializeField] private string serverObjectName = "Servers";
    [SerializeField] private Transform serverRoot;
    private GameObject generatedRoot;

    private void Start() => Rebuild();

    [ContextMenu("Rebuild server LED preview")]
    public void Rebuild()
    {
        Clear();
        if (fakeLightMaterial == null) return;
        Transform searchRoot = transform.parent;
        if (serverRoot == null && searchRoot != null)
            foreach (Transform candidate in searchRoot.GetComponentsInChildren<Transform>(true))
                if (candidate.name == serverObjectName) { serverRoot = candidate; break; }
        if (serverRoot == null)
        {
            Debug.LogWarning("Server LEDs: assign Server Root to the imported Servers object.", this);
            return;
        }
        generatedRoot = new GameObject("Server LED overlays (generated)");
        generatedRoot.transform.SetParent(transform, false);
        // Temporary preview is never serialized into the scene; regenerated on Play.
        generatedRoot.hideFlags = HideFlags.DontSave;
        foreach (MeshFilter source in serverRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            if (source.sharedMesh == null) continue;
            var originalRenderer = source.GetComponent<MeshRenderer>();
            if (originalRenderer == null || !originalRenderer.enabled || !source.gameObject.activeInHierarchy) continue;
            var overlay = new GameObject("LED - " + source.name, typeof(MeshFilter), typeof(MeshRenderer));
            overlay.hideFlags = HideFlags.DontSave;
            overlay.layer = source.gameObject.layer;
            overlay.transform.SetParent(generatedRoot.transform, false);
            // Preserve the imported mesh's exact transform (including parent scale).
            overlay.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            Vector3 parentScale = generatedRoot.transform.lossyScale;
            Vector3 scale = source.transform.lossyScale;
            overlay.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            overlay.GetComponent<MeshFilter>().sharedMesh = source.sharedMesh;
            var renderer = overlay.GetComponent<MeshRenderer>();
            var materials = new Material[source.sharedMesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++) materials[i] = fakeLightMaterial;
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }

    private void OnEnable() { if (generatedRoot != null) generatedRoot.SetActive(true); }
    private void OnDisable() { if (generatedRoot != null) generatedRoot.SetActive(false); }
    private void OnDestroy() => Clear();
    private void Clear()
    {
        if (generatedRoot == null) return;
        generatedRoot.SetActive(false);
        if (Application.isPlaying) Destroy(generatedRoot);
        else DestroyImmediate(generatedRoot);
        generatedRoot = null;
    }
}
