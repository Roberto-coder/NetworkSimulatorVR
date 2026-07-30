namespace Framework.Interaction.Tools.Interfaces
{
    public interface IPeelable
    {
        bool CanPeel { get; }
        void Peel();
    }
}