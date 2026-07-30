using UnityEngine;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
/// <summary>
/// Decide cuando debe mostrarse el menu de objetivos en la muñeca del jugador
/// </summary>
    public class WristMenuController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Transform headTransform;      // CenterEyeAnchor
        [SerializeField] private Transform wristTransform;      // Hijo de LeftHandAnchor, rotación local (0,0,0)
        [SerializeField] private GameObject wristMenuCanvas;    // El canvas a mostrar/ocultar

        [Header("Offset de orientación")]
        [Tooltip("Rotación local aplicada para que el eje de referencia apunte al suelo en reposo.")]
        [SerializeField] private Vector3 restOrientationOffsetEuler = new Vector3(90f, 0f, 0f);

        [Header("Umbral de mirada")]
        [Tooltip("Grados entre la mirada de la cabeza y la dirección hacia la muñeca. Menor = más exigente.")]
        [SerializeField] private float lookAtWristThresholdDegrees = 30f;

        [Header("Umbral de elevación de la muñeca")]
        [Tooltip("Grados por encima de la horizontal que debe superar el eje ajustado para considerar la muñeca 'girada hacia el usuario'.")]
        [SerializeField] private float minElevationAboveHorizontalDegrees = 0f;

        private Quaternion _restOrientationOffset;
        private bool isMenuVisible;
        
        private void Awake()
        {
            wristMenuCanvas.SetActive(false);
            _restOrientationOffset = Quaternion.Euler(restOrientationOffsetEuler);
        }

        private void Update()
        {
            bool isLookingAtWrist = IsLookingAtWrist();
            bool isWristForwardAboveHorizontal = IsWristForwardAboveHorizontal();

            bool shouldShow = isLookingAtWrist && isWristForwardAboveHorizontal;

            if (shouldShow != isMenuVisible)
            {
                isMenuVisible = shouldShow;
                wristMenuCanvas.SetActive(isMenuVisible);
            }
        }

        private bool IsLookingAtWrist()
        {
            Vector3 dirToWrist = (wristTransform.position - headTransform.position).normalized;
            float angle = Vector3.Angle(headTransform.forward, dirToWrist);
            return angle < lookAtWristThresholdDegrees;
        }

        private bool IsWristForwardAboveHorizontal()
        {
            float elevationDegrees = 90f - Vector3.Angle(GetAdjustedForward(), Vector3.up);
            return elevationDegrees > minElevationAboveHorizontalDegrees;
        }

        /// <summary>
        /// Aplica el offset de reposo sobre la rotación actual de wristTransform.
        /// Como wristTransform tiene rotación local (0,0,0) respecto a
        /// LeftHandAnchor, este offset se combina con la rotación mundial
        /// heredada del anchor, y el resultado apunta al suelo cuando la mano
        /// está en reposo (brazo colgando).
        /// </summary>
        private Vector3 GetAdjustedForward()
        {
            Quaternion adjustedRotation = wristTransform.rotation * _restOrientationOffset;
            return adjustedRotation * Vector3.forward;
        }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (wristTransform == null) return;

            if (!Application.isPlaying)
            {
                _restOrientationOffset = Quaternion.Euler(restOrientationOffsetEuler);
            }

            Vector3 adjustedForward = GetAdjustedForward();

            Gizmos.color = IsWristForwardAboveHorizontalGizmoSafe(adjustedForward) ? Color.green : Color.red;
            Gizmos.DrawRay(wristTransform.position, adjustedForward * 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(wristTransform.position, wristTransform.forward * 0.1f); // eje original, sin offset, de referencia

            Gizmos.color = Color.gray;
            Gizmos.DrawRay(wristTransform.position, Vector3.up * 0.1f);
        }

        private bool IsWristForwardAboveHorizontalGizmoSafe(Vector3 adjustedForward)
        {
            float elevationDegrees = 90f - Vector3.Angle(adjustedForward, Vector3.up);
            return elevationDegrees > minElevationAboveHorizontalDegrees;
        }
    #endif
    }
}