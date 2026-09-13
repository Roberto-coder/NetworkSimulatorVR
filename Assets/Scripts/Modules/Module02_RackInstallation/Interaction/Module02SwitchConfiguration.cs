using Modules.Module02_RackInstallation.Flow.Validation;
using Modules.Module02_RackInstallation.Domain;
using System;
using System.Collections.Generic;
using Shared.Cabling;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    [RequireComponent(typeof(Module02SwitchPower))]
    public sealed class Module02SwitchConfiguration : MonoBehaviour
    {
        [SerializeField] private List<PatchCableLink> cables = new();
        [SerializeField] private List<NetworkPort> switchPorts = new();
        private Module02SwitchPower power;
        public SwitchConfigurationPuzzle Puzzle { get; } = new();
        public bool IsConfigured => isActiveAndEnabled && power != null && power.IsOn && Puzzle.IsConfigured;
        public event Action ConfigurationChanged;
        public void SetCables(IEnumerable<PatchCableLink> instances)
        {
            UpdateLinks(false);
            cables = new List<PatchCableLink>(new HashSet<PatchCableLink>(instances));
        }
        private void Awake() => power = GetComponent<Module02SwitchPower>();
        private void OnEnable() { power.StateChanged += PowerChanged; PowerChanged(); }
        private void OnDisable()
        {
            power.StateChanged -= PowerChanged;
            Puzzle.Reset(); UpdateLinks(false); ConfigurationChanged?.Invoke();
        }
        private void PowerChanged()
        {
            // La configuración vive en el switch, no en la herramienta que se destruye al guardarla.
            // Solo apagar o perder alimentación reinicia también los bloques parcialmente colocados.
            if (!power.IsOn) { Puzzle.Reset(); UpdateLinks(false); ConfigurationChanged?.Invoke(); }
        }
        private bool CanEdit(ConsoleTerminalTool terminal)
        {
            if (!Flow.Module02SequenceCoordinator.Allow(Module02Action.Configure, true)) return false;
            // Volver a leer la unión física evita aplicar con IsReady del frame anterior.
            if (terminal != null) terminal.RefreshConnection();
            power.RefreshSupply();
            return isActiveAndEnabled && terminal != null && terminal.isActiveAndEnabled &&
                terminal.IsReady && terminal.Power == power && power.IsOn;
        }
        public bool Place(ConsoleTerminalTool terminal, int block, int slot)
        {
            if (!CanEdit(terminal) || !Puzzle.Place(block, slot)) return false;
            ConfigurationChanged?.Invoke(); return true;
        }
        public bool Apply(ConsoleTerminalTool terminal)
        {
            if (!CanEdit(terminal)) return false;
            bool result = Puzzle.Apply(); UpdateLinks(IsConfigured); ConfigurationChanged?.Invoke(); return result;
        }
        private void LateUpdate() { power.RefreshSupply(); UpdateLinks(IsConfigured); }
        private void UpdateLinks(bool configured)
        {
            // Evaluar cada puerto permite retirar un enlace sin apagar los otros tres.
            foreach (var port in switchPorts)
            {
                if (port == null) continue;
                bool active = false;
                foreach (var cable in cables)
                    if (Correct(cable, configured) && (cable.StartPort == port || cable.EndPort == port)) { active = true; break; }
                port.SetOperationalLink(active);
            }
            foreach (var cable in cables)
            {
                if (cable == null) continue;
                var traffic = cable.GetComponent<CableTrafficVisualizer>();
                if (traffic != null) traffic.SetTransmissionEnabled(Correct(cable, configured));
            }
        }
        private static bool Correct(PatchCableLink cable, bool configured)
        {
            if (!configured || cable == null || !cable.isActiveAndEnabled || cable.Kind != NetworkPortKind.EthernetRj45) return false;
            cable.RefreshLink();
            if (!cable.HasCompleteLink) return false;
            // El índice 0 de la tabla es alimentación: únicamente 1–4 transportan paquetes.
            return Module02ConnectionPlan.FindMatch(cable.StartPort.Address, cable.EndPort.Address,
                cable.Kind, cable.StartPort.Kind, cable.EndPort.Kind) > 0;
        }
    }
}
