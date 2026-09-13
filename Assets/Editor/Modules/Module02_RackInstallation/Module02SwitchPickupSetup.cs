#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.XR.CoreUtils;

public static class Module02SwitchPickupSetup
{
    private const string ScenePath = "Assets/Scenes/Modulo2.unity";

    [MenuItem("Network Simulator/Module 02/Configurar agarre cercano y agachado R3")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play Mode antes de configurar el switch.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath);

        Transform switchRoot = FindSceneTransform(scene, "Switch_12Port_Educational");
        if (switchRoot == null)
            throw new InvalidOperationException("No se encontró Switch_12Port_Educational en Modulo2.");

        Undo.RegisterFullObjectHierarchyUndo(switchRoot.gameObject, "Configurar agarre del switch");

        XRSimpleInteractable simpleOnRoot = switchRoot.GetComponent<XRSimpleInteractable>();
        XRGrabInteractable grab = switchRoot.GetComponent<XRGrabInteractable>();
        if (simpleOnRoot != null)
            Undo.DestroyObjectImmediate(simpleOnRoot);
        if (grab == null)
            grab = Undo.AddComponent<XRGrabInteractable>(switchRoot.gameObject);

        Rigidbody body = switchRoot.GetComponent<Rigidbody>();
        if (body == null)
            body = Undo.AddComponent<Rigidbody>(switchRoot.gameObject);
        body.mass = 5f;
        body.useGravity = true;
        body.isKinematic = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearDamping = 0.35f;
        body.angularDamping = 1.25f;

        BoxCollider grabCollider = switchRoot.GetComponent<BoxCollider>();
        if (grabCollider == null)
            grabCollider = Undo.AddComponent<BoxCollider>(switchRoot.gameObject);
        FitCollider(grabCollider, switchRoot.GetComponentsInChildren<Renderer>(true));
        grabCollider.isTrigger = false;

        // Un collider explícito evita que el grab raíz se apropie de todos los
        // colliders pequeños que pertenecen a las tarjetas informativas.
        grab.colliders.Clear();
        grab.colliders.Add(grabCollider);
        grab.selectMode = InteractableSelectMode.Multiple;
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.trackPosition = true;
        grab.trackRotation = true;
        grab.trackScale = false;
        grab.throwOnDetach = false;
        grab.forceGravityOnDetach = false; // conserva useGravity=true del Rigidbody
        grab.useDynamicAttach = true;
        grab.matchAttachPosition = true;
        grab.matchAttachRotation = true;
        grab.snapToColliderVolume = true;
        grab.reinitializeDynamicAttachEverySingleGrab = true;
        grab.attachEaseInTime = 0.15f;
        grab.farAttachMode = InteractableFarAttachMode.DeferToInteractor;

        NearOnlyGrabSelectFilter nearOnly = switchRoot.GetComponent<NearOnlyGrabSelectFilter>();
        if (nearOnly == null)
            nearOnly = Undo.AddComponent<NearOnlyGrabSelectFilter>(switchRoot.gameObject);
        List<UnityEngine.Object> rootFilters = grab.startingSelectFilters;
        rootFilters.RemoveAll(item => item == null || item is NearOnlyGrabSelectFilter && item != nearOnly);
        if (!rootFilters.Contains(nearOnly))
            rootFilters.Add(nearOnly);

        XROrigin xrOrigin = UnityEngine.Object.FindFirstObjectByType<XROrigin>(FindObjectsInactive.Include);
        if (xrOrigin == null)
            throw new InvalidOperationException("No se encontró el XR Origin de PlayerVR_XRI.");
        SeatedCrouchController crouch = xrOrigin.GetComponent<SeatedCrouchController>();
        if (crouch == null)
            crouch = Undo.AddComponent<SeatedCrouchController>(xrOrigin.gameObject);

        int hoverOnlyCount = ConfigureInformativeChildren(switchRoot, grab);

        NearFarInteractor[] nearFar = UnityEngine.Object.FindObjectsByType<NearFarInteractor>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        int activeNearFar = 0;
        foreach (NearFarInteractor interactor in nearFar)
            if (interactor.gameObject.scene == scene && interactor.gameObject.activeInHierarchy)
                activeNearFar++;

        PrefabUtility.RecordPrefabInstancePropertyModifications(switchRoot.gameObject);
        EditorUtility.SetDirty(switchRoot.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = switchRoot.gameObject;

        if (activeNearFar < 2)
            Debug.LogWarning($"Sólo se encontraron {activeNearFar} Near-Far Interactors activos. Revisa Left/Right Controller y Left/Right Hand.", switchRoot);
        Debug.Log($"Switch configurado sólo para agarre cercano. R3 alterna agachado sentado. Zonas informativas sólo-hover: {hoverOnlyCount}; Near-Far activos: {activeNearFar}.", switchRoot);
    }

    private static int ConfigureInformativeChildren(Transform root, XRGrabInteractable rootGrab)
    {
        int count = 0;
        foreach (RackInfoTarget infoTarget in root.GetComponentsInChildren<RackInfoTarget>(true))
        {
            XRBaseInteractable interactable = infoTarget.GetComponent<XRBaseInteractable>();
            if (interactable == null || interactable == rootGrab || interactable is XRGrabInteractable)
                continue;

            HoverOnlyInfoTargetSelectFilter filter =
                infoTarget.GetComponent<HoverOnlyInfoTargetSelectFilter>();
            if (filter == null)
                filter = Undo.AddComponent<HoverOnlyInfoTargetSelectFilter>(infoTarget.gameObject);

            List<UnityEngine.Object> filters = interactable.startingSelectFilters;
            filters.RemoveAll(item => item == null);
            if (!filters.Contains(filter))
                filters.Add(filter);
            EditorUtility.SetDirty(interactable);
            count++;
        }
        return count;
    }

    private static Transform FindSceneTransform(Scene scene, string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate.name == exactName)
                    return candidate;
        return null;
    }

    private static void FitCollider(BoxCollider collider, Renderer[] renderers)
    {
        if (renderers.Length == 0)
            throw new InvalidOperationException("El switch no tiene Renderers para calcular su BoxCollider.");

        Transform owner = collider.transform;
        Bounds localBounds = new(owner.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.bounds;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                localBounds.Encapsulate(owner.InverseTransformPoint(corner));
            }
        }
        collider.center = localBounds.center;
        collider.size = localBounds.size;
    }
}
#endif
