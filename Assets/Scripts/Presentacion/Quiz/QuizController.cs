using System;
using Core.Quiz.Domain;
using GameData.Quiz;
using UnityEngine;

namespace Presentacion.Quiz
{
    /// <summary>
    /// Coordina una sesion de quiz y su vista.
    /// </summary>
    public sealed class QuizController : MonoBehaviour
    {
        [SerializeField] private QuizView view;
        [Tooltip("Opcional. Permite probar el prefab sin iniciarlo desde un modulo.")]
        [SerializeField] private QuizData previewQuiz;

        private QuizData quiz;
        private QuizSession session;
        private int currentQuestionIndex;
        private bool isConfigured;

        public event Action<QuizResult> QuizCompleted;

        private void Awake()
        {
            if (view == null)
                view = GetComponentInChildren<QuizView>(true);

            if (view == null)
            {
                Debug.LogError("QuizController necesita una QuizView.", this);
                enabled = false;
                return;
            }

            view.AnswerSelected += SelectAnswer;
            view.PreviousRequested += PreviousQuestion;
            view.NextRequested += NextQuestion;
            view.SubmitRequested += Submit;
            view.RetryRequested += Restart;
        }

        private void Start()
        {
            if (!isConfigured && previewQuiz != null)
                Configure(previewQuiz);
        }

        private void OnDestroy()
        {
            if (view == null)
                return;

            view.AnswerSelected -= SelectAnswer;
            view.PreviousRequested -= PreviousQuestion;
            view.NextRequested -= NextQuestion;
            view.SubmitRequested -= Submit;
            view.RetryRequested -= Restart;
        }

        public void Configure(QuizData quizData)
        {
            if (quizData == null)
            {
                Debug.LogError("No se puede iniciar el quiz sin QuizData.", this);
                return;
            }

            quiz = quizData;

            try
            {
                session = new QuizSession(quiz);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError(exception.Message, quiz);
                return;
            }

            currentQuestionIndex = 0;
            isConfigured = true;
            RenderCurrentQuestion();
        }

        private void SelectAnswer(int optionIndex)
        {
            if (!isConfigured)
                return;

            session.SelectAnswer(currentQuestionIndex, optionIndex);
            view.SetSelectedOption(optionIndex);
        }

        private void PreviousQuestion()
        {
            if (!isConfigured || currentQuestionIndex == 0)
                return;

            currentQuestionIndex--;
            RenderCurrentQuestion();
        }

        private void NextQuestion()
        {
            if (!CanLeaveCurrentQuestion())
                return;

            if (currentQuestionIndex >= session.QuestionCount - 1)
                return;

            currentQuestionIndex++;
            RenderCurrentQuestion();
        }

        private void Submit()
        {
            if (!CanLeaveCurrentQuestion())
                return;

            if (!session.AreAllQuestionsAnswered)
            {
                view.ShowMessage("Debes responder todas las preguntas.");
                return;
            }

            QuizResult result = session.CalculateResult();
            view.ShowResult(result);
            QuizCompleted?.Invoke(result);
        }

        private void Restart()
        {
            if (quiz != null)
                Configure(quiz);
        }

        private bool CanLeaveCurrentQuestion()
        {
            if (!isConfigured)
                return false;

            if (session.GetSelectedAnswer(currentQuestionIndex) != QuizSession.Unanswered)
                return true;

            view.ShowMessage("Selecciona una respuesta para continuar.");
            return false;
        }

        private void RenderCurrentQuestion()
        {
            view.ShowQuestion(
                quiz.Title,
                quiz.Questions[currentQuestionIndex],
                currentQuestionIndex,
                session.QuestionCount,
                session.GetSelectedAnswer(currentQuestionIndex));
        }
    }
}
