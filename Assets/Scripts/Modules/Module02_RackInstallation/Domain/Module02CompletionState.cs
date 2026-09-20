namespace Modules.Module02_RackInstallation.Domain
{
    /// <summary>Separa finalizar la práctica de entregar el quiz; la nota no bloquea la completitud.</summary>
    public sealed class Module02CompletionState
    {
        public bool PracticeCompleted { get; private set; }
        public bool QuizCompleted { get; private set; }
        public bool CompletePractice()
        {
            if (PracticeCompleted) return false;
            PracticeCompleted = true;
            return true;
        }
        public bool CompleteQuiz()
        {
            if (!PracticeCompleted || QuizCompleted) return false;
            QuizCompleted = true;
            return true;
        }
    }
}
