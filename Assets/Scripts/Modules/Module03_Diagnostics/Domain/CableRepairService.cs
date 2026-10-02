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

    public enum CableRepairProgress
    {
        TestFaultyCable, RemoveFaultyCable, InstallReplacement, TestReplacement,
        CorrectLabels, VerifyFromLaptop, Complete
    }

    /// <summary>Evidencias del incidente físico; no decide mediante animaciones ni botones de completar.</summary>
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
                throw new ArgumentException("Configuración de reparación inválida.");
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
        /// <summary>Activa la etiqueta del extremo usando su puerto real, incluso con el cable invertido.</summary>
        public bool ActivateLabel(string cableId, bool endA, out string message)
        {
            RefreshReset();
            var cable = session.Snapshot().cables.Find(c => c.id == cableId);
            string port = cable == null ? null : endA ? cable.portA : cable.portB;
            if (port != rule.portA && port != rule.portB)
            { message = "Conecta el reemplazo a los puertos del incidente antes de etiquetar."; return false; }
            return LabelEnd(cableId, endA, port == rule.portA ? rule.labelAtA : rule.labelAtB, out message);
        }

        public bool LabelEnd(string cableId, bool endA, string label, out string message)
        {
            RefreshReset();
            var c = session.Snapshot().cables.Find(x => x.id == cableId);
            // El usuario elige la etiqueta: una selección incorrecta se conserva y no satisface la regla.
            if (c == null || !c.interactable || c.id == rule.faultyCableId ||
                !((c.portA == rule.portA && c.portB == rule.portB) || (c.portA == rule.portB && c.portB == rule.portA)))
            { message = "Etiqueta únicamente el reemplazo conectado entre los extremos del incidente."; return false; }
            long revision = session.Revision;
            bool accepted = session.LabelCable(c.id, endA ? label : c.labelA, endA ? c.labelB : label);
            if (accepted && session.Revision != revision) verification = null;
            message = accepted ? "Etiqueta aplicada: " + label : "Etiqueta rechazada.";
            return accepted;
        }
        public void ObserveProbe(ProbeResult result)
        {
            RefreshReset();
            if (result != null && simulation.IsCurrent(result) && result.SourcePort == rule.probeSource && result.DestinationIp == rule.destinationIp)
                verification = result;
        }
        // El progreso depende de un estado tipado, nunca del texto traducido de la interfaz.
        public bool IsComplete => Progress == CableRepairProgress.Complete;
        public CableRepairProgress Progress
        {
            get
            {
                RefreshReset();
                var n = session.Snapshot();
                if (!faultyTested) return CableRepairProgress.TestFaultyCable;
                var old = n.cables.Find(c => c.id == rule.faultyCableId);
                if (!string.IsNullOrEmpty(old.portA) || !string.IsNullOrEmpty(old.portB)) return CableRepairProgress.RemoveFaultyCable;
                var replacement = n.cables.Find(c => c.id != rule.faultyCableId && c.intact &&
                    ((c.portA == rule.portA && c.portB == rule.portB) || (c.portA == rule.portB && c.portB == rule.portA)));
                if (replacement == null) return CableRepairProgress.InstallReplacement;
                if (!healthyTests.Contains(replacement.id)) return CableRepairProgress.TestReplacement;
                bool forward = replacement.portA == rule.portA;
                if ((forward ? replacement.labelA : replacement.labelB) != rule.labelAtA ||
                    (forward ? replacement.labelB : replacement.labelA) != rule.labelAtB) return CableRepairProgress.CorrectLabels;
                if (verification == null || !simulation.IsCurrent(verification) || verification.Status != ProbeStatus.Success || verification.ResponderPort != rule.responderPort)
                    return CableRepairProgress.VerifyFromLaptop;
                return CableRepairProgress.Complete;
            }
        }
        public string Status => Progress switch
        {
            CableRepairProgress.TestFaultyCable => "Pendiente: termina la prueba del cable retirado (" + rule.faultyCableId + ") con ambos extremos en el tester.",
            CableRepairProgress.RemoveFaultyCable => "Pendiente: retira ambos extremos del cable averiado.",
            CableRepairProgress.InstallReplacement => "Pendiente: instala un reemplazo íntegro entre " + rule.portA + " y " + rule.portB + ".",
            CableRepairProgress.TestReplacement => "Pendiente: prueba también el reemplazo, desconectado de la red, hasta terminar los LEDs.",
            CableRepairProgress.CorrectLabels => "Pendiente: etiqueta el extremo de " + rule.portA + " como " + rule.labelAtA + " y el de " + rule.portB + " como " + rule.labelAtB + ".",
            CableRepairProgress.VerifyFromLaptop => "Pendiente: ejecuta ping a " + rule.destinationIp + " desde " + rule.probeSource + " después del último cambio. Un ping local de la PC a sí misma no verifica el enlace.",
            _ => "Incidente físico resuelto: reparación y ping verificados."
        };
    }
}
