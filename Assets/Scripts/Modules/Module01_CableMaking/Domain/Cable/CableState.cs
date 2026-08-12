namespace Modules.Module01_CableMaking.Domain.Cable
{
    /// <summary>
    /// Identifica un extremo físico del cable. El extremo y su progreso son
    /// conceptos independientes para evitar multiplicar los estados posibles.
    /// </summary>
    public enum CableEnd
    {
        Left,
        Right
    }

    public enum CableState
    {
        Whole,
        Peeled,
        Rj45Disordered,
        Rj45Ordered,
        Rj45Crimped
    }
}
