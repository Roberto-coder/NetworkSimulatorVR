using System.Collections;
using System.Linq;
using GameData.Module03;
using Framework.Interaction.Tools;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using Presentacion.GlobalUI.ObjectivesWristMenu;
using UnityEngine;
namespace Modules.Module03_Diagnostics.Flow
{
    /// <summary>Adaptador entre red viva, evidencia y flujo compartido de objetivos.</summary>
    public sealed class Module03GuidedFlow : MonoBehaviour
    {
        public event System.Action Restarted;
        public static Module03GuidedFlow Instance { get; private set; }
        public Module03NetworkScene network;
        public CableRepairController repair;
        public DiagnosticScreenController screens;
        public Module03FlowSettings settings;
        public ToolManager tools;
        public Module03FlowController FlowController { get; private set; }
        public bool NetworkVerified { get; private set; }
        [SerializeField, TextArea(2, 5)] private string progressStatus;
        public Module03GuidancePresenter guidance;
        private readonly DiagnosticTutorialProgress introduction = new();
        public int IntroObjectiveCount => settings != null && settings.module != null && settings.module.Objectives.Count == 6 ? 2 : 0;
        public int StageIndex => FlowController == null ? 0 : FlowController.Index + (2 - IntroObjectiveCount);
        public bool Allows(DiagnosticStage action) => FlowController != null &&
            DiagnosticTutorialProgress.Allows(action, StageIndex, guidance != null ? guidance.UnlockedStage : 5,
                guidance != null && guidance.isActiveAndEnabled && guidance.tutorialEnabled);
        public bool PhysicalRepairReady => repair != null && repair.Ready &&
            (repair.Service.Progress == CableRepairProgress.VerifyFromLaptop || repair.Service.IsComplete);
        private DiagnosticProgressEvidence evidence;
        private long resetSequence;
        private float nextEvaluation;
        private void Awake() { Instance = this; }
        private IEnumerator Start()
        {
            yield return null;
            if (network == null || network.Session == null || repair == null || screens == null || settings == null || settings.module == null || settings.targets == null)
            { Debug.LogError("M3 flujo: faltan red, reparación, pantallas o GameData.", this); enabled = false; yield break; }
            evidence = new DiagnosticProgressEvidence(network.Session, network.Diagnostics, settings.targets.DocumentedInventory(network.initialState), settings.sourcePort);
            FlowController = new Module03FlowController(settings.module, CanComplete);
            if (tools != null) tools.SetAvailableTools(settings.module.availableTools);
            screens.ProbeExecuted += Observe;
            screens.ScreenOpened += Opened;
            FlowController.Begin();
            foreach (var wrist in FindObjectsByType<WristObjectivesPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (wrist.gameObject.scene == gameObject.scene) wrist.Initialize();
        }
        private void Opened(string deviceId) => introduction.Open(deviceId, settings.sourcePort.Split('/')[0]);
        private void Observe(ProbeResult result, DiagnosticScreenView _)
        {
            evidence.Observe(result);
            introduction.Observe(result, network.Session, settings.sourcePort, repair.settings.incident.destinationIp);
        }
        private void Update()
        {
            if (FlowController == null || Time.timeScale <= 0 || !repair.Ready) return;
            long reset = network.Session.History.LastOrDefault(h => h.Action == "Reset")?.Sequence ?? 0;
            if (reset != resetSequence)
            { resetSequence = reset; NetworkVerified = false; nextEvaluation = 0; evidence.Clear(); introduction.Reset(); FlowController.Reset(); Restarted?.Invoke(); }
            if (Time.time < nextEvaluation) return;
            nextEvaluation = Time.time + 0.2f;
            NetworkVerified = CanComplete(3 + IntroObjectiveCount);
            FlowController.Evaluate();
            progressStatus = StageIndex == 2 ? repair.Service.Status :
                FlowController.CurrentObjectiveData?.Description ?? (NetworkVerified ? "Red verificada." : "Repite las verificaciones tras el último cambio.");
        }
        private bool CanComplete(int index)
        {
            if (evidence == null || repair == null || !repair.Ready) return false;
            if (IntroObjectiveCount == 2)
            {
                if (index == 0) return introduction.LaptopOpened;
                if (index == 1) return introduction.FirstPingExecuted;
                index -= 2;
            }
            var results = settings.targets.Evaluate(network.Session);
            switch (index)
            {
                case 0: return repair.Service.IsComplete;
                case 1: return evidence.ConsultedSwitch(settings.switchId) && network.Session.Snapshot().ports.Any(p => p.id == settings.disabledPort && p.enabled);
                case 2: return settings.addressRuleIds != null && settings.addressRuleIds.Length > 0 &&
                    settings.addressRuleIds.All(id => results.Any(r => r.RuleId == id && r.Passed)) && evidence.Verified(settings.addressDevices);
                case 3: return results.Count > 0 && results.All(r => r.Passed) && repair.Service.IsComplete && evidence.Verified(settings.finalDevices);
                default: return false;
            }
        }
        // Revalidación inmediata para el cierre: no depender del último Update del indicador.
        public bool CanFinishPractice() => FlowController != null && FlowController.PracticeCompleted && CanComplete(3 + IntroObjectiveCount);

        public void RestartPractice()
        {
            if (network == null || network.Session == null || Time.timeScale <= 0) return;
            screens.Close();
            if (tools != null) tools.UnequipTool();
            network.Session.Reset(); // Los adaptadores físicos y visuales observan el reinicio/revisión.
        }
        private void OnDestroy()
        {
            if (screens != null) screens.ProbeExecuted -= Observe;
            if (screens != null) screens.ScreenOpened -= Opened;
            if (Instance == this) Instance = null;
        }
    }
}
