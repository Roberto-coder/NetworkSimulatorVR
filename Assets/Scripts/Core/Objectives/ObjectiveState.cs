namespace NetworkVR.Core.Objectives
{
    /// <summary>
    /// Representa el estado actual del ciclo de vida de un objetivo del simulador.
    /// </summary>
    public enum ObjectiveState
    {
        /// <summary>
        /// El objetivo esta listo, pero aun no ha iniciado.
        /// </summary>
        Waiting,

        /// <summary>
        /// El objetivo esta activo actualmente.
        /// </summary>
        Running,

        /// <summary>
        /// El objetivo finalizo correctamente.
        /// </summary>
        Completed,

        /// <summary>
        /// El objetivo fue detenido antes de completarse.
        /// </summary>
        Cancelled
    }
}
