#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Modules.Module02_RackInstallation.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public static class Module02ExplorationSprint04Setup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";
    private const string SetupName = "SwitchInstallation_Sprint4";
    private const string SnapMaterialPath = "Assets/Recursos/LobbyUI/Materials/SnapMaterial.mat";
    private const float U = 0.04445f;

    [MenuItem("Network Simulator/Module 02/Sprint 4 - Configurar inserción del switch")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play Mode antes de configurar el Sprint 4.");
        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("No se encontró Modulo2.unity.", ScenePath);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform rack = Find(scene, "rackV1") ?? Find(scene, "TrabajoTerminal_Rack_22U");
        List<XRGrabInteractable> switchGrabs = FindSwitchGrabs(scene);
        if (rack == null || switchGrabs.Count == 0)
            throw new InvalidOperationException("Se necesitan el rack y al menos un switch con XR Grab Interactable.");

        Transform existing = Find(scene, SetupName);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        if (!TryGetRootOrientedWorldBounds(rack, out Bounds rackBounds))
            throw new InvalidOperationException("El rack no contiene renderers medibles.");

        var setup = new GameObject(SetupName);
        setup.transform.SetPositionAndRotation(rack.position + rack.rotation * rackBounds.center, rack.rotation);

        // Ranura provisional en la mitad superior, por encima del conjunto estático generado.
        float slotY = 4.5f * U;
        float rackFront = Mathf.Max(0.12f, rackBounds.extents.z);

        Transform entry = NewPose("EntryPose", setup.transform,
            new Vector3(0f, slotY, rackFront + 0.20f));
        Transform installed = NewPose("InstalledPose", setup.transform,
            new Vector3(0f, slotY, rackFront - 0.18f));

        GameObject visualGuide = CreateVisualGuide(setup.transform, entry, installed);

        RackInsertionSlot slot = setup.AddComponent<RackInsertionSlot>();
        RackInsertionGrabTransformer primaryTransformer = null;
        foreach (XRGrabInteractable switchGrab in switchGrabs)
        {
            RackInsertionGrabTransformer configured = ConfigureGrab(switchGrab);
            if (primaryTransformer == null) primaryTransformer = configured;
        }

        Set(slot, "acceptedGrab", switchGrabs[0]);
        Set(slot, "grabTransformer", primaryTransformer);
        SetObjectList(slot, "additionalAcceptedGrabs", switchGrabs, 1);
        Set(slot, "entryPose", entry);
        Set(slot, "installedPose", installed);
        Set(slot, "visualGuide", visualGuide);
        Set(slot, "guideController", visualGuide.GetComponent<RackInsertionGuide>());

        GameObject triggerObject = new("CaptureVolume");
        triggerObject.transform.SetParent(setup.transform, false);
        triggerObject.transform.localPosition = (entry.localPosition + installed.localPosition) * 0.5f;
        BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(0.56f, 0.16f, Vector3.Distance(entry.localPosition, installed.localPosition) + 0.20f);
        RackInsertionTrigger relay = triggerObject.AddComponent<RackInsertionTrigger>();
        Set(relay, "slot", slot);

        foreach (XRGrabInteractable switchGrab in switchGrabs)
        {
            Rigidbody body = switchGrab.GetComponent<Rigidbody>();
            body.useGravity = true;
            body.isKinematic = false;
            switchGrab.enabled = true;
        }

        EditorUtility.SetDirty(slot);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = setup;
        Debug.Log($"Sprint 4 configurado para {switchGrabs.Count} switch(es). Ajusta EntryPose e InstalledPose y prueba uno a la vez.", setup);
    }

    private static RackInsertionGrabTransformer ConfigureGrab(XRGrabInteractable grab)
    {
        Rigidbody body = grab.GetComponent<Rigidbody>();
        if (body == null)
            throw new InvalidOperationException($"{grab.name} necesita Rigidbody en el mismo objeto que XR Grab Interactable.");

        RackInsertionGrabTransformer railTransformer = GetOrAdd<RackInsertionGrabTransformer>(grab.gameObject);
        XRGeneralGrabTransformer generalTransformer = GetOrAdd<XRGeneralGrabTransformer>(grab.gameObject);

        // Primero XRI calcula el seguimiento natural; después el transformer del riel
        // proyecta esa pose sobre el eje de inserción cuando la ranura lo captura.
        generalTransformer.permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.All;
        generalTransformer.allowOneHandedScaling = false;
        generalTransformer.allowTwoHandedScaling = false;
        grab.addDefaultGrabTransformers = false;
        grab.startingSingleGrabTransformers.Clear();
        grab.startingSingleGrabTransformers.Add(generalTransformer);
        grab.startingSingleGrabTransformers.Add(railTransformer);
        grab.startingMultipleGrabTransformers.Clear();
        grab.startingMultipleGrabTransformers.Add(generalTransformer);
        grab.startingMultipleGrabTransformers.Add(railTransformer);

        grab.smoothPosition = true;
        grab.smoothPositionAmount = 10f;
        grab.tightenPosition = 0.75f;
        grab.smoothRotation = true;
        grab.smoothRotationAmount = 12f;
        grab.tightenRotation = 0.75f;

        // Conserva la configuración aunque el switch sea una instancia de prefab.
        PrefabUtility.RecordPrefabInstancePropertyModifications(grab);
        EditorUtility.SetDirty(grab);
        return railTransformer;
    }

    private static GameObject CreateVisualGuide(Transform parent, Transform entry, Transform installed)
    {
        Material snapMaterial = AssetDatabase.LoadAssetAtPath<Material>(SnapMaterialPath);
        if (snapMaterial == null)
            throw new FileNotFoundException("No se encontró SnapMaterial.", SnapMaterialPath);

        GameObject root = new("VisualGuide_Snap");
        root.transform.SetParent(parent, false);

        Vector3 segment = installed.localPosition - entry.localPosition;
        float distance = segment.magnitude;

        GameObject corridor = CreateGuideCube("InsertionCorridor", root.transform, snapMaterial);
        corridor.transform.localPosition = (entry.localPosition + installed.localPosition) * 0.5f;
        corridor.transform.localScale = new Vector3(0.50f, 0.055f, Mathf.Max(0.04f, distance));

        GameObject destination = CreateGuideCube("InstalledPosition", root.transform, snapMaterial);
        destination.transform.localPosition = installed.localPosition;
        destination.transform.localScale = new Vector3(0.50f, 0.055f, 0.35f);

        RackInsertionGuide guide = root.AddComponent<RackInsertionGuide>();
        Set(guide, "entryPose", entry);
        Set(guide, "installedPose", installed);
        Set(guide, "corridor", corridor.transform);
        Set(guide, "destination", destination.transform);
        guide.Refresh();
        return root;
    }

    private static GameObject CreateGuideCube(string name, Transform parent, Material material)
    {
        GameObject guide = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guide.name = name;
        guide.transform.SetParent(parent, false);
        UnityEngine.Object.DestroyImmediate(guide.GetComponent<Collider>());
        MeshRenderer renderer = guide.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return guide;
    }

    private static Transform NewPose(string name, Transform parent, Vector3 localPosition)
    {
        GameObject pose = new(name);
        pose.transform.SetParent(parent, false);
        pose.transform.localPosition = localPosition;
        pose.transform.localRotation = Quaternion.identity;
        return pose.transform;
    }

    private static void Set(UnityEngine.Object target, string property, UnityEngine.Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectList(UnityEngine.Object target, string property,
        IReadOnlyList<XRGrabInteractable> values, int startIndex)
    {
        SerializedObject serialized = new(target);
        SerializedProperty list = serialized.FindProperty(property);
        list.arraySize = Mathf.Max(0, values.Count - startIndex);
        for (int i = startIndex; i < values.Count; i++)
            list.GetArrayElementAtIndex(i - startIndex).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.TryGetComponent(out T component) ? component : target.AddComponent<T>();

    private static List<XRGrabInteractable> FindSwitchGrabs(Scene scene)
    {
        var result = new List<XRGrabInteractable>();
        foreach (XRGrabInteractable grab in UnityEngine.Object.FindObjectsByType<XRGrabInteractable>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (grab.gameObject.scene != scene || !HierarchyContains(grab.transform, "switch"))
                continue;
            result.Add(grab);
        }

        // Mantiene un orden estable en el Inspector: Switch 1U antes de Switch 2U.
        result.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    private static bool HierarchyContains(Transform candidate, string text)
    {
        for (Transform current = candidate; current != null; current = current.parent)
            if (current.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

    private static Transform Find(Scene scene, string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            if (string.Equals(candidate.name, exactName, StringComparison.OrdinalIgnoreCase))
                return candidate;
        return null;
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
}
#endif
