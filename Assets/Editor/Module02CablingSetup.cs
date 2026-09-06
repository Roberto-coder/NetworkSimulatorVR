using System;
using HPhysic;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Shared.Cabling;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class Module02CablingSetup
{
    // Se conserva el prefab de Module03 como fuente. El resultado se guarda como un
    // prefab distinto para evitar que el ajuste de escala afecte escenas anteriores.
    private const string SourceCable =
        "Assets/Scripts/Modules/Module03_Diagnostics/Cable_physics/PhysicCalbes/Prefabs/PhysicCableMM.prefab";
    private const string SourceSocket =
        "Assets/Scripts/Modules/Module03_Diagnostics/Cable_physics/PhysicCalbes/Prefabs/CableContactFemale.prefab";
    private const string Rj45Visual = "Assets/Prefabs/Modulo1/Entradas_Cable/Rj45.prefab";
    private const string OutputFolder = "Assets/Prefabs/Tools/Module02/Cabling";
    private const string CableOutput = OutputFolder + "/PatchCable_RJ45_1_7m.prefab";
    private const string PortOutput = OutputFolder + "/NetworkPortSocket_RJ45.prefab";

    [MenuItem("Network Simulator/Module 02/Build cabling prefabs")]
    public static void Build()
    {
        EnsureFolders();
        BuildCable();
        BuildSocket();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Cableado de Module02 creado: {CableOutput} y {PortOutput}");
    }

    private static void BuildCable()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(SourceCable);
        try
        {
            PhysicCable cable = root.GetComponent<PhysicCable>();
            if (cable == null)
                throw new InvalidOperationException("PhysicCableMM no contiene PhysicCable.");

            // 16 tramos de 10 cm dan un patch cord manejable de aproximadamente 1.7 m.
            cable.ConfigureDimensions(16, 0.1f, 0.006f);
            // La reconstrucción debe ejecutarse mientras el prefab está cargado; cambiar
            // únicamente los valores serializados dejaría la cantidad antigua de tramos.
            cable.RebuildConfiguredPoints();
            // StartConnector y EndConnector se inicializan en Play Mode. Durante la edición
            // se resuelven directamente desde los objetos serializados de la jerarquía.
            ConfigureCableEnd(FindCableEnd(root.transform, "Start"), -1f);
            ConfigureCableEnd(FindCableEnd(root.transform, "End"), 1f);

            PatchCableLink link = GetOrAdd<PatchCableLink>(root);
            CableTrafficVisualizer traffic = GetOrAdd<CableTrafficVisualizer>(root);
            var trafficSerialized = new SerializedObject(traffic);
            trafficSerialized.FindProperty("physicalCable").objectReferenceValue = cable;
            trafficSerialized.FindProperty("link").objectReferenceValue = link;
            trafficSerialized.FindProperty("emitDemoTraffic").boolValue = true;
            trafficSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CableOutput);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void BuildSocket()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(SourceSocket);
        try
        {
            // El prefab legado contiene un MonoBehaviour cuyo GUID ya no existe en el
            // proyecto. Unity permite cargarlo, pero no volver a guardarlo. Se quitan sólo
            // componentes realmente huérfanos, conservando Connector, Rigidbody y visuales.
            int removedScripts = RemoveMissingScriptsRecursively(root);
            if (removedScripts > 0)
                Debug.LogWarning($"Se eliminaron {removedScripts} scripts faltantes de la copia del socket RJ45.");

            Connector connector = root.GetComponent<Connector>();
            if (connector == null)
                throw new InvalidOperationException("CableContactFemale no contiene Connector.");

            NetworkPort port = GetOrAdd<NetworkPort>(root);
            port.Configure("device", "port-01", NetworkPortKind.EthernetRj45, connector);
            root.name = "NetworkPortSocket_RJ45";
            PrefabUtility.SaveAsPrefabAsset(root, PortOutput);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.TryGetComponent(out T component) ? component : target.AddComponent<T>();

    private static Connector FindCableEnd(Transform cableRoot, string childName)
    {
        Transform child = cableRoot.Find(childName);
        Connector connector = child != null ? child.GetComponent<Connector>() : null;
        if (connector == null)
            throw new InvalidOperationException($"No se encontró {childName} con un Connector válido.");
        return connector;
    }

    /// <summary>Elimina scripts faltantes del objeto y de toda su jerarquía.</summary>
    private static int RemoveMissingScriptsRecursively(GameObject root)
    {
        int removed = 0;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
        return removed;
    }

    /// <summary>
    /// Conserva Connector, Rigidbody y los eventos de agarre del extremo, pero sustituye
    /// sus primitivas provisionales por el RJ45 creado para Module01.
    /// </summary>
    private static void ConfigureCableEnd(Connector connector, float outwardSign)
    {
        if (connector == null)
            throw new InvalidOperationException("El cable físico no tiene ambos Connector configurados.");

        Transform end = connector.transform;
        // El Near-Far Interactor del rig de Module02 incluye Default (capa 0) en su
        // Raycast Mask, pero excluye la antigua capa 8 heredada de PhysicCableMM.
        end.gameObject.layer = 0;
        RemoveOldEndVisuals(end);

        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Rj45Visual);
        if (visualPrefab == null)
            throw new InvalidOperationException($"No se encontró el modelo RJ45 en {Rj45Visual}.");

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, end);
        visual.name = "RJ45_Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = outwardSign < 0f
            ? Quaternion.identity
            : Quaternion.Euler(0f, 180f, 0f);
        visual.transform.localScale = Vector3.one;
        FitVisualToLength(visual, end, outwardSign, 0.025f);

        // El collider es apenas mayor que la geometría para que el agarre VR sea viable,
        // pero permanece en escala centimétrica en lugar de los 50 cm del prefab original.
        BoxCollider collider = end.GetComponent<BoxCollider>();
        if (collider == null)
            collider = end.gameObject.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0f, outwardSign * 0.0125f);
        collider.size = new Vector3(0.018f, 0.016f, 0.03f);
        collider.isTrigger = false;

        Rigidbody body = end.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.mass = 0.02f;
            body.linearDamping = 0.08f;
            body.angularDamping = 0.12f;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // El punto lógico está en la punta del plug; su rotación original contiene la
        // orientación que Connector necesita para enfrentar el socket hembra.
        Transform connectionPoint = end.Find("ConnectionPosition");
        if (connectionPoint != null)
            connectionPoint.localPosition = new Vector3(0f, 0f, outwardSign * 0.025f);

        BoxCollider distanceTarget = ConfigureConnectionDetection(end, outwardSign);
        ConfigureGrabInteractable(end.gameObject, collider, distanceTarget);

        // El renderer anterior acaba de eliminarse; evitamos conservar una referencia rota.
        var connectorSerialized = new SerializedObject(connector);
        SerializedProperty colorRenderer = connectorSerialized.FindProperty("collorRenderer");
        if (colorRenderer != null)
            colorRenderer.objectReferenceValue = null;
        connectorSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// Mantiene una zona trigger separada del collider físico. Más adelante esta zona
    /// servirá para recordar el socket RJ45 cercano cuando el usuario suelte el cable.
    /// </summary>
    private static BoxCollider ConfigureConnectionDetection(Transform end, float outwardSign)
    {
        Transform detection = end.Find("ConnectionDetection");
        if (detection == null)
        {
            var detectionObject = new GameObject("ConnectionDetection");
            detection = detectionObject.transform;
            detection.SetParent(end, false);
        }

        detection.localPosition = new Vector3(0f, 0f, outwardSign * 0.025f);
        detection.localRotation = Quaternion.identity;
        detection.localScale = Vector3.one;
        detection.gameObject.layer = 0;

        BoxCollider trigger = GetOrAdd<BoxCollider>(detection.gameObject);
        trigger.isTrigger = true;
        trigger.center = Vector3.zero;
        trigger.size = new Vector3(0.025f, 0.022f, 0.025f);
        return trigger;
    }

    /// <summary>
    /// Configura agarre cercano con el collider sólido y distance grab con la zona trigger.
    /// Ambos pertenecen al mismo XRGrabInteractable, por lo que mueven el mismo extremo.
    /// </summary>
    private static void ConfigureGrabInteractable(GameObject end, BoxCollider physicalCollider,
        BoxCollider distanceTarget)
    {
        XRGrabInteractable grab = GetOrAdd<XRGrabInteractable>(end);
        var serializedGrab = new SerializedObject(grab);

        SerializedProperty colliders = serializedGrab.FindProperty("m_Colliders");
        colliders.arraySize = 2;
        colliders.GetArrayElementAtIndex(0).objectReferenceValue = physicalCollider;
        colliders.GetArrayElementAtIndex(1).objectReferenceValue = distanceTarget;
        serializedGrab.FindProperty("m_MovementType").enumValueIndex = 1; // Velocity Tracking
        serializedGrab.FindProperty("m_TrackPosition").boolValue = true;
        serializedGrab.FindProperty("m_TrackRotation").boolValue = true;
        serializedGrab.FindProperty("m_SmoothPosition").boolValue = true;
        serializedGrab.FindProperty("m_SmoothRotation").boolValue = true;
        serializedGrab.FindProperty("m_ThrowOnDetach").boolValue = false;
        serializedGrab.FindProperty("m_RetainTransformParent").boolValue = true;
        serializedGrab.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Quita sólo la geometría provisional; no toca ConnectionPosition.</summary>
    private static void RemoveOldEndVisuals(Transform end)
    {
        string[] removableNames = { "CableContactMale", "Point", "Cylidner", "Cylinder", "RJ45_Visual" };
        for (int i = end.childCount - 1; i >= 0; i--)
        {
            Transform child = end.GetChild(i);
            if (Array.Exists(removableNames, name => child.name == name))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Normaliza uniformemente un modelo importado usando sus bounds reales. De esta forma
    /// futuros cambios de escala en el FBX no vuelven a crear conectores gigantes.
    /// </summary>
    private static void FitVisualToLength(GameObject visual, Transform parent, float outwardSign,
        float targetLength)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            throw new InvalidOperationException("El prefab Rj45 no contiene ningún Renderer.");

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension <= Mathf.Epsilon)
            throw new InvalidOperationException("El modelo Rj45 tiene bounds vacíos.");

        visual.transform.localScale *= targetLength / largestDimension;

        // Tras escalar se recalcula el centro para situar la mitad del plug fuera del cable.
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        Vector3 localCenter = parent.InverseTransformPoint(bounds.center);
        Vector3 desiredCenter = new(0f, 0f, outwardSign * targetLength * 0.5f);
        visual.transform.localPosition += desiredCenter - localCenter;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Tools", "Module02");
        EnsureFolder("Assets/Prefabs/Tools/Module02", "Cabling");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
