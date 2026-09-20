using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace Shared.Cabling
{
    /// <summary>Separates cable physics from locomotion while preserving XR grab queries.</summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class CablePlayerCollisionFilter : MonoBehaviour
    {
        private readonly List<Collider> cableColliders = new();
        private static CharacterController[] controllers;
        private static float lastScan = float.NegativeInfinity;
        private static int lastRigSetupFrame = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            controllers = null;
            lastScan = float.NegativeInfinity;
            lastRigSetupFrame = -1;
        }

        private void OnEnable() { controllers = null; lastRigSetupFrame = -1; Apply(); }

        // Refresh before physics: covers rebuilt segments and pooled/re-enabled bodies.
        // Character discovery is shared across cables, once per physics step.
        private void FixedUpdate() => Apply();
        private void Update() => Apply();

        private void Apply()
        {
            int layer = LayerMask.NameToLayer("Cable");
            if (layer < 0) return;
            int mask = 1 << layer;
            if (controllers == null || lastScan != Time.fixedTime)
            {
                controllers = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
                lastScan = Time.fixedTime;
            }
            GetComponentsInChildren(false, cableColliders);
            foreach (var cableCollider in cableColliders)
                cableCollider.gameObject.layer = layer;
            foreach (var controller in controllers)
            {
                if (controller == null || !controller.enabled) continue;
                var origin = controller.GetComponentInParent<XROrigin>();
                if (origin == null) continue;
                controller.excludeLayers |= mask;
                if (lastRigSetupFrame != Time.frameCount)
                {
                foreach (var gravity in origin.GetComponentsInChildren<GravityProvider>(true))
                    gravity.sphereCastLayerMask &= ~mask;
                foreach (var caster in origin.GetComponentsInChildren<SphereInteractionCaster>(true))
                    caster.physicsLayerMask |= mask;
                foreach (var caster in origin.GetComponentsInChildren<CurveInteractionCaster>(true))
                    caster.raycastMask |= mask;
                foreach (var ray in origin.GetComponentsInChildren<XRRayInteractor>(true))
                    ray.raycastMask |= mask;
                }
                foreach (var cableCollider in cableColliders)
                {
                    if (cableCollider == null || !cableCollider.enabled || cableCollider == controller) continue;
                    if (!Physics.GetIgnoreCollision(controller, cableCollider))
                        Physics.IgnoreCollision(controller, cableCollider, true);
                }
            }
            lastRigSetupFrame = Time.frameCount;
        }
    }
}
