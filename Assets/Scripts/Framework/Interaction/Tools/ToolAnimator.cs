using Framework.Interaction.Tools;
using Systems.Input;
using UnityEngine;

namespace Framework.Interaction.Tools
{
    [RequireComponent(typeof(Animator))]
    public class ToolAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            VRInputManager.Instance.RightTriggerChanged += OnTriggerChanged;
        }

        private void OnDisable()
        {
            if (VRInputManager.Instance == null)
                return;

            VRInputManager.Instance.RightTriggerChanged -= OnTriggerChanged;
        }

        private void OnTriggerChanged(float value)
        {
            animator.SetFloat("Grip", value);
        }
    }
}