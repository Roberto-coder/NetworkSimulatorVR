using UnityEngine;

namespace Modules.Module03_Diagnostics.Cable_physics.Scripts
{
    [RequireComponent(typeof(Connector))]
    public class PhysicCableCon : MonoBehaviour
    {
        private Connector _connector;

        private void Awake()
        {
            _connector = GetComponent<Connector>();
        }

        // se llama cuando se suelta
        public void TryConnect(Connector target)
        {
            if (target == null) return;

            if (_connector.CanConnect(target))
            {
                target.Connect(_connector);
            }
            else if (!target.IsConnected)
            {
                transform.rotation = target.ConnectionRotation * _connector.RotationOffset;
                transform.position =
                    (target.ConnectionPosition + target.ConnectedOutOffset * 0.2f)
                    - (_connector.ConnectionPosition - transform.position);
            }
        }
    }
}