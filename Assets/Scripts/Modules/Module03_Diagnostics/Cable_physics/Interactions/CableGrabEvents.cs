using HPhysic;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Oculus.Interaction;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Cable_physics.Interactions
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

