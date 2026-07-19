using System.Collections;

namespace Presentacion.Tutorial
{
    /// <summary>
    /// Clase base para cualquier paso ejecutable del tutorial.
    /// Cada paso implementa su propia lógica y finaliza cuando
    /// termina la corrutina Execute().
    /// </summary>
    public abstract class TutorialStep
    {
        /// <summary>
        /// Ejecuta el paso del tutorial.
        /// </summary>
        public abstract IEnumerator Execute(TutorialDirector director);
    }
}