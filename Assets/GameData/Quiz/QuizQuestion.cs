using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData.Quiz
{
    /// <summary>
    /// Define una pregunta de opcion multiple y su respuesta correcta.
    /// </summary>
    [Serializable]
    public sealed class QuizQuestion
    {
        [SerializeField, TextArea(2, 5)]
        private string statement;

        [SerializeField]
        private List<string> options = new();

        [SerializeField, Min(0)]
        private int correctOptionIndex;

        public string Statement => statement;

        public IReadOnlyList<string> Options => options;

        public int CorrectOptionIndex => correctOptionIndex;

        /// <summary>
        /// Indica si la pregunta contiene los datos minimos necesarios.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(statement) &&
            options != null &&
            options.Count >= 2 &&
            correctOptionIndex >= 0 &&
            correctOptionIndex < options.Count;
    }
}
