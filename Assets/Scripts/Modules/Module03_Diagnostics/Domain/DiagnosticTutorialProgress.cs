namespace Modules.Module03_Diagnostics.Domain
{
    // Orden pedagógico independiente de Unity y de la duración de los diálogos.
    public enum DiagnosticStage { OpenLaptop, FirstPing, Cable, Ports, Addresses, FinalVerification }

    public sealed class DiagnosticTutorialProgress
    {
        public bool LaptopOpened { get; private set; }
        public bool FirstPingExecuted { get; private set; }
        public void Open(string deviceId, string laptopId)
        { if (deviceId == laptopId) LaptopOpened = true; }
        public void Observe(ProbeResult result, NetworkSession session, string source, string destination)
        {
            // El primer intento acredita diagnóstico, aunque falle; nunca acepta otra sesión/origen.
            if (LaptopOpened && result != null && result.SessionId == session.Id &&
                result.Revision == session.Revision && result.SourcePort == source && result.DestinationIp == destination)
                FirstPingExecuted = true;
        }
        public void Reset() { LaptopOpened = FirstPingExecuted = false; }
        public static bool Allows(DiagnosticStage action, int objectiveIndex, int narrativeStage, bool guided)
            => objectiveIndex >= (int)action && (!guided || narrativeStage >= (int)action);
    }
}
