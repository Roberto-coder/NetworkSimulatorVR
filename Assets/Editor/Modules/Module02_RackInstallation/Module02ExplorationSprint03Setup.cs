#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Modules.Module02_RackInstallation.Data;
using Modules.Module02_RackInstallation.Exploration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Sprint 3: genera dispositivos estáticos desde la plantilla, crea sus tarjetas
/// y monta una distribución provisional dentro del rack de Modulo2.
/// </summary>
public static class Module02ExplorationSprint03Setup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";
    private const string TemplatePath = "Assets/Prefabs/Dispositivos/Modulo2/Templates/RackDevice_Static_1U.prefab";
    private const string OutputFolder = "Assets/Prefabs/Dispositivos/Modulo2/Configured";
    private const string DataFolder = "Assets/GameData/Module02/Components";
    private const string GeneratedRootName = "RackDevices_Sprint3";
    private const float U = 0.04445f;

    private enum VisualKind { Prefab, Cables, Fiber }

    private readonly struct DeviceSpec
    {
        public readonly string Id, Name, FileName, Description, Function, Standard, SourcePath;
        public readonly RackComponentCategory Category;
        public readonly Vector3 Size;
        public readonly int RackUnits;
        public readonly VisualKind Visual;
        public readonly bool VerticalPdu;

        public DeviceSpec(string id, string name, string fileName, RackComponentCategory category,
            Vector3 size, int rackUnits, string description, string function, string standard,
            string sourcePath, VisualKind visual = VisualKind.Prefab, bool verticalPdu = false)
        {
            Id = id; Name = name; FileName = fileName; Category = category; Size = size;
            RackUnits = rackUnits; Description = description; Function = function; Standard = standard;
            SourcePath = sourcePath; Visual = visual; VerticalPdu = verticalPdu;
        }
    }

    private static readonly DeviceSpec[] Specs =
    {
        new("router", "Router", "Router_Static", RackComponentCategory.Router,
            new Vector3(0.4826f, U, 0.30f), 1,
            "Dispositivo provisional que representa un router montable en rack.",
            "Interconecta redes diferentes y decide por qué ruta enviar cada paquete.",
            "Modelo educativo provisional; las dimensiones y conexiones dependen del fabricante.",
            "Assets/Prefabs/Dispositivos/Modulo2/Switch1U_provisional.prefab"),

        new("server", "Servidor 2U", "Server_Static_2U", RackComponentCategory.Server,
            new Vector3(0.4826f, 2f * U, 0.60f), 2,
            "Servidor provisional de dos unidades de rack.",
            "Ejecuta servicios, aplicaciones o almacenamiento para otros equipos de la red.",
            "2U equivalen a 88.9 mm de altura nominal.",
            "Assets/Prefabs/Dispositivos/Modulo2/Server_2U_provisional.prefab"),

        new("firewall", "Firewall", "Firewall_Static_1U", RackComponentCategory.Firewall,
            new Vector3(0.4826f, U, 0.35f), 1,
            "Dispositivo provisional de seguridad de red.",
            "Inspecciona y controla el tráfico de acuerdo con reglas de seguridad.",
            "Modelo educativo: no representa un producto ni una política de seguridad específicos.",
            "Assets/Prefabs/Dispositivos/Modulo2/Switch1U_provisional.prefab"),

        new("patch_panel", "Patch panel", "PatchPanel_Static_1U", RackComponentCategory.PatchPanel,
            new Vector3(0.4826f, U, 0.08f), 1,
            "Panel pasivo para terminación y organización del cableado estructurado.",
            "Relaciona las salidas permanentes del edificio con latiguillos hacia los equipos activos.",
            "La categoría y el desempeño del enlace dependen del sistema de cableado instalado.",
            "Assets/Prefabs/Dispositivos/Modulo2/PatchPanel1U_provisional.prefab"),

        new("ups_pdu", "UPS / PDU vertical", "UPS_PDU_Static", RackComponentCategory.UpsPdu,
            new Vector3(0.055f, 0.75f, 0.055f), 0,
            "Representación provisional de distribución y respaldo de energía.",
            "Una UPS aporta respaldo temporal; una PDU distribuye energía a los equipos del rack.",
            "La capacidad, protecciones y conexión deben seguir la documentación eléctrica del equipo.",
            "Assets/Prefabs/Dispositivos/Modulo2/PDUVertical_provisional2 1.prefab",
            verticalPdu: true),

        new("structured_cabling", "Cableado estructurado", "StructuredCabling_Static", RackComponentCategory.Cabling,
            new Vector3(0.45f, U, 0.08f), 1,
            "Grupo simplificado de latiguillos de cobre dentro del rack.",
            "Conecta puertos del patch panel con switches y otros dispositivos manteniendo identificación y orden.",
            "La instalación real debe respetar categoría, radio de curvatura, longitud y etiquetado del sistema.",
            null, VisualKind.Cables),

        new("cooling", "Cooling", "Cooling_Static_2U", RackComponentCategory.Cooling,
            new Vector3(0.4826f, 2f * U, 0.40f), 2,
            "Unidad provisional para representar ventilación y administración térmica.",
            "Ayuda a retirar el calor y mantener el flujo de aire previsto para los equipos.",
            "La dirección del flujo y la capacidad térmica dependen del diseño del rack y del fabricante.",
            "Assets/Prefabs/Dispositivos/Modulo2/Server_2U_provisional.prefab"),

        new("fiber_optics", "Fibra óptica", "FiberOptics_Static_1U", RackComponentCategory.FiberOptics,
            new Vector3(0.45f, U, 0.12f), 1,
            "Representación simplificada de enlaces y terminaciones de fibra óptica.",
            "Transporta datos mediante luz y puede enlazar switches, distribuidores y equipos de telecomunicaciones.",
            "El tipo de fibra, conector, limpieza y radio de curvatura deben verificarse antes de conectar.",
            null, VisualKind.Fiber)
    };

    [MenuItem("Network Simulator/Module 02/Sprint 3 - Crear y montar dispositivos estáticos")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play Mode antes de ejecutar el Sprint 3.");
        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("No se encontró la escena Modulo2.", ScenePath);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath) == null)
            throw new InvalidOperationException("Primero ejecuta: Sprint 3 - Crear prefabs base de dispositivos.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder(OutputFolder);
        EnsureFolder(DataFolder);

        var generatedPrefabs = new Dictionary<string, GameObject>();
        foreach (DeviceSpec spec in Specs)
        {
            RackComponentInfo info = CreateOrUpdateInfo(spec);
            generatedPrefabs.Add(spec.Id, CreateOrUpdateDevicePrefab(spec, info));
        }
        AssetDatabase.SaveAssets();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform rack = FindRack(scene);
        if (rack == null)
            throw new InvalidOperationException("No se encontró rackV1 ni TrabajoTerminal_Rack_22U en Modulo2.");

        Transform oldRoot = FindSceneTransform(scene, GeneratedRootName);
        if (oldRoot != null)
            UnityEngine.Object.DestroyImmediate(oldRoot.gameObject);

        var assembly = new GameObject(GeneratedRootName);
        PlaceDevices(assembly.transform, rack, generatedPrefabs);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = assembly;

        Debug.Log("Sprint 3 configurado: 8 dispositivos estáticos, modelos provisionales y tarjetas añadidos al rack. " +
                  "Ajusta RackDevices_Sprint3 si la cara frontal del modelo está orientada al lado contrario.", assembly);
    }

    private static RackComponentInfo CreateOrUpdateInfo(DeviceSpec spec)
    {
        string path = $"{DataFolder}/{spec.Id}.asset";
        RackComponentInfo info = AssetDatabase.LoadAssetAtPath<RackComponentInfo>(path);
        if (info == null)
        {
            info = ScriptableObject.CreateInstance<RackComponentInfo>();
            AssetDatabase.CreateAsset(info, path);
        }

        SerializedObject data = new(info);
        data.FindProperty("id").stringValue = spec.Id;
        data.FindProperty("displayName").stringValue = spec.Name;
        data.FindProperty("category").enumValueIndex = (int)spec.Category;
        data.FindProperty("educationalApproximation").boolValue = true;
        data.FindProperty("shortDescription").stringValue = spec.Description;
        data.FindProperty("function").stringValue = spec.Function;
        data.FindProperty("rackUnits").intValue = spec.RackUnits;
        data.FindProperty("standardReference").stringValue = spec.Standard;
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(info);
        return info;
    }

    private static GameObject CreateOrUpdateDevicePrefab(DeviceSpec spec, RackComponentInfo info)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(TemplatePath);
        try
        {
            root.name = spec.FileName;
            BoxCollider collider = root.GetComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = spec.Size;

            Transform visuals = root.transform.Find("Visuals");
            ClearChildren(visuals);
            Component note = visuals.GetComponent("Module02TemplateNote");
            if (note != null) UnityEngine.Object.DestroyImmediate(note);

            if (spec.Visual == VisualKind.Prefab)
                AddPrefabVisual(visuals, spec.SourcePath, spec.Size);
            else
                AddCableVisuals(visuals, spec.Size, spec.Visual == VisualKind.Fiber);

            RackInfoTarget target = root.GetComponent<RackInfoTarget>();
            Renderer[] renderers = visuals.GetComponentsInChildren<Renderer>(false);
            SerializedObject targetData = new(target);
            targetData.FindProperty("information").objectReferenceValue = info;
            SerializedProperty highlights = targetData.FindProperty("highlightRenderers");
            highlights.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
                highlights.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            targetData.ApplyModifiedPropertiesWithoutUndo();

            XRSimpleInteractable simple = root.GetComponent<XRSimpleInteractable>();
            simple.colliders.Clear();
            simple.colliders.Add(collider);

            string path = $"{OutputFolder}/{spec.FileName}.prefab";
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void AddPrefabVisual(Transform parent, string sourcePath, Vector3 targetSize)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (source == null)
            throw new FileNotFoundException("No se encontró el modelo provisional.", sourcePath);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
        instance.name = "Model_Provisional";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        RemoveInteractionComponents(instance);
        if (!TryGetLocalRendererBounds(parent, out Bounds initial))
            return;

        Vector3 available = targetSize * 0.94f;
        Vector3 current = initial.size;
        instance.transform.localScale = new Vector3(
            SafeRatio(available.x, current.x),
            SafeRatio(available.y, current.y),
            SafeRatio(available.z, current.z));

        if (TryGetLocalRendererBounds(parent, out Bounds fitted))
            instance.transform.localPosition -= fitted.center;
    }

    private static void AddCableVisuals(Transform parent, Vector3 targetSize, bool fiber)
    {
        int count = fiber ? 3 : 5;
        float diameter = fiber ? 0.008f : 0.012f;
        for (int i = 0; i < count; i++)
        {
            GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cable.name = fiber ? $"Fiber_{i + 1:00}" : $"Cable_{i + 1:00}";
            UnityEngine.Object.DestroyImmediate(cable.GetComponent<Collider>());
            SceneManager.MoveGameObjectToScene(cable, parent.gameObject.scene);
            cable.transform.SetParent(parent, false);
            cable.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            cable.transform.localScale = new Vector3(diameter * 0.5f, targetSize.x * 0.45f, diameter * 0.5f);
            float t = count == 1 ? 0f : i / (float)(count - 1) - 0.5f;
            cable.transform.localPosition = new Vector3(0f, t * targetSize.y * 0.55f, t * targetSize.z * 0.35f);
        }
    }

    private static void PlaceDevices(Transform assembly, Transform rack, IReadOnlyDictionary<string, GameObject> prefabs)
    {
        if (!TryGetRootOrientedWorldBounds(rack, out Bounds rackBounds))
            throw new InvalidOperationException("El rack no contiene renderers medibles.");

        assembly.SetPositionAndRotation(rack.position + rack.rotation * rackBounds.center, rack.rotation);
        float rackHeight = 22f * U;
        float bottom = -rackHeight * 0.5f + U;
        float frontLocal = Mathf.Max(0.12f, rackBounds.extents.z) - 0.02f;
        float cursor = bottom;

        foreach (DeviceSpec spec in Specs)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[spec.Id]);
            instance.name = spec.Name;
            instance.transform.SetParent(assembly, false);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (spec.VerticalPdu)
            {
                instance.transform.localPosition = new Vector3(0.265f, 0f, frontLocal - spec.Size.z * 0.5f);
                continue;
            }

            float halfHeight = spec.Size.y * 0.5f;
            instance.transform.localPosition = new Vector3(0f, cursor + halfHeight, frontLocal - spec.Size.z * 0.5f);
            cursor += spec.Size.y + 0.006f;
        }
    }

    private static void RemoveInteractionComponents(GameObject root)
    {
        foreach (Collider component in root.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.DestroyImmediate(component);
        foreach (Rigidbody component in root.GetComponentsInChildren<Rigidbody>(true))
            UnityEngine.Object.DestroyImmediate(component);
        foreach (XRBaseInteractable component in root.GetComponentsInChildren<XRBaseInteractable>(true))
            UnityEngine.Object.DestroyImmediate(component);
        foreach (RackInfoTarget component in root.GetComponentsInChildren<RackInfoTarget>(true))
            UnityEngine.Object.DestroyImmediate(component);
    }

    private static bool TryGetLocalRendererBounds(Transform root, out Bounds result)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false);
        if (renderers.Length == 0) { result = default; return false; }
        result = new Bounds(root.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (Renderer renderer in renderers)
            EncapsulateWorldBounds(root, renderer.bounds, ref result);
        return true;
    }

    private static bool TryGetRootOrientedWorldBounds(Transform root, out Bounds result)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) { result = default; return false; }
        Quaternion inverseRotation = Quaternion.Inverse(root.rotation);
        bool initialized = false;
        result = default;
        foreach (Renderer renderer in renderers)
        {
            Bounds world = renderer.bounds;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 point = inverseRotation *
                    (world.center + Vector3.Scale(world.extents, new Vector3(x, y, z)) - root.position);
                if (!initialized) { result = new Bounds(point, Vector3.zero); initialized = true; }
                else result.Encapsulate(point);
            }
        }
        return true;
    }

    private static void EncapsulateWorldBounds(Transform root, Bounds world, ref Bounds local)
    {
        for (int x = -1; x <= 1; x += 2)
        for (int y = -1; y <= 1; y += 2)
        for (int z = -1; z <= 1; z += 2)
            local.Encapsulate(root.InverseTransformPoint(
                world.center + Vector3.Scale(world.extents, new Vector3(x, y, z))));
    }

    private static float SafeRatio(float target, float current) => current > 0.00001f ? target / current : 1f;

    private static Transform FindRack(Scene scene) =>
        FindSceneTransform(scene, "rackV1") ?? FindSceneTransform(scene, "TrabajoTerminal_Rack_22U");

    private static Transform FindSceneTransform(Scene scene, string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            if (string.Equals(candidate.name, exactName, StringComparison.OrdinalIgnoreCase))
                return candidate;
        return null;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(normalized)) return;
        string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(normalized));
    }
}
#endif
