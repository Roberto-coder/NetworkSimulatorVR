using System.Collections;
using System.Collections.Generic;
using Presentacion.NPC;
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

            bool completed = false;

            void OnDestinationReached()
            {
                completed = true;
            }

            movement.DestinationReached += OnDestinationReached;

            movement.MoveTo(_targetWaypoint);

            while (!completed)
                yield return null;

            movement.DestinationReached -= OnDestinationReached;
        }
    }
}