using System;
namespace Modules.Module01_CableMaking.Domain.Cable
{
    public static class CableEvents
    {
        public static event Action<CableEnd> CablePeeled;
        public static event Action<CableEnd> CableDisordered;
        public static event Action<CableEnd> CableOrdered;
        public static event Action<CableEnd> CableCrimped;
        /// <summary>
        /// Validar es una operacion del cable completo, no de un extremo.
        /// </summary>
        public static event Action CableValidated;

        public static void RaiseCablePeeled(CableEnd end)
            => CablePeeled?.Invoke(end);
        
        public static void RaiseCableDisordered(CableEnd end)
            => CableDisordered?.Invoke(end);

        public static void RaiseCableOrdered(CableEnd end)
            => CableOrdered?.Invoke(end);
        
        public static void RaiseCableCrimped(CableEnd end)
            => CableCrimped?.Invoke(end);

        public static void RaiseCableValidated()
            => CableValidated?.Invoke();
    }
}
