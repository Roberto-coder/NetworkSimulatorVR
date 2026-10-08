using Modules.Module02_RackInstallation.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Exploration
{
    public sealed class RackInfoTarget : MonoBehaviour
    {
        [SerializeField] private RackComponentInfo information;
        [Tooltip("Opcional: comparte la tarjeta de otro target. No crear ciclos.")]
        [SerializeField] private RackInfoTarget parentTarget;
        [SerializeField] private Renderer[] highlightRenderers;
        [SerializeField] private Color highlightColor = new(0.1f, 0.75f, 1f, 1f);
        [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

        private XRBaseInteractable interactable;
        private readonly List<Outline> hoverOutlines = new();
        private readonly List<Outline> ownedOutlines = new();
        private bool outlinesInitialized;

        public RackInfoTarget ResolvedTarget
        {
            get
            {
                RackInfoTarget candidate = this;
                for (int i = 0; i < 32 && candidate.parentTarget != null; i++)
                {
                    candidate = candidate.parentTarget;
                    if (candidate == this) return this;
                }
                return candidate;
            }
        }
        public RackComponentInfo Information => ResolvedTarget.information;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
        }

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<XRBaseInteractable>();
            if (interactable == null)
            {
                Debug.LogError("RackInfoTarget necesita XR Simple o Grab Interactable en el mismo objeto.", this);
                return;
            }
            interactable.hoverEntered.AddListener(HandleHoverEntered);
            interactable.hoverExited.AddListener(HandleHoverExited);
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.RemoveListener(HandleHoverEntered);
                interactable.hoverExited.RemoveListener(HandleHoverExited);
            }
            InfoFocusDetector.Instance?.ClearFocus(this);
            SetHighlight(false);
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            SetHighlight(true);
            InfoFocusDetector.Instance?.BeginFocus(this, args.interactorObject);
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            SetHighlight(interactable.isHovered);
            InfoFocusDetector.Instance?.EndFocus(this, args.interactorObject);
        }

        private void SetHighlight(bool visible)
        {
            if (visible && !outlinesInitialized)
                InitializeOutlines();

            foreach (Outline outline in hoverOutlines)
            {
                if (outline == null) continue;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = highlightColor;
                outline.OutlineWidth = outlineWidth;
                outline.enabled = visible;
            }
        }

        private void InitializeOutlines()
        {
            outlinesInitialized = true;
            bool hasAssignedRenderer = false;
            if (highlightRenderers != null)
                foreach (Renderer renderer in highlightRenderers)
                    if (renderer != null) { hasAssignedRenderer = true; break; }
            if (!hasAssignedRenderer)
                highlightRenderers = GetComponentsInChildren<Renderer>();
            var visited = new HashSet<GameObject>();
            foreach (Renderer targetRenderer in highlightRenderers)
            {
                if (targetRenderer == null || !visited.Add(targetRenderer.gameObject)) continue;
                Outline outline = targetRenderer.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = targetRenderer.gameObject.AddComponent<Outline>();
                    ownedOutlines.Add(outline);
                }
                hoverOutlines.Add(outline);
            }
        }

        private void OnDestroy()
        {
            foreach (Outline outline in ownedOutlines)
                if (outline != null) Destroy(outline);
        }
    }
}
