using UnityEngine;

namespace Framework.Interaction.Tools
{
    public abstract class InteractionTool<T> : MonoBehaviour
        where T : class
    {
        [Header("Detection")]
        [SerializeField] private Transform toolTip;
        [SerializeField] private float radius = 0.015f;
        [SerializeField] private LayerMask interactionMask;
        

        protected bool TryGetTarget(out T target)
        {
            Collider[] hits = Physics.OverlapSphere(
                toolTip.position,
                radius,
                interactionMask);

            foreach (Collider hit in hits)
            {
                target = hit.GetComponentInChildren(typeof(T)) as T;

                if (target != null)
                    return true;
            }

            target = null;
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (toolTip == null)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(toolTip.position, radius);
        }
#endif
    }
}