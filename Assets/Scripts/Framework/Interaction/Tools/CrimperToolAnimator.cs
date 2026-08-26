using Systems.Input;
using UnityEngine;

namespace Framework.Interaction.Tools
{
    [RequireComponent(typeof(Animator))]
    public class CrimperToolAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private static readonly int CrimpHash =
            Animator.StringToHash("Crimp");

        public void Play()
        {
            animator.SetTrigger(CrimpHash);
        }
    }
}