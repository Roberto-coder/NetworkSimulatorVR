using System.Collections;
using System.Collections.Generic;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Shared.Cabling
{
    /// <summary>
    /// Detecta sockets RJ45 cerca de un extremo del cable y solicita la conexión cuando
    /// el usuario lo suelta. Debe colocarse en el hijo trigger ConnectionDetection.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CableEndSocketDetector : MonoBehaviour
    {
        private readonly HashSet<NetworkPort> nearbyPorts = new();

        private Connector cableConnector;
        private PhysicCableCon physicalConnection;
        private XRGrabInteractable grabInteractable;
        private PatchCableLink cableLink;
        private Coroutine pendingConnection;

        /// <summary>Puerto compatible más cercano, útil para ayudas visuales futuras.</summary>
        public NetworkPort CandidatePort => FindClosestCompatiblePort();

        private void Awake()
        {
            cableConnector = GetComponentInParent<Connector>();
            physicalConnection = GetComponentInParent<PhysicCableCon>();
            grabInteractable = GetComponentInParent<XRGrabInteractable>();
            cableLink = GetComponentInParent<PatchCableLink>();

            Collider detectionCollider = GetComponent<Collider>();
            detectionCollider.isTrigger = true;

            if (cableConnector == null || physicalConnection == null || grabInteractable == null)
            {
                Debug.LogError("ConnectionDetection requiere Connector, PhysicCableCon y XRGrabInteractable en su extremo padre.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (grabInteractable == null)
                return;
            grabInteractable.selectEntered.AddListener(HandleGrabbed);
            grabInteractable.selectExited.AddListener(HandleReleased);
        }

        private void OnDisable()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(HandleGrabbed);
                grabInteractable.selectExited.RemoveListener(HandleReleased);
            }
            if (pendingConnection != null)
            {
                StopCoroutine(pendingConnection);
                pendingConnection = null;
            }
            nearbyPorts.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            NetworkPort port = other.GetComponentInParent<NetworkPort>();
            if (port != null)
                nearbyPorts.Add(port);
        }

        private void OnTriggerExit(Collider other)
        {
            NetworkPort port = other.GetComponentInParent<NetworkPort>();
            if (port != null)
                nearbyPorts.Remove(port);
        }

        private void HandleGrabbed(SelectEnterEventArgs _)
        {
            // Si se vuelve a tomar el cable antes de completar una conexión pendiente,
            // se cancela para que el plug no salte al socket mientras está en la mano.
            if (pendingConnection != null)
            {
                StopCoroutine(pendingConnection);
                pendingConnection = null;
            }

            // Tomar un plug ya instalado equivale a presionar la pestaña y retirarlo.
            if (cableConnector != null && cableConnector.IsConnected)
            {
                cableConnector.Disconnect();

                // Disconnect puede devolver el Rigidbody a dinámico mientras el plug aún
                // está dentro de los colliders del dispositivo. Mantenerlo cinemático
                // durante el agarre evita que la despenetración física lo haga rebotar;
                // ConnectAfterXriRelease restaurará la física si se suelta desconectado.
                Rigidbody body = cableConnector.Rigidbody;
                if (body != null)
                {
                    if (!body.isKinematic)
                    {
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                    }
                    body.isKinematic = true;
                }
            }
        }

        private void HandleReleased(SelectExitEventArgs _)
        {
            if (pendingConnection != null)
                StopCoroutine(pendingConnection);
            pendingConnection = StartCoroutine(ConnectAfterXriRelease());
        }

        /// <summary>
        /// XR Grab Interactable con movimiento Kinematic termina de restaurar el Rigidbody
        /// después de emitir selectExited. Esperar un frame garantiza que Connector guarde
        /// el estado dinámico real y pueda restaurarlo correctamente al desconectar.
        /// </summary>
        private IEnumerator ConnectAfterXriRelease()
        {
            yield return null;
            pendingConnection = null;

            if (grabInteractable == null || grabInteractable.isSelected) yield break;
            // Restore before Connect, so the socket remembers a dynamic free plug.
            RestoreDynamicBodyWhenDisconnected();

            NetworkPort candidate = FindClosestCompatiblePort();
            if (candidate != null && physicalConnection != null)
                physicalConnection.TryConnect(candidate.Socket);

            // Movement Type Kinematic permite un agarre VR estable, pero XRI puede dejar
            // el Rigidbody cinemático durante el frame en que notifica selectExited.
            // Si el plug no terminó conectado, recuperamos explícitamente su física; si
            // sí conectó, el socket conserva correctamente el estado cinemático del plug.
            RestoreDynamicBodyWhenDisconnected();
        }

        /// <summary>
        /// Garantiza que un extremo suelto vuelva a responder a gravedad y colisiones.
        /// Se ejecuta después de esperar a XRI para no competir con su ciclo de agarre.
        /// </summary>
        private void RestoreDynamicBodyWhenDisconnected()
        {
            if (cableConnector == null || cableConnector.IsConnected || cableConnector.Rigidbody == null)
                return;

            Rigidbody body = cableConnector.Rigidbody;
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.WakeUp();
        }

        private NetworkPort FindClosestCompatiblePort()
        {
            NetworkPort closest = null;
            float closestSqrDistance = float.PositiveInfinity;

            // HashSet evita registrar dos veces el mismo puerto cuando tiene varios colliders.
            nearbyPorts.RemoveWhere(port => port == null);
            foreach (NetworkPort port in nearbyPorts)
            {
                if (!port.isActiveAndEnabled || port.Socket == null || port.IsConnected)
                    continue;
                if (cableLink != null && port.Kind != cableLink.Kind)
                    continue;
                if (cableConnector == null || !cableConnector.CanConnectConditioned(port.Socket))
                    continue;

                float sqrDistance = (port.Socket.ConnectionPosition - transform.position).sqrMagnitude;
                if (sqrDistance >= closestSqrDistance)
                    continue;
                closest = port;
                closestSqrDistance = sqrDistance;
            }
            return closest;
        }
    }
}
