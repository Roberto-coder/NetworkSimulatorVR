using System.Collections.Generic;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Representa una secuencia ordenada de pasos del tutorial.
    /// En el futuro podrá convertirse en un ScriptableObject.
    /// </summary>
    public class TutorialSequence
    {
        private readonly List<TutorialStep> _steps = new();

        public IReadOnlyList<TutorialStep> Steps => _steps;

        public int Count => _steps.Count;

        public void AddStep(TutorialStep step)
        {
            if (step == null)
                return;

            _steps.Add(step);
        }

        public void Clear()
        {
            _steps.Clear();
        }
    }
}