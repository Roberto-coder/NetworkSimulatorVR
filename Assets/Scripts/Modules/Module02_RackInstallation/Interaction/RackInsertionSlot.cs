using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>
    /// Guía un XR Grab Interactable sobre el segmento EntryPose -> InstalledPose.
    /// Es una aproximación de riel/cajón para la primera fase del montaje.
    /// </summary>
    public sealed class RackInsertionSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private XRGrabInteractable acceptedGrab;
        [Tooltip("Otros dispositivos que pueden utilizar esta misma ranura.")]
        [SerializeField] private List<XRGrabInteractable> additionalAcceptedGrabs = new();
        [SerializeField] private RackInsertionGrabTransformer grabTransformer;
        [SerializeField] private Transform entryPose;
        [SerializeField] private Transform installedPose;
        [Tooltip("Guía visible durante el juego. Se oculta al completar la instalación.")]
        [SerializeField] private GameObject visualGuide;
        [SerializeField] private RackInsertionGuide guideController;

        [Header("Capture")]
        [Min(0.01f)] [SerializeField] private float maximumLateralDistance = 0.12f;
        [Range(1f, 90f)] [SerializeField] private float maximumAngle = 25f;
        [Range(0.8f, 1f)] [SerializeField] private float completionProgress = 0.96f;

        [Header("Objective")]
        [SerializeField] private string objectiveId = "mount_switch";

        [Header("Events")]
        [SerializeField] private UnityEvent onCaptured;
        [SerializeField] private UnityEvent<float> onProgressChanged;
        [SerializeField] private UnityEvent onInstalled;

        private XRGrabInteractable activeGrab;
        private RackInsertionGrabTransformer activeTransformer;
        private Rigidbody activeBody;
        private bool captured;
        private bool installed;
        private float progress;

        public XRGrabInteractable AcceptedGrab => acceptedGrab;
        public bool IsCaptured => captured;
        public bool IsInstalled => installed;
        public float Progress => progress;

        public event Action Captured;
        public event Action<float> ProgressChanged;
        public event Action Installed;

        private void Awake()
        {
            if (grabTransformer == null && acceptedGrab != null)
                grabTransformer = acceptedGrab.GetComponent<RackInsertionGrabTransformer>();
            if (guideController == null && visualGuide != null)
                guideController = visualGuide.GetComponent<RackInsertionGuide>();
        }

        private void OnEnable()
        {
            SetGuideVisible(!installed);
            ForEachAccepted(RegisterGrabEvents);
        }

        private void OnDisable()
        {
            ForEachAccepted(UnregisterGrabEvents);
        }

        internal void Consider(Collider candidate)
        {
            if (captured || installed)
                return;

            XRGrabInteractable candidateGrab = candidate.GetComponentInParent<XRGrabInteractable>();
            if (!IsAccepted(candidateGrab) || !candidateGrab.isSelected || entryPose == null || installedPose == null)
                return;

            Transform item = candidateGrab.transform;
            Vector3 axis = installedPose.position - entryPose.position;
            if (axis.sqrMagnitude < 0.000001f)
                return;

            Vector3 fromEntry = item.position - entryPose.position;
            Vector3 lateral = fromEntry - Vector3.Project(fromEntry, axis);
            float angle = Quaternion.Angle(item.rotation, entryPose.rotation);
            if (lateral.magnitude > maximumLateralDistance || angle > maximumAngle)
                return;

            captured = true;
            activeGrab = candidateGrab;
            activeTransformer = candidateGrab.GetComponent<RackInsertionGrabTransformer>();
            activeBody = candidateGrab.GetComponent<Rigidbody>();
            progress = SegmentProgress(item.position);
            if (activeBody != null) activeBody.useGravity = false;

            // Desde este momento XRI continúa siguiendo las manos, pero el transformer
            // posterior proyecta su pose sobre EntryPose -> InstalledPose.
            activeTransformer?.BeginGuidance(this, entryPose, installedPose);
            guideController?.SetState(RackInsertionGuideState.Captured);
            onCaptured?.Invoke();
            Captured?.Invoke();
        }

        private void Update()
        {
            // La finalización se difiere fuera de Process para no desactivar el interactuable
            // mientras XRI está recorriendo internamente su lista de grab transformers.
            if (!captured || installed)
                return;
            if (progress >= completionProgress)
                CompleteInstallation();
        }

        /// <summary>Recibe el avance calculado a partir de la pose de las manos.</summary>
        internal void ReportGuidedProgress(float value) => SetProgress(value);

        private float SegmentProgress(Vector3 position)
        {
            Vector3 segment = installedPose.position - entryPose.position;
            return segment.sqrMagnitude < 0.000001f
                ? 0f
                : Mathf.Clamp01(Vector3.Dot(position - entryPose.position, segment) / segment.sqrMagnitude);
        }

        private void SetProgress(float value)
        {
            if (Mathf.Approximately(progress, value)) return;
            progress = value;
            onProgressChanged?.Invoke(progress);
            ProgressChanged?.Invoke(progress);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            if (!captured || installed || activeBody == null || !ReferenceEquals(args.interactableObject, activeGrab))
                return;
            // XR Grab Interactable administra el estado cinemático según Movement Type.
            // Aquí sólo retiramos gravedad para que el riel sostenga el dispositivo.
            activeBody.useGravity = false;
            activeBody.linearVelocity = Vector3.zero;
            activeBody.angularVelocity = Vector3.zero;
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            if (!captured || installed || activeBody == null || !ReferenceEquals(args.interactableObject, activeGrab))
                return;
            // Los rieles sostienen el switch aunque se suelte a mitad del recorrido.
            activeBody.linearVelocity = Vector3.zero;
            activeBody.angularVelocity = Vector3.zero;
            activeBody.useGravity = false;
            activeBody.isKinematic = true;
        }

        private void CompleteInstallation()
        {
            installed = true;
            captured = true;
            SetProgress(1f);
            activeGrab.transform.SetPositionAndRotation(installedPose.position, installedPose.rotation);

            if (activeBody != null)
            {
                activeBody.linearVelocity = Vector3.zero;
                activeBody.angularVelocity = Vector3.zero;
                activeBody.useGravity = false;
                activeBody.isKinematic = true;
            }

            // Se bloquea hasta que la futura mecánica de tornillos decida liberarlo.
            activeTransformer?.EndGuidance();
            activeGrab.enabled = false;
            guideController?.SetState(RackInsertionGuideState.Installed);
            StartCoroutine(HideGuideAfterInstalledFeedback());
            Module02Manager.Instance?.FlowController?.TryCompleteCurrent(objectiveId);
            onInstalled?.Invoke();
            Installed?.Invoke();
        }

        [ContextMenu("Reset installation")]
        public void ResetInstallation()
        {
            XRGrabInteractable resetGrab = activeGrab != null ? activeGrab : acceptedGrab;
            if (resetGrab == null || entryPose == null) return;
            installed = false;
            captured = false;
            progress = 0f;
            SetGuideVisible(true);
            guideController?.SetState(RackInsertionGuideState.Available);
            resetGrab.enabled = true;
            activeTransformer?.EndGuidance();
            resetGrab.transform.SetPositionAndRotation(entryPose.position, entryPose.rotation);
            Rigidbody resetBody = resetGrab.GetComponent<Rigidbody>();
            if (resetBody != null)
            {
                resetBody.isKinematic = false;
                resetBody.useGravity = true;
                resetBody.linearVelocity = Vector3.zero;
                resetBody.angularVelocity = Vector3.zero;
            }
            activeGrab = null;
            activeTransformer = null;
            activeBody = null;
            onProgressChanged?.Invoke(0f);
            ProgressChanged?.Invoke(0f);
        }

        private void SetGuideVisible(bool visible)
        {
            if (visualGuide != null && visualGuide.activeSelf != visible)
                visualGuide.SetActive(visible);
        }

        private IEnumerator HideGuideAfterInstalledFeedback()
        {
            // Se conserva el verde unos instantes como confirmación del snap final.
            yield return new WaitForSeconds(0.45f);
            if (installed) SetGuideVisible(false);
        }

        private bool IsAccepted(XRGrabInteractable candidate)
        {
            if (candidate == null) return false;
            if (candidate == acceptedGrab) return true;
            return additionalAcceptedGrabs != null && additionalAcceptedGrabs.Contains(candidate);
        }

        private void ForEachAccepted(Action<XRGrabInteractable> action)
        {
            if (acceptedGrab != null) action(acceptedGrab);
            if (additionalAcceptedGrabs == null) return;
            foreach (XRGrabInteractable grab in additionalAcceptedGrabs)
                if (grab != null && grab != acceptedGrab) action(grab);
        }

        private void RegisterGrabEvents(XRGrabInteractable grab)
        {
            grab.selectEntered.AddListener(HandleSelectEntered);
            grab.selectExited.AddListener(HandleSelectExited);
        }

        private void UnregisterGrabEvents(XRGrabInteractable grab)
        {
            grab.selectEntered.RemoveListener(HandleSelectEntered);
            grab.selectExited.RemoveListener(HandleSelectExited);
        }

        private void OnDrawGizmosSelected()
        {
            if (entryPose == null || installedPose == null) return;
            Gizmos.color = installed ? Color.green : new Color(0.1f, 0.75f, 1f);
            Gizmos.DrawLine(entryPose.position, installedPose.position);
            Gizmos.DrawWireSphere(entryPose.position, maximumLateralDistance);
            Gizmos.DrawWireCube(installedPose.position, new Vector3(0.4826f, 0.04445f, 0.04f));
        }
    }
}
