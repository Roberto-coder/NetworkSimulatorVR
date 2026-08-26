using System.Collections;
using Presentacion.NPC;
using UnityEngine;
using Waypoints;

namespace Presentacion.Tutorial
{
    public class MoveNpcStep: TutorialStep
    {
        private readonly Waypoint _targetWaypoint;
        
        public MoveNpcStep(Waypoint targetWaypoint)
        {
            _targetWaypoint = targetWaypoint;
        }
        
        public override IEnumerator Execute(TutorialDirector director)
        {
            NPCMovementController movement = director.MovementController;
            if (movement == null || _targetWaypoint == null)
            {
                Debug.LogError("No se puede mover al NPC sin controlador y waypoint.");
                yield break;
            }

            bool completed = false;

            void OnDestinationReached()
            {
                completed = true;
            }

            movement.DestinationReached += OnDestinationReached;

            try
            {
                movement.MoveTo(_targetWaypoint);

                while (!completed)
                    yield return null;
            }
            finally
            {
                movement.DestinationReached -= OnDestinationReached;
            }
        }
    }
}
