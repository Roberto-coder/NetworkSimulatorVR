using Modules.Module03_Diagnostics.Cable_physics.Scripts;

namespace Framework.Interaction.Tools.Interfaces
{
    public interface ICrimpable
    {
        bool CanCrimp { get; }
        void Crimp();
    }
}