using System.Collections;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Flow;
using Presentacion.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Waypoints;
namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Secuencia narrativa: cada espera observa evidencia, nunca completa objetivos por diálogo.</summary>
    public sealed class Module03GuidancePresenter : MonoBehaviour
    {
        [Header("Tutorial opcional")]
        public bool tutorialEnabled = true;
        [Min(30)] public float reminderInterval = 75f;
        public Module03GuidedFlow flow;
        public TutorialDirector director;
        [Header("Recorrido guardado en escena")]
        public Waypoint initialWaypoint, laptopEntryWaypoint, laptopWaypoint, cableWaypoint, quizWaypoint;
        // Compatibilidad con escenas que aún conservan el antiguo panel de ayudas.
        [HideInInspector] public Button nextHelp, listenHelp, repeatInstruction, skip, restart;
        [HideInInspector] public TMP_Text helpTitle, progress;
        public int UnlockedStage { get; private set; } = -1;
        public bool ReadyForQuiz => !isActiveAndEnabled || !tutorialEnabled || sequenceComplete;
        private bool ready, wasEnabled, sequenceComplete, paused;
        private AudioSource[] voices;
        private IEnumerator Start()
        {
            while (flow != null && flow.isActiveAndEnabled && flow.FlowController == null) yield return null;
            if (flow == null || flow.FlowController == null || director == null || director.DialogueController == null)
            { Debug.LogError("M3 tutorial: faltan flujo o director. Revisa el configurador.", this); enabled = false; yield break; }
            flow.guidance = this;
            if (progress != null) progress.GetComponentInParent<Canvas>()?.gameObject.SetActive(false);
            voices = director.transform.root.GetComponentsInChildren<AudioSource>(true);
            flow.Restarted += RestartSequence;
            ready = true;
            RestartSequence();
        }
        private void RestartSequence()
        {
            StopAllCoroutines(); director.StopTutorial();
            sequenceComplete = false; UnlockedStage = -1; wasEnabled = tutorialEnabled;
            if (!tutorialEnabled) return;
            if (initialWaypoint == null || laptopEntryWaypoint == null || laptopWaypoint == null || cableWaypoint == null || quizWaypoint == null || director.MovementController == null)
            {
                Debug.LogError("M3 tutorial: asigna los cinco waypoints con Actualizar recorrido y presentación del tutorial. Se libera la práctica sin narración.", this);
                tutorialEnabled = wasEnabled = false; return;
            }
            director.SetFlowController(flow.FlowController);
            director.SetSequence(new TutorialSequence()); director.StartTutorial();
            StartCoroutine(RunSequence());
        }
        private void Unlock(DiagnosticStage stage) => UnlockedStage = Mathf.Max(UnlockedStage, (int)stage);
        private IEnumerator RunSequence()
        {
            yield return Move(initialWaypoint);
            yield return Say("M3D01"); yield return Say("M3D02");
            yield return Move(laptopEntryWaypoint); yield return Move(laptopWaypoint);
            yield return Say("M3D03");
            Unlock(DiagnosticStage.OpenLaptop);
            yield return Say("M3D04", () => flow.StageIndex > 0);
            yield return WaitFor(() => flow.StageIndex > 0, "M3R01");
            yield return Say("M3D05"); yield return Say("M3D06");
            yield return Say("M3D07"); yield return Say("M3D08");
            yield return Say("M3D09"); yield return Say("M3D10"); yield return Say("M3D11");
            Unlock(DiagnosticStage.FirstPing);
            yield return Say("M3D12", () => flow.StageIndex > 1);
            yield return WaitFor(() => flow.StageIndex > 1, "M3R02");
            yield return Say("M3D13"); yield return Say("M3D14");
            yield return Move(laptopEntryWaypoint); yield return Move(cableWaypoint);
            yield return Say("M3D15"); yield return Say("M3D16");
            Unlock(DiagnosticStage.Cable); // El presenter habilita conectores y muestra el repuesto.
            yield return Say("M3D17", () => flow.PhysicalRepairReady);
            yield return WaitFor(() => flow.PhysicalRepairReady, "M3R03");
            yield return Say("M3D18");
            yield return Move(laptopEntryWaypoint); yield return Move(laptopWaypoint);
            yield return Say("M3D19", () => flow.StageIndex > 2);
            yield return WaitFor(() => flow.StageIndex > 2, "M3R04");
            yield return Say("M3D20"); yield return Say("M3D21");
            Unlock(DiagnosticStage.Ports);
            yield return Say("M3D22", () => flow.StageIndex > 3);
            yield return WaitFor(() => flow.StageIndex > 3, "M3R05");
            yield return Say("M3D23");
            Unlock(DiagnosticStage.Addresses);
            yield return Say("M3D24", () => flow.StageIndex > 4);
            yield return WaitFor(() => flow.StageIndex > 4, "M3R06");
            Unlock(DiagnosticStage.FinalVerification);
            yield return Say("M3D25", flow.CanFinishPractice);
            yield return WaitFor(flow.CanFinishPractice, "M3R07");
            yield return Say("M3D26");
            yield return Move(laptopEntryWaypoint); yield return Move(quizWaypoint);
            sequenceComplete = true; // El cierre abre el quiz sólo al terminar este recorrido.
        }
        private IEnumerator Move(Waypoint waypoint)
        {
            director.MovementController.MoveTo(waypoint);
            while (director.MovementController.IsMoving) yield return null;
        }
        private IEnumerator Say(string id, System.Func<bool> completed = null)
        {
            var line = flow.settings.Find(id);
            if (line == null) { Debug.LogError("Falta diálogo " + id + " en Module03Tutorial.asset", this); yield break; }
            yield return director.DialogueController.ShowDialogueUntilConfirmed(line.text, line.speaker,
                completed, line.audio, line.legacyVoiceId);
        }
        private IEnumerator WaitFor(System.Func<bool> condition, string reminder)
        {
            float next = Time.time + reminderInterval;
            while (!condition())
            {
                if (Time.timeScale > 0 && Time.time >= next)
                {
                    var line = flow.settings.Find(reminder);
                    if (line != null) yield return director.DialogueController.ShowTransientDialogue(line.text,
                        Mathf.Max(5, line.text.Length / 15f), line.speaker, line.audio?.Resolve(), line.legacyVoiceId);
                    next = Time.time + reminderInterval;
                }
                yield return null;
            }
        }
        private void Update()
        {
            if (!ready) return;
            if (wasEnabled != tutorialEnabled) RestartSequence();
            bool pause = Time.timeScale <= 0;
            if (paused == pause) return;
            paused = pause;
            foreach (var voice in voices) { if (pause) voice.Pause(); else voice.UnPause(); }
        }
        private void OnEnable() { if (ready) RestartSequence(); }
        private void OnDisable() { StopAllCoroutines(); if (director != null) director.StopTutorial(); }
        private void OnDestroy() { if (flow != null) flow.Restarted -= RestartSequence; }
    }
}
