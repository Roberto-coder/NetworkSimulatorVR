using Framework.Interaction.Tools.Interfaces;
using Framework.Interaction.Tools;
using Systems.Input;
using UnityEngine;
namespace Modules.Module01_CableMaking.Interaction
{
    
    [RequireComponent(typeof(ToolInteractor))]
    public class CrimperTool : InteractionTool<ICrimpable>
    {
        private ToolInteractor interactor;

        [SerializeField] private AudioSource audioSource;

        [Header("Haptic Feedback")]
        [SerializeField] private OVRInput.Controller hapticController =
            OVRInput.Controller.RTouch;
        [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.7f;
        [SerializeField, Min(0f)] private float hapticDuration = 0.12f;
        [SerializeField, Range(0f, 1f)] private float hapticFrequency = 1f;

        private void Awake()
        {
            interactor = GetComponent<ToolInteractor>();
        }

        private void OnEnable()
        {
            interactor.InteractPressed += TryCrimp;
        }

        private void OnDisable()
        {
            if (interactor != null)
                interactor.InteractPressed -= TryCrimp;
        }
        
        private void TryCrimp()
        {
            if (!TryGetTarget(out ICrimpable crimpable) || !crimpable.CanCrimp)
                return;

            crimpable.Crimp();

            if (audioSource != null)
                audioSource.Play();

            VRInputManager.Instance?.PlayHaptic(
                hapticController,
                hapticAmplitude,
                hapticDuration,
                hapticFrequency);
        }
    }
}
