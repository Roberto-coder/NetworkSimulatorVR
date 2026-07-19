using Modules.Module01_CableMaking.Domain.Cable;
using UnityEngine;


namespace Modules.Module01_CableMaking.Interaction
{
    [RequireComponent(typeof(Wire))]
    public class WireView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform tip;
        [SerializeField] private LineRenderer lineRenderer;
        
        [SerializeField] private BoxCollider  dragPlane;
        // [SerializeField] private Vector2 xLimits;
        // [SerializeField] private Vector2 yLimits;
        
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

            
            Bounds bounds = dragPlane.bounds;
            Vector3 point = worldPoint;

            point.x = Mathf.Clamp(point.x, bounds.min.x, bounds.max.x);
            point.y = Mathf.Clamp(point.y, bounds.min.y, bounds.max.y);

            tip.position = point;

            Refresh();
        }

        public void Snap(Transform target)
        {
            tip.position = target.position;

            UpdateLine();
        }

        public void ResetView()
        {
            tip.position = anchor.position;

            UpdateLine();
        }

        public void Refresh()
        {
            UpdateLine();
        }

        private void UpdateLine()
        {
            lineRenderer.SetPosition(0, anchor.position);
            lineRenderer.SetPosition(1, tip.position);
        }
    }
}