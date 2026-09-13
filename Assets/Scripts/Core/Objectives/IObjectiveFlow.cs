using System;
using System.Collections.Generic;
using GameData.Objectives;

namespace Core.Objectives
{
    /// <summary>
    /// Lectura común del progreso para tutoriales de cualquier módulo.
    /// No permite completar objetivos ni conoce las mecánicas de cada práctica.
    /// </summary>
    public interface IObjectiveFlow
    {
        IReadOnlyList<ObjectiveBase> Objectives { get; }
        ObjectiveData CurrentObjectiveData { get; }
        event Action<ObjectiveData> ObjectiveCompleted;
        event Action<ObjectiveData> CurrentObjectiveChanged;
    }
}
