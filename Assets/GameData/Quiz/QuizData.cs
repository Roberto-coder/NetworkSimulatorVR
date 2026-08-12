using System.Collections.Generic;
using UnityEngine;

namespace GameData.Quiz
{
    /// <summary>
    /// Contiene la configuracion y las preguntas del quiz de un modulo.
    /// </summary>
    [CreateAssetMenu(fileName = "QuizData", menuName = "Scriptable Objects/QuizData")]
    public sealed class QuizData : ScriptableObject
    {
        [SerializeField]
        private string quizId;

        [SerializeField]
        private string title;

        [SerializeField, Range(0, 100)]
        private int passingPercentage = 70;

        [SerializeField]
        private List<QuizQuestion> questions = new();

        public string QuizId => quizId;

        public string Title => title;

        public int PassingPercentage => passingPercentage;

        public IReadOnlyList<QuizQuestion> Questions => questions;

        /// <summary>
        /// Comprueba que el quiz y todas sus preguntas puedan utilizarse.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(quizId) ||
                    questions == null ||
                    questions.Count == 0)
                {
                    return false;
                }

                foreach (QuizQuestion question in questions)
                {
                    if (question == null || !question.IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(quizId))
            {
                Debug.LogWarning($"El quiz '{name}' necesita un identificador.", this);
            }

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning($"El quiz '{name}' no contiene preguntas.", this);
                return;
            }

            for (int index = 0; index < questions.Count; index++)
            {
                if (questions[index] == null || !questions[index].IsValid)
                {
                    Debug.LogWarning(
                        $"La pregunta {index + 1} del quiz '{name}' esta incompleta.",
                        this);
                }
            }
        }
#endif
    }
}
