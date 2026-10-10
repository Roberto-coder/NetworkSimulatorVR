using Modules.Module01_CableMaking.Interaction;
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
            if (target == null || _connector.IsConnectionLocked) return;

            if (_connector.CanConnectConditioned(target))
            {
                target.Connect(_connector);
            }
            else if (!target.IsConnected)
            {
                Quaternion offset = Quaternion.Inverse(transform.rotation) * _connector.ConnectionRotation;
                transform.rotation = target.ConnectionRotation * Quaternion.Inverse(offset);
                transform.position =
                    (target.ConnectionPosition + target.ConnectedOutOffset * 0.2f)
                    - (_connector.ConnectionPosition - transform.position);
            }
        }
    }
}
