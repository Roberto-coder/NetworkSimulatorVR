using System;
using GameData.Quiz;

namespace Core.Quiz.Domain
{
    /// <summary>
    /// Mantiene las respuestas de un intento y calcula su resultado.
    /// No modifica el ScriptableObject utilizado como fuente de datos.
    /// </summary>
    public sealed class QuizSession
    {
        public const int Unanswered = -1;

        private readonly QuizData quiz;
        private readonly int[] selectedAnswers;

        public QuizSession(QuizData quiz)
        {
            this.quiz = quiz != null
                ? quiz
                : throw new ArgumentNullException(nameof(quiz));

            if (!quiz.IsValid)
            {
                throw new ArgumentException(
                    "El quiz contiene datos incompletos o invalidos.",
                    nameof(quiz));
            }

            selectedAnswers = new int[quiz.Questions.Count];
            for (int index = 0; index < selectedAnswers.Length; index++)
            {
                selectedAnswers[index] = Unanswered;
            }
        }

        public int QuestionCount => selectedAnswers.Length;

        public bool AreAllQuestionsAnswered
        {
            get
            {
                foreach (int selectedAnswer in selectedAnswers)
                {
                    if (selectedAnswer == Unanswered)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public int GetSelectedAnswer(int questionIndex)
        {
            ValidateQuestionIndex(questionIndex);
            return selectedAnswers[questionIndex];
        }

        public void SelectAnswer(int questionIndex, int optionIndex)
        {
            ValidateQuestionIndex(questionIndex);

            QuizQuestion question = quiz.Questions[questionIndex];
            if (optionIndex < 0 || optionIndex >= question.Options.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(optionIndex),
                    "La opcion no pertenece a la pregunta indicada.");
            }

            selectedAnswers[questionIndex] = optionIndex;
        }

        public QuizResult CalculateResult()
        {
            if (!AreAllQuestionsAnswered)
            {
                throw new InvalidOperationException(
                    "No se puede calcular el resultado hasta responder todas las preguntas.");
            }

            int correctAnswers = 0;
            for (int index = 0; index < selectedAnswers.Length; index++)
            {
                if (selectedAnswers[index] == quiz.Questions[index].CorrectOptionIndex)
                {
                    correctAnswers++;
                }
            }

            return new QuizResult(
                correctAnswers,
                selectedAnswers.Length,
                quiz.PassingPercentage);
        }

        private void ValidateQuestionIndex(int questionIndex)
        {
            if (questionIndex < 0 || questionIndex >= selectedAnswers.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(questionIndex));
            }
        }
    }
}
