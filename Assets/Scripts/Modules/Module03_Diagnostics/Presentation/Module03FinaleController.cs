using System.Collections;
using Core.Quiz.Domain;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module02_RackInstallation.Presentation.Quiz;
using Presentacion.Quiz;
using Systems.Scenes;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Cierre independiente del tutorial. El Canvas se coloca y guarda en Editor.</summary>
    public sealed class Module03FinaleController : MonoBehaviour
    {
        public Module03GuidedFlow flow;
        public GameObject quizRoot;
        public QuizController quiz;
        public Module02QuizActionsView actions; // Vista reutilizable: solo botones, no lógica del módulo 2.
        public Module03GuidancePresenter guidance;
        [SerializeField, TextArea(2, 5)] private string closureStatus = "Inicializando cierre";
        public string ClosureStatus => closureStatus;
        private bool ready, opened, submitted, saved;
        private float startedAt, completedTime;
        private void Awake() { startedAt = Time.realtimeSinceStartup; if (quizRoot != null) quizRoot.SetActive(false); }
        private IEnumerator Start()
        {
            while (flow != null && flow.isActiveAndEnabled && flow.FlowController == null) yield return null;
            if (flow == null || flow.FlowController == null || quizRoot == null || quiz == null || actions == null ||
                flow.settings.module.FinalQuiz == null || !flow.settings.module.FinalQuiz.IsValid || flow.settings.module.CompletionAchievement == null)
            {
                closureStatus = "Faltan referencias, quiz válido o insignia. Reaplica Sprint 9 - Quiz y cierre.";
                Debug.LogError("M3 cierre: " + closureStatus, this); enabled = false; yield break;
            }
            quiz.QuizCompleted += Submit;
            quiz.FinishRequested += Lobby;
            actions.RestartModuleRequested += Reload;
            actions.RetrySaveRequested += Save;
            flow.Restarted += ResetFinale;
            ready = true;
        }
        private void Update()
        {
            if (!ready || saved) return;
            if (Time.timeScale <= 0) { closureStatus = "Pausa"; return; }
            bool valid = flow.CanFinishPractice();
            if (opened && !valid) { ResetFinale(); return; }
            if (opened) { closureStatus = "Quiz activo"; return; }
            if (!valid)
            {
                closureStatus = flow.FlowController.PracticeCompleted
                    ? "Falta revalidar la red: repite los pings desde la laptop tras el último cambio. " + flow.repair.Service.Status
                    : "Objetivo pendiente: " + flow.FlowController.CurrentObjectiveData?.Description;
                return;
            }
            if (guidance != null && !guidance.ReadyForQuiz)
            { closureStatus = "Esperando tutorial — " + guidance.TutorialStatus; return; }
            OpenQuiz();
        }
        private void OpenQuiz()
        {
            // El tutorial ya terminó o está omitido. No deshabilitarlo/reiniciarlo al volver a verificar.
            flow.screens.Close();
            if (flow.tools != null) flow.tools.UnequipTool();
            quizRoot.SetActive(true);
            if (!quizRoot.activeInHierarchy)
            {
                closureStatus = "El Canvas está activo, pero un padre está desactivado: " + quizRoot.transform.parent?.name;
                return;
            }
            quiz.Configure(flow.settings.module.FinalQuiz);
            actions.ShowSaveState(false, "Responde y entrega el quiz para registrar la finalización.", false);
            opened = true;
            closureStatus = "Quiz activo";
        }
        private void Submit(QuizResult result)
        {
            if (saved) { Save(); return; }
            if (!opened || !quiz.HasSubmitted || Time.timeScale <= 0 || !flow.CanFinishPractice())
            { ResetFinale(); return; }
            // Como en los módulos anteriores, el logro acredita práctica + entrega; la nota es formativa.
            if (!submitted) completedTime = Mathf.Max(0, Time.realtimeSinceStartup - startedAt);
            submitted = true;
            Save();
        }
        private void Save()
        {
            if (!submitted || !quiz.HasSubmitted || Time.timeScale <= 0) return;
            if (!saved)
            {
                if (!flow.CanFinishPractice()) { ResetFinale(); return; }
                if (SaveManager.Instance == null) new GameObject("SaveManager").AddComponent<SaveManager>();
                var module = flow.settings.module;
                if (!SaveManager.Instance.TryCompleteModuleLocally(module.ModuleId, module.ModuleName,
                    module.CompletionAchievement, module.Objectives.Count, completedTime, out var error))
                { actions.ShowSaveState(false, error, true); return; }
                saved = true;
            }
            quiz.ShowAchievement(flow.settings.module.CompletionAchievement);
            actions.ShowSaveState(true, "Módulo completado. Progreso guardado en este dispositivo.", false);
        }
        private void ResetFinale()
        {
            opened = submitted = saved = false;
            quizRoot.SetActive(false);
            closureStatus = "Esperando práctica verificada";
        }
        private void Lobby() => Navigate("Lobby");
        private void Reload() => Navigate(gameObject.scene.name);
        private void Navigate(string scene)
        {
            if (!saved || !quiz.HasSubmitted || SceneTransitionManager.IsLoading || Time.timeScale <= 0) return;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            { actions.ShowSaveState(true, "Habilita la escena " + scene + " en Build Settings.", false); return; }
            SceneTransitionManager.LoadScene(scene);
        }
        private void OnDestroy()
        {
            if (!ready) return;
            quiz.QuizCompleted -= Submit; quiz.FinishRequested -= Lobby;
            actions.RestartModuleRequested -= Reload; actions.RetrySaveRequested -= Save;
            flow.Restarted -= ResetFinale;
        }
    }
}
