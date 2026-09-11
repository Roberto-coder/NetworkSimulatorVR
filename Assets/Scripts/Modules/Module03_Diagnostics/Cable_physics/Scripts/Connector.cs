using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Cable_physics.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Connector : MonoBehaviour
    {
        public enum ConType { Male, Female }
        public enum CableColor { White, Red, Green, Yellow, Blue, Cyan, Magenta }

        [field: Header("Settings")]

        [field: SerializeField] public ConType ConnectionType { get; private set; } = ConType.Male;
        [field: SerializeField, OnValueChanged(nameof(UpdateConnectorColor))] public CableColor ConnectionColor { get; private set; } = CableColor.White;

        [SerializeField] private bool makeConnectionKinematic = false;
        private bool _wasConnectionKinematic;

        [SerializeField] private bool hideInteractableWhenIsConnected = false;
        [SerializeField] private bool allowConnectDifrentCollor = false;

        [field: SerializeField] public Connector ConnectedTo { get; private set; }


        [Header("Object to set")]
        [SerializeField, Required] private Transform connectionPoint;
        [SerializeField] private MeshRenderer collorRenderer;
        [SerializeField] private ParticleSystem sparksParticle;


        private FixedJoint _fixedJoint;
        public Rigidbody Rigidbody { get; private set; }
        public Shared.Cabling.PatchCableLink CableOwner { get; private set; }

        public Vector3 ConnectionPosition => connectionPoint ? connectionPoint.position : transform.position;
        public Quaternion ConnectionRotation => connectionPoint ? connectionPoint.rotation : transform.rotation;
        public Quaternion RotationOffset => connectionPoint ? connectionPoint.localRotation : Quaternion.Euler(Vector3.zero);
        public Vector3 ConnectedOutOffset => connectionPoint ? connectionPoint.right : transform.right;

        public bool IsConnected => ConnectedTo != null;
        public bool IsConnectedRight => IsConnected && ConnectionColor == ConnectedTo.ConnectionColor;



        private void Awake()
        {
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            // XRI detaches the grabbed endpoint from its parent until release.
            CableOwner = GetComponentInParent<Shared.Cabling.PatchCableLink>();
        }

        private void Start()
        {
            UpdateConnectorColor();

            if (ConnectedTo != null && _fixedJoint == null && ConnectedTo._fixedJoint == null)
            {
                Connector t = ConnectedTo;
                ConnectedTo = null;
                if (t.ConnectedTo == this) t.ConnectedTo = null;
                Connect(t);
            }
        }

        private void OnDisable() => Disconnect();

        private void LateUpdate()
        {
            // Un FixedJoint no puede arrastrar de forma fiable dos Rigidbody cinemáticos.
            // Cuando este Connector pertenece a un socket fijo o a un dispositivo móvil,
            // mantenemos el plug alineado explícitamente con su punto de conexión.
            if (makeConnectionKinematic && ConnectedTo != null && ConnectedTo.Rigidbody != null &&
                ConnectedTo.Rigidbody.isKinematic)
            {
                AlignConnector(ConnectedTo);
            }
        }

        public void SetAsConnectedTo(Connector secondConnector)
        {
            ConnectedTo = secondConnector;
            _wasConnectionKinematic = secondConnector.Rigidbody.isKinematic;
            UpdateInteractableWhenIsConnected();
        }
        public void Connect(Connector secondConnector)
        {
            if (secondConnector == null)
            {
                Debug.LogWarning("Attempt to connect null");
                return;
            }

            if (!Shared.Cabling.CablePortCompatibility.Allows(this, secondConnector)) return;
            // Typed sockets must not replace another cable or leave an orphaned joint.
            if ((GetComponentInParent<Shared.Cabling.NetworkPort>() != null ||
                 secondConnector.GetComponentInParent<Shared.Cabling.NetworkPort>() != null) &&
                !CanConnect(secondConnector)) return;

            if (IsConnected)
                Disconnect(secondConnector);

            AlignConnector(secondConnector);

            _fixedJoint = gameObject.AddComponent<FixedJoint>();
            _fixedJoint.connectedBody = secondConnector.Rigidbody;

            secondConnector.SetAsConnectedTo(this);
            _wasConnectionKinematic = secondConnector.Rigidbody.isKinematic;
            if (makeConnectionKinematic)
                secondConnector.Rigidbody.isKinematic = true;
            ConnectedTo = secondConnector;

            // sparks on inncretc connection
            if (incorrectSparksC == null && sparksParticle && IsConnected && !IsConnectedRight)
            {
                incorrectSparksC = IncorrectSparks();
                StartCoroutine(incorrectSparksC);
            }

            // disable outline on select
            UpdateInteractableWhenIsConnected();
        }
        public void Disconnect(Connector onlyThis = null)
        {
            if (ConnectedTo == null || onlyThis != null && onlyThis != ConnectedTo)
                return;

            Destroy(_fixedJoint);

            // important to dont make recusrion
            Connector toDisconect = ConnectedTo;
            ConnectedTo = null;
            if (makeConnectionKinematic)
            {
                toDisconect.Rigidbody.isKinematic = _wasConnectionKinematic;
                // Evita que una velocidad residual del agarre o de los resortes lance el
                // plug al recuperar la simulación dinámica.
                if (!toDisconect.Rigidbody.isKinematic)
                {
                    toDisconect.Rigidbody.linearVelocity = Vector3.zero;
                    toDisconect.Rigidbody.angularVelocity = Vector3.zero;
                }
            }
            toDisconect.Disconnect(this);

            // sparks on inncretc connection
            if (sparksParticle)
            {
                sparksParticle.Stop();
                sparksParticle.Clear();
            }

            // enable outline on select
            UpdateInteractableWhenIsConnected();
        }

        private void UpdateInteractableWhenIsConnected()
        {
            if (hideInteractableWhenIsConnected)
            {
                if (TryGetComponent(out Collider collider))
                    collider.enabled = !IsConnected;
            }
        }


        private IEnumerator incorrectSparksC;
        private IEnumerator IncorrectSparks()
        {
            while (incorrectSparksC != null && sparksParticle && IsConnected && !IsConnectedRight)
            {
                sparksParticle.Play();

                yield return new WaitForSeconds(Random.Range(0.6f, 0.8f));
            }
            incorrectSparksC = null;
        }

        private void UpdateConnectorColor()
        {
            if (collorRenderer == null)
                return;

            Color color = MaterialColor(ConnectionColor);
            MaterialPropertyBlock probs = new();
            collorRenderer.GetPropertyBlock(probs);
            probs.SetColor("_Color", color);
            collorRenderer.SetPropertyBlock(probs);
        }

        /// <summary>
        /// Hace coincidir el ConnectionPosition del plug con el del socket sin asumir que
        /// el origen de ninguno de los modelos esté situado exactamente en la punta.
        /// </summary>
        private void AlignConnector(Connector connector)
        {
            connector.transform.rotation = ConnectionRotation * connector.RotationOffset;
            connector.transform.position =
                ConnectionPosition - (connector.ConnectionPosition - connector.transform.position);
        }

        private Color MaterialColor(CableColor cableColor) => cableColor switch
        {
            CableColor.White => Color.white,
            CableColor.Red => Color.red,
            CableColor.Green => Color.green,
            CableColor.Yellow => Color.yellow,
            CableColor.Blue => Color.blue,
            CableColor.Cyan => Color.cyan,
            CableColor.Magenta => Color.magenta,
            _ => Color.clear
        };


        public bool CanConnect(Connector secondConnector) =>
            secondConnector != null && this != secondConnector
            && Shared.Cabling.CablePortCompatibility.Allows(this, secondConnector)
            && !this.IsConnected && !secondConnector.IsConnected
            && this.ConnectionType != secondConnector.ConnectionType
            && (this.allowConnectDifrentCollor || secondConnector.allowConnectDifrentCollor || this.ConnectionColor == secondConnector.ConnectionColor);
    }
}
