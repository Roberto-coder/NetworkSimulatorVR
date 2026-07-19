using System.Collections;
using UnityEngine;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Paso del tutorial que simplemente espera una cantidad
    /// determinada de segundos.
    /// </summary>
    public class WaitSecondsStep : TutorialStep
    {
        private readonly float _seconds;

        public WaitSecondsStep(float seconds)
        {
            _seconds = Mathf.Max(0f, seconds);
        }

        public override IEnumerator Execute(TutorialDirector director)
        {
            yield return new WaitForSeconds(_seconds);
        }
    }
}