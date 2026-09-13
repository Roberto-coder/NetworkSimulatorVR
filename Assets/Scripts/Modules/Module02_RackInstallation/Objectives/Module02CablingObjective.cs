using System;
using System.Collections.Generic;
using Shared.Cabling;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    [Serializable]
    /// <summary>Par de direcciones que debe quedar unido sin importar su orientación.</summary>
    public struct RequiredCableLink
    {
        public string firstPortAddress;
        public string secondPortAddress;
    }

    /// <summary>
    /// Valida que todos los pares requeridos estén presentes antes de completar el objetivo.
    /// Esto impide avanzar sólo por insertar cables en puertos cualquiera.
    /// </summary>
    public sealed class Module02CablingObjective : MonoBehaviour
    {
        [SerializeField] private string objectiveId = "connect_patch_ports";
        [SerializeField] private List<RequiredCableLink> requiredLinks = new();
        [SerializeField] private List<PatchCableLink> cables = new();

        private void OnEnable()
        {
            foreach (PatchCableLink cable in cables)
                if (cable != null)
                    cable.LinkChanged += HandleLinkChanged;
        }

        private void OnDisable()
        {
            foreach (PatchCableLink cable in cables)
                if (cable != null)
                    cable.LinkChanged -= HandleLinkChanged;
        }

        public void Configure(IEnumerable<RequiredCableLink> links, IEnumerable<PatchCableLink> patchCables)
        {
            requiredLinks = links != null ? new List<RequiredCableLink>(links) : new List<RequiredCableLink>();
            cables = patchCables != null ? new List<PatchCableLink>(patchCables) : new List<PatchCableLink>();
        }

        public bool IsTopologyValid()
        {
            // Cada requisito puede satisfacerse con cualquiera de los cables registrados.
            if (requiredLinks.Count == 0)
                return false;
            foreach (RequiredCableLink required in requiredLinks)
            {
                bool found = cables.Exists(cable => cable != null &&
                    cable.Connects(required.firstPortAddress, required.secondPortAddress));
                if (!found)
                    return false;
            }
            return true;
        }

        private void HandleLinkChanged(PatchCableLink _) 
        {
            if (IsTopologyValid())
                Module02Manager.Instance?.FlowController?.TryCompleteCurrent(objectiveId);
        }
    }
}
