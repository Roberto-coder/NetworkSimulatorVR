
using Modules.Module01_CableMaking.Domain.Cable;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    [RequireComponent(typeof(Wire))]
    public class WireDragHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform tip;
        [SerializeField] private Transform anchor;
        [SerializeField] private LineRenderer line;

        [Header("Movement")]
        [SerializeField] private Transform dragPlane;

        private Wire wire;

        public Transform Tip => tip;

        private void Awake()
        {
            wire = GetComponent<Wire>();

            UpdateLine();
        }

        public void Drag(Vector3 worldPoint)
        {
            if (wire.IsConnected)
                return;

            Vector3 point = worldPoint;

            // Mantener el hilo siempre en el plano del puzzle
            point.z = dragPlane.position.z;

            tip.position = point;

            UpdateLine();
        }

        public void Snap(Transform connectionPoint)
        {
            tip.position = connectionPoint.position;

            UpdateLine();
        }

        public void ResetPosition()
        {
            tip.position = anchor.position;

            UpdateLine();
        }

        private void UpdateLine()
        {
            line.SetPosition(0, anchor.position);
            line.SetPosition(1, tip.position);
        }
    }
}