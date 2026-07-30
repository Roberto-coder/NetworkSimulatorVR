using Framework.Interaction.Tools.Interfaces;
using UnityEngine;

namespace Modules.Module01_CableMaking.Domain.Cable
{
    public class CableRJ45Crimped :
        MonoBehaviour,
        ITesterConnectable
    {
        public bool CanConnectTester => true;

        public void Validate()
        {
            CableEvents.RaiseCableValidated();
        }
    }
}