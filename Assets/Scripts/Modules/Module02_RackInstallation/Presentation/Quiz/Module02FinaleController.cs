using System.Collections;
using Framework.Spawning;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Presentation.Tutorial;
using Presentacion.Quiz;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Module02_RackInstallation.Presentation.Quiz
{
    /// <summary>Presenta el quiz tras la práctica y el recorrido opcional del NPC.</summary>
    public sealed class Module02FinaleController : MonoBehaviour
    {
        [SerializeField] private Module02Manager manager;
        [SerializeField] private ObjectSpawner quizSpawner;
        [SerializeField] private Module02TutorialController tutorial;
        private Module02FlowController flow;
        private float startedAt;
        private bool requested;
        private void Awake() => startedAt = Time.realtimeSinceStartup;
        private void Start()
        {
            flow = manager != null ? manager.FlowController : null;
            if (flow == null || quizSpawner == null || flow.ModuleDefinition.FinalQuiz == null)
            { Debug.LogError("El cierre del módulo 2 requiere flujo, spawner y Module02Quiz.", this); enabled = false; return; }
            flow.PracticalObjectivesCompleted += RequestQuiz;
            if (flow.ArePracticalObjectivesCompleted) RequestQuiz();
        }
        private void OnDisable()
        {
            if (flow != null) flow.PracticalObjectivesCompleted -= RequestQuiz;
            StopAllCoroutines();
        }
        private void RequestQuiz()
        {
            if (requested) return;
            requested = true;
            StartCoroutine(ShowQuiz());
        }
        private IEnumerator ShowQuiz()
        {
            // El mismo cierre funciona sin NPC o si el tutorial se desactivó.
            while (tutorial != null && tutorial.IsGuiding) yield return null;
            quizSpawner.Spawn();
            var root = quizSpawner.CurrentInstance;
            var quiz = root.GetComponentInChildren<QuizController>(true);
            var actions = root.GetComponentInChildren<Module02QuizActionsView>(true);
            if (quiz == null || actions == null)
            { Debug.LogError("Usa QuizCanvas_Module02_XRI, con controlador y acciones de resultado.", this); yield break; }
            var completion = root.AddComponent<Module02QuizCompletionController>();
            completion.Configure(quiz, actions, flow, startedAt, gameObject.scene.name);
            quiz.Configure(flow.ModuleDefinition.FinalQuiz);
        }
    }
}
