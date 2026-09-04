using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    public enum RackInsertionGuideState
    {
        Available,
        Captured,
        Installed
    }

    /// <summary>Mantiene la guía visual alineada con las poses de entrada e instalación.</summary>
    [ExecuteAlways]
    public sealed class RackInsertionGuide : MonoBehaviour
    {
        [SerializeField] private Transform entryPose;
        [SerializeField] private Transform installedPose;
        [SerializeField] private Transform corridor;
        [SerializeField] private Transform destination;
        [SerializeField] private Vector3 deviceSize = new(0.50f, 0.055f, 0.35f);

        [Header("State colors")]
        [SerializeField] private Color availableColor = new(0.05f, 0.75f, 1f, 0.35f);
        [SerializeField] private Color capturedColor = new(1f, 0.72f, 0.05f, 0.42f);
        [SerializeField] private Color installedColor = new(0.05f, 1f, 0.20f, 0.55f);

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColor = Shader.PropertyToID("_Color");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock propertyBlock;

        private void OnEnable()
        {
            Refresh();
            SetState(RackInsertionGuideState.Available);
        }
        private void OnValidate() => Refresh();
        private void LateUpdate()
        {
            if (!Application.isPlaying) Refresh();
        }

        public void Refresh()
        {
            if (entryPose == null || installedPose == null) return;
            Vector3 segment = installedPose.position - entryPose.position;
            float distance = segment.magnitude;

            if (corridor != null && distance > 0.0001f)
            {
                corridor.SetPositionAndRotation(
                    (entryPose.position + installedPose.position) * 0.5f,
                    Quaternion.LookRotation(segment.normalized, entryPose.up));
                corridor.localScale = new Vector3(deviceSize.x, deviceSize.y, distance);
            }

            if (destination != null)
            {
                destination.SetPositionAndRotation(installedPose.position, installedPose.rotation);
                destination.localScale = deviceSize;
            }
        }

        /// <summary>
        /// Cambia el color sin duplicar ni modificar SnapMaterial.mat. Un
        /// MaterialPropertyBlock hace que el estado afecte sólo a esta guía.
        /// </summary>
        public void SetState(RackInsertionGuideState state)
        {
            Color color = state switch
            {
                RackInsertionGuideState.Captured => capturedColor,
                RackInsertionGuideState.Installed => installedColor,
                _ => availableColor
            };

            propertyBlock ??= new MaterialPropertyBlock();
            foreach (Renderer targetRenderer in GetComponentsInChildren<Renderer>(true))
            {
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColor, color);
                propertyBlock.SetColor(LegacyColor, color);
                propertyBlock.SetColor(EmissionColor, new Color(color.r, color.g, color.b, 1f) * 0.35f);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
