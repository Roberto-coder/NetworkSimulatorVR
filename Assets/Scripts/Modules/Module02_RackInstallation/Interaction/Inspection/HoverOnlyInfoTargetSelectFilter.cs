using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Modules.Module02_RackInstallation.Exploration
{
    /// <summary>
    /// Las zonas informativas reciben hover/dwell, pero no consumen el Grip.
    /// Esto permite que el XR Grab Interactable padre recoja el switch completo.
    /// </summary>
    public sealed class HoverOnlyInfoTargetSelectFilter : MonoBehaviour, IXRSelectFilter
    {
        public bool canProcess => isActiveAndEnabled;

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) => false;
    }
}
