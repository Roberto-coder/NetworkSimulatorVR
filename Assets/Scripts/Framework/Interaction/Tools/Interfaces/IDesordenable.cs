namespace Framework.Interaction.Tools.Interfaces
{
    public interface IDesordenable
    {
        bool CanDisorder { get; }
        void Disorder();
    }
}