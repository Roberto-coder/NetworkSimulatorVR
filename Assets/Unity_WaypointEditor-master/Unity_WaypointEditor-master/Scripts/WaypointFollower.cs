using System;
using System.Collections;
using UnityEngine;
using Waypoints;

namespace Unity_WaypointEditor_master.Unity_WaypointEditor_master.Scripts
{
    /// <summary>
    /// Recorre una secuencia de Waypoints.
    /// El movimiento se controla completamente mediante código.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WaypointFollower : MonoBehaviour
    {
        [Header("Movement")]

        [SerializeField]
        [Tooltip("Distancia mínima para considerar alcanzado un waypoint.")]
        private float distanceThreshold = 0.15f;

        [SerializeField]
        private float moveSpeed = 0.5f;

        [SerializeField]
        private float turnSpeed = 5f;

        private Rigidbody rb;

        private Coroutine followRoutine;

        private Waypoint currentWaypoint;
        private bool followLinkedWaypoints;

        /// <summary>
        /// Waypoint que actualmente se está siguiendo.
        /// </summary>
        public Waypoint CurrentWaypoint => currentWaypoint;

        /// <summary>
        /// Indica si el objeto se encuentra recorriendo una ruta.
        /// </summary>
        public bool IsMoving { get; private set; }

        /// <summary>
        /// Se dispara cuando se abandona un waypoint.
        /// </summary>
        public event Action<Waypoint> WaypointPassed;

        /// <summary>
        /// Se dispara cuando finaliza completamente la ruta.
        /// </summary>
        public event Action PathCompleted;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Comienza a recorrer una ruta.
        /// </summary>
        public void BeginFollow(Waypoint firstWaypoint)
        {
            BeginMovement(firstWaypoint, true);
        }

        /// <summary>
        /// Se mueve solamente hasta el waypoint indicado, ignorando los enlaces
        /// posteriores de la ruta.
        /// </summary>
        public void MoveTo(Waypoint destination)
        {
            BeginMovement(destination, false);
        }

        private void BeginMovement(
            Waypoint firstWaypoint,
            bool continueThroughLinkedWaypoints)
        {
            if (firstWaypoint == null)
            {
                Debug.LogWarning($"{name}: se recibió un waypoint nulo.");
                return;
            }

            Stop();

            currentWaypoint = firstWaypoint;
            followLinkedWaypoints = continueThroughLinkedWaypoints;
            IsMoving = true;

            followRoutine = StartCoroutine(FollowPathRoutine());
        }

        /// <summary>
        /// Detiene inmediatamente el recorrido.
        /// </summary>
        public void Stop()
        {
            IsMoving = false;

            if (followRoutine != null)
            {
                StopCoroutine(followRoutine);
                followRoutine = null;
            }
        }

        private IEnumerator FollowPathRoutine()
        {
            while (IsMoving && currentWaypoint != null)
            {
                RotateTowardsWaypoint(currentWaypoint.transform);

                if (Vector3.Distance(transform.position, currentWaypoint.transform.position) <= distanceThreshold)
                {
                    Waypoint passedWaypoint = currentWaypoint;

                    currentWaypoint = followLinkedWaypoints
                        ? currentWaypoint.GetNextWaypoint()
                        : null;

                    WaypointPassed?.Invoke(passedWaypoint);
                }
                else
                {
                    MoveTowardsWaypoint();
                }

                yield return new WaitForFixedUpdate();
            }

            IsMoving = false;
            followRoutine = null;

            PathCompleted?.Invoke();
        }

        /// <summary>
        /// Avanza en la dirección frontal.
        /// </summary>
        protected virtual void MoveTowardsWaypoint()
        {
            // Vector3 nextPosition =
            //     rb.position +
            //     transform.forward * moveSpeed * Time.fixedDeltaTime;
            //
            // rb.MovePosition(nextPosition);
            Vector3 direction =
                (currentWaypoint.transform.position - rb.position).normalized;

            rb.MovePosition(
                rb.position +
                direction * moveSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Gira hacia el waypoint actual.
        /// </summary>
        protected virtual void RotateTowardsWaypoint(Transform target)
        {
            Vector3 direction = target.position - transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction.normalized);

            Quaternion rotation =
                Quaternion.RotateTowards(
                    rb.rotation,
                    targetRotation,
                    turnSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime);

            rb.MoveRotation(rotation);
        }
    }
}
