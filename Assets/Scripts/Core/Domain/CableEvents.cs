using System;

namespace Core.Domain
{
    public static class CableEvents
    {
        
        public static event Action CableStripped;
        public static event Action CableCrimped;
        public static event Action CableInsertedIntoTester;

        public static void RaiseCableStripped()
            => CableStripped?.Invoke();

        public static void RaiseCableCrimped()
            => CableCrimped?.Invoke();

        public static void RaiseCableInsertedIntoTester()
            => CableInsertedIntoTester?.Invoke();
    }
}