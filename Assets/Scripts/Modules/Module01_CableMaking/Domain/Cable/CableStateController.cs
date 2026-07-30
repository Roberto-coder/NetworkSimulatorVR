using System;
using Modules.Module01_CableMaking.Presentation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class CableStateController : MonoBehaviour
    {
        public CableState CurrentState { get; private set; }
        
        public event Action<CableState> StateChanged;
        
        private void Awake()
        {
            CurrentState = CableState.Whole;
        }

        public bool CanPeel => CurrentState == CableState.Whole;

        public bool CanInsertRJ45 => CurrentState == CableState.Peeled;

        public bool CanCrimp => CurrentState == CableState.RJ45;

        public bool CanTest => CurrentState == CableState.RJ45Crimped;

        public bool TryAdvance(CableState nextState)
        {
            if (nextState <= CurrentState)
                return false;

            CurrentState = nextState;

            StateChanged?.Invoke(CurrentState);

            return true;
        }
    }
}