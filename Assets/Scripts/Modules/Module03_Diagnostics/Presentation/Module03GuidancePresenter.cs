using System.Collections;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module03_Diagnostics.Presentation.Tutorial;
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
        [SerializeField, TextArea] private string tutorialStatus;
        public string TutorialStatus => tutorialStatus;
        internal void SetTutorialStatus(string value) => tutorialStatus = value;
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
            director.TutorialCompleted += OnTutorialCompleted;
            ready = true;
            RestartSequence();
        }
        private void RestartSequence()
        {
            StopAllCoroutines(); director.StopTutorial();
            sequenceComplete = false; UnlockedStage = -1; wasEnabled = tutorialEnabled;
            if (!tutorialEnabled) { tutorialStatus = "Tutorial desactivado"; return; }
            if (initialWaypoint == null || laptopEntryWaypoint == null || laptopWaypoint == null || cableWaypoint == null || quizWaypoint == null || director.MovementController == null)
            {
                Debug.LogError("M3 tutorial: asigna los cinco waypoints con Actualizar recorrido y presentación del tutorial. Se libera la práctica sin narración.", this);
                tutorialEnabled = wasEnabled = false; return;
            }
            director.SetFlowController(flow.FlowController);
            director.SetSequence(new Module03TutorialBuilder().Build(this));
            director.StartTutorial();
        }
        private void OnTutorialCompleted()
        {
            sequenceComplete = true;
            tutorialStatus = "Tutorial terminado; recorrido al quiz completado";
        }
        internal void Unlock(DiagnosticStage stage) => UnlockedStage = Mathf.Max(UnlockedStage, (int)stage);
        internal IEnumerator Say(string id, System.Func<bool> completed = null)
        {
            var line = flow.settings.Find(id);
            if (line == null) { Debug.LogError("Falta diálogo " + id + " en Module03Tutorial.asset", this); yield break; }
            yield return director.DialogueController.ShowDialogueUntilConfirmed(line.text, line.speaker,
                completed, line.audio, line.legacyVoiceId);
        }
        internal IEnumerator WaitFor(System.Func<bool> condition, string reminder)
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
        private void OnDestroy()
        {
            if (flow != null) flow.Restarted -= RestartSequence;
            if (director != null) director.TutorialCompleted -= OnTutorialCompleted;
        }
    }
}
