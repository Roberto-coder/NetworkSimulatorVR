using Modules.Module02_RackInstallation.Flow.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Spawning;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Interaction;
using Modules.Module02_RackInstallation.Presentation;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Shared.Cabling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Flow
{
    /// <summary>Integra la práctica; el cierre del quiz se encarga de la insignia y el guardado.</summary>
    public sealed class Module02SequenceCoordinator : MonoBehaviour
    {
        public static Module02SequenceCoordinator Instance { get; private set; }
        [SerializeField] private Module02Manager manager;
        [SerializeField] private RackInsertionSlot slot;
        [SerializeField] private GameObject installationZone;
        [SerializeField] private RackFasteningAssembly fastening;
        [SerializeField] private Module02CablingState cabling;
        [SerializeField] private Module02LabelingState labeling;
        [SerializeField] private Module02SwitchPower power;
        [SerializeField] private Module02SwitchConfiguration configuration;
        [SerializeField] private InfoCardController cards;
        [SerializeField] private Button confirmReview;
        [SerializeField] private TMP_Text status;
        [SerializeField] private RackInfoTarget[] rackTargets;
        [SerializeField] private RackInfoTarget[] switchTargets;
        [SerializeField] private ObjectSpawner[] cableSpawners;
        [Header("Depuración (solo Editor / Development Build)")]
        [SerializeField] private bool skipRackInspection;
        private readonly HashSet<string> reviewed = new();
        private readonly Dictionary<string, HashSet<int>> pages = new();
        private readonly List<PatchCableLink> spawnedCables = new();
        private Module02FlowController flow;
        private bool spawned, locked;
        private int step = -1;
        private Module02SequencePresenter presenter;
        public bool CablesLocked => locked;
        public event Action PracticalCompleted;
        public event Action<string> ActionRejected;

        private void Awake()
        {
            if (Instance != null && Instance != this) { enabled = false; return; }
            Instance = this;
        }
        private void Start()
        {
            flow = manager != null ? manager.FlowController : null;
            if (flow == null) { Debug.LogError("Falta el flujo del módulo 2.", this); enabled = false; return; }
            if (flow.ModuleDefinition.Objectives.Count != 9 || flow.ModuleDefinition.Objectives.Where((d, i) => d.Id != Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[i]).Any())
            { Debug.LogError("La secuencia requiere los nueve objetivos del sprint 7 en orden.", this); enabled = false; return; }
            flow.CompletionGuard = IsSatisfied;
            flow.CurrentObjectiveChanged += OnObjective;
            flow.PracticalObjectivesCompleted += OnPracticalComplete;
            cards.PageShown += OnPage;
            power.StateChanged += OnPowerChanged;
            // Mantener estos campos serializados conserva las referencias de escenas existentes.
            presenter = new Module02SequencePresenter(confirmReview, status, ConfirmCard);
            // Los tres monitores comienzan sin cables: las plantillas de escena no son progreso real.
            cabling.SetCables(Array.Empty<PatchCableLink>());
            labeling.SetCables(Array.Empty<CableLabelPair>());
            configuration.SetCables(Array.Empty<PatchCableLink>());
            flow.Begin(); OnObjective(flow.CurrentObjectiveData);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (skipRackInspection && step == 0)
            {
                foreach (var id in RequiredIds()) reviewed.Add(id);
                flow.TryCompleteCurrent("inspect_rack");
            }
#endif
        }
        private void OnDisable()
        {
            if (Instance != this) return;
            if (flow != null)
            {
                flow.CurrentObjectiveChanged -= OnObjective; flow.PracticalObjectivesCompleted -= OnPracticalComplete;
                // Si se desactiva el coordinador de una práctica integrada, no permitir avances sin validar.
                flow.CompletionGuard = _ => false;
            }
            if (cards != null) cards.PageShown -= OnPage;
            if (power != null) power.StateChanged -= OnPowerChanged;
            presenter?.Dispose();
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void OnObjective(ObjectiveData _)
        {
            step = flow.ArePracticalObjectivesCompleted ? 9 : flow.CurrentObjectiveIndex;
            cards.Close();
            foreach (var target in rackTargets) if (target != null) target.enabled = step == 0;
            foreach (var target in switchTargets) if (target != null) target.enabled = step == 1;
            installationZone.SetActive(step >= 2);
            if (step >= 4 && !spawned) SpawnAndRegister();
        }
        private void SpawnAndRegister()
        {
            // ObjectSpawner conserva su propia instancia. Los monitores reciben referencias
            // a componentes de esas instancias, nunca al prefab ni a las plantillas desactivadas.
            foreach (var spawner in cableSpawners)
            {
                spawner.Spawn();
                var link = spawner.CurrentInstance.GetComponent<PatchCableLink>();
                if (link == null || link.GetComponent<CableLabelPair>() == null)
                    throw new InvalidOperationException("Cada cable generado requiere PatchCableLink y CableLabelPair.");
                spawnedCables.Add(link);
            }
            if (spawnedCables.Count != 5 || spawnedCables.Distinct().Count() != 5)
                throw new InvalidOperationException("Se necesitan cinco instancias únicas.");
            cabling.SetCables(spawnedCables);
            labeling.SetCables(spawnedCables.Select(c => c.GetComponent<CableLabelPair>()));
            configuration.SetCables(spawnedCables);
            spawned = true;
        }
        private string[] RequiredIds() => (step == 0 ? rackTargets : switchTargets)
            .Where(t => t != null && t.Information != null).Select(t => t.Information.Id).Distinct().ToArray();
        private void OnPage(RackInfoTarget target, int page)
        {
            if (step > 1 || target == null || !RequiredIds().Contains(target.Information.Id)) return;
            if (!pages.TryGetValue(target.Information.Id, out var seen)) pages[target.Information.Id] = seen = new HashSet<int>();
            seen.Add(page);
        }
        private bool CanConfirm()
        {
            var target = cards.DisplayedTarget;
            return step >= 0 && step <= 1 && target != null && target.Information != null &&
                RequiredIds().Contains(target.Information.Id) && !reviewed.Contains(target.Information.Id) &&
                pages.TryGetValue(target.Information.Id, out var seen) && seen.Count == target.Information.Sections.Count + 1;
        }
        private void ConfirmCard()
        {
            if (!CanConfirm()) return;
            reviewed.Add(cards.DisplayedTarget.Information.Id); cards.Close();
        }
        private bool ValidWiring()
        {
            if (!spawned) return false;
            cabling.RefreshState(); labeling.RefreshState();
            return cabling.IsComplete && labeling.IsComplete;
        }
        private ConsoleTerminalTool ConnectedConsole() => FindObjectsByType<ConsoleTerminalTool>(FindObjectsSortMode.None)
            .FirstOrDefault(t => { t.RefreshConnection(); return t.isActiveAndEnabled && t.Power == power && t.IsConnected; });
        private bool IsSatisfied(string id)
        {
            if (step < 0 || step >= 9 || id != Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[step]) return false;
            switch (step)
            {
                case 0: case 1: var required = RequiredIds(); return required.Length > 0 && required.All(reviewed.Contains);
                case 2: return slot.IsInstalled;
                case 3: return fastening.IsSecured;
                case 4: cabling.RefreshState(); return cabling.IsComplete;
                case 5: return ValidWiring();
                case 6: return ValidWiring() && ConnectedConsole() != null;
                case 7: return ValidWiring() && power.IsOn;
                case 8: return ValidWiring() && configuration.IsConfigured;
                default: return false;
            }
        }
        private void Update()
        {
            if (flow == null || step < 0) return;
            // Cerrar el agarre justo tras el encendido, sin desactivar Connector ni sus sockets.
            if (!locked && step >= 7 && power.IsOn && ValidWiring()) LockInstallationCables();
            if (step < 9 && IsSatisfied(Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[step]))
                flow.TryCompleteCurrent(Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[step]);
            var required = step <= 1 ? RequiredIds() : Array.Empty<string>();
            presenter?.Show(step, 9, flow.CurrentObjectiveData?.Title, CanConfirm(),
                required.Count(reviewed.Contains), required.Length, fastening.FastenedCount,
                cabling.ConnectedCount, labeling.LabeledEndCount);
        }
        private void LockInstallationCables()
        {
            locked = true;
            foreach (var cable in spawnedCables)
            {
                var physics = cable.GetComponent<HPhysic.PhysicCable>();
                foreach (var end in new[] { physics.StartConnector, physics.EndConnector })
                {
                    if (end == null) continue;
                    var grab = end.GetComponent<XRGrabInteractable>(); if (grab != null) grab.enabled = false;
                }
            }
        }
        private void OnPowerChanged()
        {
            if (!locked && step >= 7 && power.IsOn && ValidWiring()) LockInstallationCables();
        }
        private void OnPracticalComplete() { step = 9; PracticalCompleted?.Invoke(); }
        public string LastRejectionId { get; private set; }
        private void Reject(string message, string dialogueId)
        {
            LastRejectionId = dialogueId;
            presenter?.ShowRejection(message);
            ActionRejected?.Invoke(message);
        }
        public static bool AllowSwitchGrab()
        {
            var self = Instance;
            // Revisar las fichas requiere poder girar el equipo. El montaje mantiene
            // su permiso independiente para impedir insertarlo antes del objetivo 3.
            return self == null || (self.isActiveAndEnabled && (self.step == 1 || self.step == 2));
        }
        public static bool Allow(Module02Action action, bool feedback = false)
        {
            var self = Instance;
            if (self == null) return true; // Las escenas de prueba de sprints anteriores siguen siendo independientes.
            bool allowed = self.isActiveAndEnabled && Module02SequenceRules.Allows(self.step, action, self.locked);
            if (!allowed && feedback) self.Reject("Esta acción aún no corresponde al objetivo actual.", "alert_wrong_action");
            return allowed;
        }
        public static bool AllowConnection(Connector first, Connector second)
        {
            if (Instance == null) return true;
            var port = first.GetComponentInParent<NetworkPort>() ?? second.GetComponentInParent<NetworkPort>();
            if (port == null || port.DeviceId == "TERM1") return true; // Unión interna permanente de la consola.
            return Allow(port.Kind == NetworkPortKind.ConsoleRj45 ? Module02Action.Console : Module02Action.Wire);
        }
        public static bool AllowPower()
        {
            var self = Instance; if (self == null) return true;
            if (!self.isActiveAndEnabled) return false;
            if (Allow(Module02Action.Power) && (self.power.IsOn || self.ValidWiring())) return true;
            self.Reject("Realice el cableado y etiquetado correctamente antes de continuar", "alert_wiring"); return false;
        }
    }
}
