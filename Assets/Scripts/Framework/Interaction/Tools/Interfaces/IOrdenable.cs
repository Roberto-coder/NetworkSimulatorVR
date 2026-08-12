namespace Framework.Interaction.Tools.Interfaces
{
    public interface IOrdenable
    {
        bool CanOrder { get; }
        void Order();
    }
}