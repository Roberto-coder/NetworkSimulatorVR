namespace Modules.Module02_RackInstallation.Flow.Validation
{
    public static class Module02InspectionReminderRules
    {
        public static bool IsBlinkOn(float elapsed, float duration, float halfPeriod) =>
            elapsed >= 0f && elapsed < duration && halfPeriod > 0f && ((int)(elapsed / halfPeriod) % 2 == 0);

        public static bool ShouldHighlight(int step, bool active, bool required, bool reviewed, bool blinkOn) =>
            (step == 0 || step == 1) && active && required && !reviewed && blinkOn;
    }
}
