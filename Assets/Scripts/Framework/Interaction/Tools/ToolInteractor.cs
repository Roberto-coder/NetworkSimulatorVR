using System;
using Systems.Input;
using UnityEngine;

namespace Framework.Interaction.Tools
{
    [RequireComponent(typeof(Tool))]
    public class ToolInteractor : MonoBehaviour
    {
        public event Action InteractPressed;

        private void OnEnable()
        {
            VRInputManager.Instance.RightTriggerPressed += RaiseInteraction;
        }

        private void OnDisable()
        {
            if (VRInputManager.Instance != null)
                VRInputManager.Instance.RightTriggerPressed -= RaiseInteraction;
        }

        private void RaiseInteraction()
        {
            InteractPressed?.Invoke();
        }
    }
}