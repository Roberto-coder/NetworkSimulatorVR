using System;
using Unity_WaypointEditor_master.Unity_WaypointEditor_master.Scripts;
using UnityEngine;
using Waypoints;

namespace Presentacion.NPC
{
    /// <summary>
    /// Capa de abstracción para controlar el movimiento del NPC.
    /// Oculta completamente el sistema de Waypoints.
    /// </summary>
    public class NPCMovementController : MonoBehaviour
    {
        [SerializeField]
        private WaypointFollower waypointFollower;

        public bool IsMoving =>
            waypointFollower != null &&
            waypointFollower.IsMoving;

        public event Action DestinationReached;

        private void Awake()
        {
            if (waypointFollower == null)
                waypointFollower = GetComponent<WaypointFollower>();
        }

        private void OnEnable()
        {
            if (waypointFollower != null)
                waypointFollower.PathCompleted += HandlePathCompleted;
        }

        private void OnDisable()
        {
            if (waypointFollower != null)
                waypointFollower.PathCompleted -= HandlePathCompleted;
        }

        /// <summary>
        /// Mueve el NPC únicamente hasta el waypoint indicado.
        /// </summary>
        public void MoveTo(Waypoint destination)
        {
            waypointFollower.MoveTo(destination);
        }

        /// <summary>
        /// Detiene el movimiento.
        /// </summary>
        public void Stop()
        {
            waypointFollower.Stop();
        }

        private void HandlePathCompleted()
        {
            DestinationReached?.Invoke();
        }
    }
}
