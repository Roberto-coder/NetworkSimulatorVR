using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>
    /// Postprocesa la pose calculada por XRGeneralGrabTransformer cuando el switch
    /// está dentro del riel. Antes de la captura no modifica el agarre normal.
    /// </summary>
    public sealed class RackInsertionGrabTransformer : XRBaseGrabTransformer
    {
        [Header("Soft alignment")]
        [Tooltip("Tiempo utilizado para pasar del agarre libre al movimiento sobre el riel.")]
        [Min(0.01f)] [SerializeField] private float alignmentDuration = 0.20f;

        private RackInsertionSlot activeSlot;
        private Transform entryPose;
        private Transform installedPose;
        private float alignmentStartedAt;
        private float lastProgress;

        // Normalmente el configurador lo registra explícitamente después del transformer
        // general. SingleAndMultiple funciona además como respaldo si un prefab pierde
        // accidentalmente esas listas al guardar sus overrides en la escena.
        protected override RegistrationMode registrationMode => RegistrationMode.SingleAndMultiple;

        public bool IsGuiding => activeSlot != null;

        /// <summary>Activa la guía al entrar en la zona de captura.</summary>
        public void BeginGuidance(RackInsertionSlot slot, Transform entry, Transform installed)
        {
            activeSlot = slot;
            entryPose = entry;
            installedPose = installed;
            alignmentStartedAt = Time.time;
            lastProgress = CalculateProgress(transform.position);
        }

        /// <summary>Devuelve el objeto al comportamiento normal de XR Grab.</summary>
        public void EndGuidance()
        {
            activeSlot = null;
            entryPose = null;
            installedPose = null;
            lastProgress = 0f;
        }

        public override void Process(XRGrabInteractable grabInteractable,
            XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            // Fuera del riel dejamos intacta la pose que calculó XRGeneralGrabTransformer.
            if (activeSlot == null || entryPose == null || installedPose == null)
                return;

            Vector3 segment = installedPose.position - entryPose.position;
            if (segment.sqrMagnitude < 0.000001f)
                return;

            // Proyectamos la posición solicitada por las manos sobre el segmento del riel.
            // Esto elimina movimiento lateral sin introducir fuerzas ni un solver de joints.
            float progress = Mathf.Clamp01(
                Vector3.Dot(targetPose.position - entryPose.position, segment) / segment.sqrMagnitude);
            Vector3 railPosition = Vector3.Lerp(entryPose.position, installedPose.position, progress);

            // Durante los primeros instantes mezclamos la pose libre con la pose restringida.
            // SmoothStep evita el salto visible que produciría un snap inmediato.
            float elapsed = Time.time - alignmentStartedAt;
            float blend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / alignmentDuration));
            targetPose.position = Vector3.Lerp(targetPose.position, railPosition, blend);
            targetPose.rotation = Quaternion.Slerp(targetPose.rotation, entryPose.rotation, blend);

            // La escala se conserva: este módulo nunca debe escalar el dispositivo con dos manos.
            if (!Mathf.Approximately(lastProgress, progress))
            {
                lastProgress = progress;
                activeSlot.ReportGuidedProgress(progress);
            }
        }

        private float CalculateProgress(Vector3 position)
        {
            if (entryPose == null || installedPose == null) return 0f;
            Vector3 segment = installedPose.position - entryPose.position;
            return segment.sqrMagnitude < 0.000001f
                ? 0f
                : Mathf.Clamp01(Vector3.Dot(position - entryPose.position, segment) / segment.sqrMagnitude);
        }
    }
}
