namespace Framework.Interaction.Tools.Interfaces
{
    public interface ITestable
    {
        bool CanTest { get; }
        void Test();
    }
}