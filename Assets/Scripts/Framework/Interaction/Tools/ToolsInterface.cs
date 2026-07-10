using Modules.Module03_Diagnostics.Cable_physics.Scripts;

namespace Framework.Interaction.Tools
{
    
    public interface IPeelable
    {
        bool IsPeeled { get; }
        void Peel();
    }

    public interface ICrimpable
    {
        bool IsCrimped { get; }
        void Crimp(Connector.CableColor color);
    }

    public interface ITestable
    {
        bool IsTested { get; }
        void Test();
    }
}