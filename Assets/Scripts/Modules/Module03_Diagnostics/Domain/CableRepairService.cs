using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Module03_Diagnostics.Domain
{
    [Serializable]
    public sealed class CableRepairDefinition
    {
        public string faultyCableId = "Patch-01";
        public string portA = "R01/front", portB = "PC-01/eth0";
        public string labelAtA = "R01", labelAtB = "PC-01";
        public string probeSource = "Laptop/eth0", destinationIp = "192.168.10.11", responderPort = "PC-01/eth0";
        public CableRepairDefinition Copy() => (CableRepairDefinition)MemberwiseClone();
    }

    /// <summary>Evidencias del incidente fÃ­sico; no decide mediante animaciones ni botones de completar.</summary>
    public sealed class CableRepairService
    {
        private readonly NetworkSession session;
        private readonly NetworkSimulationService simulation;
        private readonly CableRepairDefinition rule;
        private readonly HashSet<string> healthyTests = new();
        private bool faultyTested;
        private long resetSequence;
        private ProbeResult verification;
        public CableRepairService(NetworkSession session, NetworkSimulationService simulation, CableRepairDefinition definition)
        {
            this.session = session; this.simulation = simulation; rule = definition.Copy();
            var n = session.Snapshot();
            if (!n.cables.Any(c => c.id == rule.faultyCableId && c.interactable && !c.intact) ||
                rule.portA == rule.portB || !n.ports.Any(p => p.id == rule.portA) || !n.ports.Any(p => p.id == rule.portB) ||
                !n.ports.Any(p => p.id == rule.probeSource) || !n.ports.Any(p => p.id == rule.responderPort) ||
                !NetworkDefinitionValidator.IsIpv4(rule.destinationIp) || string.IsNullOrWhiteSpace(rule.labelAtA) || string.IsNullOrWhiteSpace(rule.labelAtB))
                throw new ArgumentException("ConfiguraciÃ³n de reparaciÃ³n invÃ¡lida.");
            RefreshReset();
        }
        private void RefreshReset()
        {
            long latest = session.History.LastOrDefault(h => h.Action == "Reset")?.Sequence ?? 0;
            if (latest == resetSequence) return;
            resetSequence = latest; faultyTested = false; healthyTests.Clear(); verification = null;
        }
        public bool TestCable(string cableId, out string message)
        {
            RefreshReset();
            var c = session.Snapshot().cables.Find(x => x.id == cableId);
            bool valid = c != null && c.interactable && string.IsNullOrEmpty(c.portA) && string.IsNullOrEmpty(c.portB);
            message = !valid ? "Desconecta ambos extremos antes de probar." : c.intact ? "Continuidad correcta." : "Sin continuidad: sustituir cable.";
            session.RecordCableTest(cableId, valid, message);
            if (!valid) return false;
            verification = null;
            if (c.id == rule.faultyCableId && !c.intact) faultyTested = true;
            if (c.intact) healthyTests.Add(c.id);
            return true;
        }
        public bool LabelEnd(string cableId, bool endA, string label, out string message)
        {
            RefreshReset();
            var c = session.Snapshot().cables.Find(x => x.id == cableId);
            // El usuario elige la etiqueta: una selecciÃ³n incorrecta se conserva y no satisface la regla.
            if (c == null || !c.interactable || c.id == rule.faultyCableId ||
                !((c.portA == rule.portA && c.portB == rule.portB) || (c.portA == rule.portB && c.portB == rule.portA)))
            { message = "Etiqueta Ãºnicamente el reemplazo conectado entre los extremos del incidente."; return false; }
            bool accepted = session.LabelCable(c.id, endA ? label : c.labelA, endA ? c.labelB : label);
            if (accepted) verification = null;
            message = accepted ? "Etiqueta aplicada: " + label : "Etiqueta rechazada.";
            return accepted;
        }
        public void ObserveProbe(ProbeResult result)
        {
            RefreshReset();
            if (result != null && simulation.IsCurrent(result) && result.SourcePort == rule.probeSource && result.DestinationIp == rule.destinationIp)
                verification = result;
        }
        public bool IsComplete => Status == "Incidente fÃ­sico resuelto: reparaciÃ³n y ping verificados.";
        public string Status
        {
            get
            {
                RefreshReset();
                var n = session.Snapshot();
                if (!faultyTested) return "Diagnostica el cable retirado con el tester.";
                var old = n.cables.Find(c => c.id == rule.faultyCableId);
                if (!string.IsNullOrEmpty(old.portA) || !string.IsNullOrEmpty(old.portB)) return "Retira ambos extremos del cable averiado.";
                var replacement = n.cables.Find(c => c.id != rule.faultyCableId && c.intact &&
                    ((c.portA == rule.portA && c.portB == rule.portB) || (c.portA == rule.portB && c.portB == rule.portA)));
                if (replacement == null) return "Instala un reemplazo Ã­ntegro en los puertos correctos.";
                if (!healthyTests.Contains(replacement.id)) return "Comprueba el reemplazo desconectado con el tester.";
                bool forward = replacement.portA == rule.portA;
                if ((forward ? replacement.labelA : replacement.labelB) != rule.labelAtA ||
                    (forward ? replacement.labelB : replacement.labelA) != rule.labelAtB) return "Corrige las etiquetas de ambos extremos del reemplazo.";
                if (verification == null || !simulation.IsCurrent(verification) || verification.Status != ProbeStatus.Success || verification.ResponderPort != rule.responderPort)
                    return "Ejecuta un nuevo ping al equipo reparado desde la laptop.";
                return "Incidente fÃ­sico resuelto: reparaciÃ³n y ping verificados.";
            }
        }
    }
}
