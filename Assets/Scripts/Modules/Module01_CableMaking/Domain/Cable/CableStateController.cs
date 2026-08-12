using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class CableStateController : MonoBehaviour
    {
        [Header("Debug Initial States")]
        [Tooltip("Usa Whole para el flujo normal. Rj45Ordered permite probar el crimpado directamente.")]
        [SerializeField] private CableState initialLeftState = CableState.Whole;

        [Tooltip("Usa Whole para el flujo normal. Rj45Ordered permite probar el crimpado directamente.")]
        [SerializeField] private CableState initialRightState = CableState.Whole;

        private readonly Dictionary<CableEnd, CableState> states = new()
        {
            { CableEnd.Left, CableState.Whole },
            { CableEnd.Right, CableState.Whole }
        };
        
        public event Action<CableEnd, CableState> StateChanged;

        private void Awake()
        {
            states[CableEnd.Left] = initialLeftState;
            states[CableEnd.Right] = initialRightState;

            Debug.Log(
                $"[CableStateController] Inicializado. " +
                $"Left={GetState(CableEnd.Left)}, Right={GetState(CableEnd.Right)}.",
                this);
        }
        
        /// <summary>
        /// Busca el controlador del mismo cable. Admite que el controlador sea
        /// un componente de un padre o un GameObject hijo de la raiz del cable.
        /// </summary>
        public static CableStateController ResolveFor(Component source)
        {
            if (source == null)
                return null;

            CableStateController controller =
                source.GetComponentInParent<CableStateController>();

            return controller != null
                ? controller
                : source.transform.root.GetComponentInChildren<CableStateController>(true);
        }

        public CableState GetState(CableEnd end) => states[end];
        
        public bool CanPeel(CableEnd end) => GetState(end) == CableState.Whole;
        
        public bool CanDisorder(CableEnd end) => GetState(end) == CableState.Peeled;

        public bool CanOrderRJ45(CableEnd end) => GetState(end) == CableState.Rj45Disordered;

        public bool CanCrimp(CableEnd end) => GetState(end) == CableState.Rj45Ordered;

        public bool CanTest => AreAllEndsInState(CableState.Rj45Crimped);

        public bool AreAllEndsInState(CableState state) =>
            GetState(CableEnd.Left) == state && GetState(CableEnd.Right) == state;

        public bool TryAdvance(CableEnd end, CableState nextState)
        {
            CableState currentState = GetState(end);

            if ((int)nextState != (int)currentState + 1)
            {
                Debug.LogWarning(
                    $"[CableStateController] Transición inválida en {end}: " +
                    $"{currentState} -> {nextState}.",
                    this);
                return false;
            }

            states[end] = nextState;

            Debug.Log(
                $"[CableStateController] {end}: {currentState} -> {nextState}.",
                this);
            StateChanged?.Invoke(end, nextState);

            return true;
        }
    }
}
