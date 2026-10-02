using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module03_Diagnostics.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Presenta el inventario existente sin duplicar IDs de cables ni destruir bindings de sesión.</summary>
    public sealed class Module03PresentationController : MonoBehaviour, IXRSelectFilter
    {
        public Module03GuidedFlow flow;
        public RepairPatchCord replacement;
        private RepairPatchCord faulty;
        private readonly List<XRGrabInteractable> grabs = new();
        public bool canProcess => isActiveAndEnabled;
        // Mantener los sockets seleccionados evita desconexiones al bloquear sólo las manos.
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable target) =>
            interactor is XRSocketInteractor || (flow != null && flow.Allows(DiagnosticStage.Cable));
        private bool ready, replacementRevealed;
        private IEnumerator Start()
        {
            while (flow != null && flow.repair != null && !flow.repair.Ready) yield return null;
            if (flow == null || replacement == null || flow.repair == null) yield break;
            faulty = flow.repair.cables.Single(c => c.cableId == flow.repair.settings.incident.faultyCableId);
            foreach (var cable in flow.repair.cables)
                foreach (var grab in cable.GetComponentsInChildren<XRGrabInteractable>(true)) { grabs.Add(grab); grab.selectFilters.Add(this); }
            flow.Restarted += ResetPresentation;
            ready = true;
            Refresh();
        }
        private void Update() { if (ready) Refresh(); }
        private void Refresh()
        {
            bool available = flow.Allows(DiagnosticStage.Cable);
            // La reserva ya forma parte del inventario; aparecer significa activar su única instancia.
            // No ocultar un repuesto ya instalado si se reactiva el tutorial a mitad de práctica.
            replacementRevealed |= available;
            if (replacement.gameObject.activeSelf != replacementRevealed) replacement.gameObject.SetActive(replacementRevealed);
            // Esperar a que el jugador suelte el cable, incluido el tester. No retirar objetos sostenidos.
            if (flow.StageIndex > 2 && faulty.gameObject.activeSelf && !faulty.endA.IsConnected && !faulty.endB.IsConnected &&
                !faulty.GetComponentsInChildren<XRGrabInteractable>(true).Any(g => g.isSelected))
                faulty.gameObject.SetActive(false);
        }
        private void ResetPresentation() { replacementRevealed = false; faulty.gameObject.SetActive(true); Refresh(); }
        private void OnDestroy()
        {
            if (flow != null) flow.Restarted -= ResetPresentation;
            foreach (var grab in grabs) if (grab != null) grab.selectFilters.Remove(this);
        }
    }
}
