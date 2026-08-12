using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Wire;
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

            if (dragPlane == null)
                return;

            // El DragPlane es un plano vertical (su superficie local es X/Z).
            // Convertir a local evita que la punta se salga por profundidad al
            // mover la mano y funciona aunque el puzzle se rote o se escale.
            Vector3 localPoint = dragPlane.transform.InverseTransformPoint(worldPoint);
            Vector3 halfSize = dragPlane.size * 0.5f;
            Vector3 center = dragPlane.center;

            localPoint.x = Mathf.Clamp(localPoint.x,
                center.x - halfSize.x,
                center.x + halfSize.x);
            localPoint.z = Mathf.Clamp(localPoint.z,
                center.z - halfSize.z,
                center.z + halfSize.z);
            localPoint.y = center.y;

            tip.position = dragPlane.transform.TransformPoint(localPoint);

            Refresh();
        }

        public bool TryProjectRay(Ray ray, out Vector3 worldPoint)
        {
            worldPoint = default;

            if (dragPlane == null)
                return false;

            // El Plane visual del puzzle usa la normal local Y. La proyeccion
            // permite recorrer toda su superficie con la orientacion del
            // control, sin depender del alcance fisico del brazo.
            Plane plane = new(
                dragPlane.transform.up,
                dragPlane.transform.TransformPoint(dragPlane.center));

            if (!plane.Raycast(ray, out float distance))
                return false;

            worldPoint = ray.GetPoint(distance);
            return true;
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
