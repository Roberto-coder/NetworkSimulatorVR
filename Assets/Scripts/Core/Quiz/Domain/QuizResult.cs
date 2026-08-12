namespace Core.Quiz.Domain
{
    /// <summary>
    /// Resultado inmutable de un intento de quiz.
    /// </summary>
    public readonly struct QuizResult
    {
        public QuizResult(
            int correctAnswers,
            int totalQuestions,
            int passingPercentage)
        {
            CorrectAnswers = correctAnswers;
            TotalQuestions = totalQuestions;
            Percentage = totalQuestions == 0
                ? 0f
                : correctAnswers * 100f / totalQuestions;
            Passed = Percentage >= passingPercentage;
        }

        public int CorrectAnswers { get; }

        public int TotalQuestions { get; }

        public float Percentage { get; }

        public bool Passed { get; }
    }
}
