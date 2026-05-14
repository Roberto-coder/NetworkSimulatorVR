using HPhysic;
using UnityEngine;
using Oculus.Interaction;

namespace HInteractions
{
    
    public class CableGrabEvents : MonoBehaviour
    {
        public PhysicCableCon cable;

        private Grabbable grabbable;

        void Awake()
        {
            grabbable = GetComponent<Grabbable>();
        }

        void OnEnable()
        {
            grabbable.WhenPointerEventRaised += OnEvent;
        }

        void OnDisable()
        {
            grabbable.WhenPointerEventRaised -= OnEvent;
        }

        private void OnEvent(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Unselect)
            {
                TryAutoConnect();
            }
        }

        void TryAutoConnect()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.1f);

            foreach (var hit in hits)
            {
                Connector other = hit.GetComponent<Connector>();
                if (other != null)
                {
                    cable.TryConnect(other);
                    break;
                }
            }
        }
    }
    
}

