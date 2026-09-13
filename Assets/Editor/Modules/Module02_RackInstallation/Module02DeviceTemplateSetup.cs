#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Crea las dos plantillas físicas del Sprint 3 sin tocar la escena.</summary>
public static class Module02DeviceTemplateSetup
{
    private const string Folder = "Assets/Prefabs/Dispositivos/Modulo2/Templates";
    private const string StaticPath = Folder + "/RackDevice_Static_1U.prefab";
    private const string ManipulablePath = Folder + "/RackDevice_Manipulable_1U.prefab";

    private static readonly Vector3 OneUSize = new(0.4826f, 0.04445f, 0.35f);

    [MenuItem("Network Simulator/Module 02/Sprint 3 - Crear prefabs base de dispositivos")]
    public static void CreateTemplates()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play Mode antes de crear las plantillas.");

        EnsureFolder(Folder);
        GameObject staticPrefab = CreateStaticTemplate();
        GameObject manipulablePrefab = CreateManipulableTemplate();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.objects = new UnityEngine.Object[] { staticPrefab, manipulablePrefab };
        EditorGUIUtility.PingObject(staticPrefab);
        Debug.Log(
            "Sprint 3: prefabs base creados. Son plantillas 1U; duplica y renombra antes de asignar modelo, información y colliders definitivos.");
    }

    private static GameObject CreateStaticTemplate()
    {
        var root = new GameObject("RackDevice_Static_1U");
        try
        {
            CreateMarker(root.transform, "Visuals", "Coloca aquí el modelo. Ajusta sólo este hijo; conserva la raíz en escala 1.");
            CreateMarker(root.transform, "InfoTargets", "Añade aquí zonas informativas opcionales con collider, XR Simple y RackInfoTarget.");

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = OneUSize;
            collider.isTrigger = false;

            XRSimpleInteractable interactable = root.AddComponent<XRSimpleInteractable>();
            interactable.colliders.Clear();
            interactable.colliders.Add(collider);

            root.AddComponent<RackInfoTarget>();
            AddHoverOnlyFilter(root, interactable);
            return PrefabUtility.SaveAsPrefabAsset(root, StaticPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static GameObject CreateManipulableTemplate()
    {
        var root = new GameObject("RackDevice_Manipulable_1U");
        try
        {
            CreateMarker(root.transform, "Visuals", "Coloca aquí el modelo. Ajusta sólo este hijo; conserva la raíz en escala 1.");

            GameObject grabVolume = new("GrabVolume");
            grabVolume.transform.SetParent(root.transform, false);
            BoxCollider collider = grabVolume.AddComponent<BoxCollider>();
            collider.size = OneUSize;
            collider.isTrigger = false;

            CreateMarker(root.transform, "InfoTargets", "Las tarjetas de partes pequeñas van aquí y no deben usar el collider de agarre.");

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 5f;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0.35f;
            body.angularDamping = 1.25f;

            XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
            grab.colliders.Clear();
            grab.colliders.Add(collider);
            grab.selectMode = InteractableSelectMode.Multiple;
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.trackScale = false;
            grab.throwOnDetach = false;
            grab.forceGravityOnDetach = false;
            grab.useDynamicAttach = true;
            grab.matchAttachPosition = true;
            grab.matchAttachRotation = true;
            grab.snapToColliderVolume = true;
            grab.attachEaseInTime = 0.15f;

            root.AddComponent<RackInfoTarget>();
            NearOnlyGrabSelectFilter nearOnly = root.AddComponent<NearOnlyGrabSelectFilter>();
            List<UnityEngine.Object> filters = grab.startingSelectFilters;
            filters.Add(nearOnly);

            return PrefabUtility.SaveAsPrefabAsset(root, ManipulablePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void AddHoverOnlyFilter(GameObject owner, XRBaseInteractable interactable)
    {
        HoverOnlyInfoTargetSelectFilter filter = owner.AddComponent<HoverOnlyInfoTargetSelectFilter>();
        interactable.startingSelectFilters.Add(filter);
    }

    private static void CreateMarker(Transform parent, string name, string note)
    {
        GameObject marker = new(name);
        marker.transform.SetParent(parent, false);
        marker.AddComponent<Module02TemplateNote>().Set(note);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
