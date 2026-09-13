namespace Modules.Module02_RackInstallation.Flow.Validation
{
    public enum Module02Action { Mount, Fasten, Wire, Label, Console, Power, Configure }

    /// <summary>Permisos independientes de Unity; equipar herramientas nunca está restringido.</summary>
    public static class Module02SequenceRules
    {
        public static bool Allows(int step, Module02Action action, bool cablesLocked)
        {
            if (step < 0 || step > 9) return false;
            return action switch
            {
                Module02Action.Mount => step == 2,
                Module02Action.Fasten => step == 3,
                // Tras completar estos objetivos se permite reparar hasta encender el switch.
                Module02Action.Wire => step >= 4 && step <= 7 && !cablesLocked,
                Module02Action.Label => step >= 5 && step <= 7 && !cablesLocked,
                Module02Action.Console => step >= 6,
                Module02Action.Power => step >= 7 && step <= 8,
                Module02Action.Configure => step == 8,
                _ => false
            };
        }
    }
}
