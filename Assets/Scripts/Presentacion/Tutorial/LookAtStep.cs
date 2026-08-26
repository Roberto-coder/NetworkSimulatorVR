using System.Collections;
using Presentacion.NPC;
using UnityEngine;

namespace Presentacion.Tutorial
{
    public sealed class LookAtStep : TutorialStep
    {
        private readonly NPCLookMode mode;
        private readonly Transform target;

        public LookAtStep(NPCLookMode mode, Transform target = null)
        {
            this.mode = mode;
            this.target = target;
        }

        public override IEnumerator Execute(TutorialDirector director)
        {
            NPCPlayerLookController controller = director.LookController;
            if (controller == null)
            {
                Debug.LogWarning("No existe NPCPlayerLookController para ejecutar LookAtStep.");
                yield break;
            }

            switch (mode)
            {
                case NPCLookMode.Player:
                    controller.LookAtPlayer();
                    break;
                case NPCLookMode.Transform:
                    controller.LookAt(target);
                    break;
                case NPCLookMode.MovementDirection:
                    controller.FollowMovementDirection();
                    break;
                default:
                    controller.StopLooking();
                    break;
            }

            yield return null;
        }
    }
}
