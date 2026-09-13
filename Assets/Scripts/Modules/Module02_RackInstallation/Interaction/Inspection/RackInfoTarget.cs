using Modules.Module02_RackInstallation.Data;
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

        private XRBaseInteractable interactable;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

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
            propertyBlock = new MaterialPropertyBlock();
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
            if (interactable == null) return;
            interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            interactable.hoverExited.RemoveListener(HandleHoverExited);
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
            if (highlightRenderers == null) return;
            foreach (Renderer targetRenderer in highlightRenderers)
            {
                if (targetRenderer == null) continue;
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColor, visible ? highlightColor : Color.black);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
