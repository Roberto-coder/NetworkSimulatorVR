namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class CablePeeled
    {
        public bool CanCrimp => true;

        public void Crimp()
        {
            CableEvents.RaiseCableCrimped();
        }
    }
}