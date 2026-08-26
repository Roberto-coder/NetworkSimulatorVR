using UnityEngine;

namespace Presentacion.NPC
{
    public enum NPCLookMode
    {
        None,
        Player,
        Transform,
        MovementDirection
    }

    /// <summary>Orienta suavemente la parte visual del NPC hacia el objetivo solicitado.</summary>
    public sealed class NPCPlayerLookController : MonoBehaviour
    {
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Transform rotatingRoot;
        [SerializeField] private bool horizontalOnly = true;
        [SerializeField, Min(0f)] private float turnSpeed = 8f;
        [SerializeField] private NPCLookMode mode = NPCLookMode.Player;

        private Transform playerTarget;

        private void Start()
        {
            rotatingRoot ??= transform;
            playerTarget = FindCenterEyeAnchor();
            if (mode == NPCLookMode.Player)
                lookTarget = playerTarget;
        }

        private void LateUpdate()
        {
            if (mode == NPCLookMode.None)
                return;

            if (mode == NPCLookMode.MovementDirection)
            {
                rotatingRoot.localRotation = turnSpeed <= 0f
                    ? Quaternion.identity
                    : Quaternion.Slerp(rotatingRoot.localRotation, Quaternion.identity, turnSpeed * Time.deltaTime);
                return;
            }

            if (mode == NPCLookMode.Player && (playerTarget == null || !playerTarget.gameObject.activeInHierarchy))
                playerTarget = FindCenterEyeAnchor();
            if (mode == NPCLookMode.Player)
                lookTarget = playerTarget;
            if (lookTarget == null || rotatingRoot == null)
                return;

            Vector3 direction = lookTarget.position - rotatingRoot.position;
            if (horizontalOnly)
                direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion target = Quaternion.LookRotation(direction.normalized, Vector3.up);
            rotatingRoot.rotation = turnSpeed <= 0f
                ? target
                : Quaternion.Slerp(rotatingRoot.rotation, target, turnSpeed * Time.deltaTime);
        }

        public void LookAtPlayer()
        {
            playerTarget = FindCenterEyeAnchor();
            lookTarget = playerTarget;
            mode = NPCLookMode.Player;
        }

        public void LookAt(Transform target)
        {
            lookTarget = target;
            mode = target != null ? NPCLookMode.Transform : NPCLookMode.None;
        }

        public void FollowMovementDirection()
        {
            lookTarget = null;
            mode = NPCLookMode.MovementDirection;
        }

        public void StopLooking()
        {
            lookTarget = null;
            mode = NPCLookMode.None;
        }

        private static Transform FindCenterEyeAnchor()
        {
            GameObject centerEye = GameObject.Find("CenterEyeAnchor");
            return centerEye != null ? centerEye.transform : Camera.main?.transform;
        }
    }
}
