using System;
using System.Collections.Generic;
using NetworkVR.Core.Objectives;

namespace Core.Objectives
{
    /// <summary>
    /// Coordina una secuencia de objetivos sin conocer su implementacion concreta.
    /// </summary>
    public class ObjectiveController 
    {
        private readonly List<ObjectiveBase> objectives = new List<ObjectiveBase>();
        private int currentObjectiveIndex = -1;

        /// <summary>
        /// Se produce cuando cambia el objetivo activo.
        /// </summary>
        public event Action<ObjectiveBase> CurrentObjectiveChanged;

        /// <summary>
        /// Se produce despues de completar el ultimo objetivo de la secuencia.
        /// </summary>
        public event Action AllObjectivesCompleted;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObjectiveController"/>.
        /// </summary>
        public ObjectiveController()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObjectiveController"/> con una secuencia de objetivos.
        /// </summary>
        /// <param name="objectives">Los objetivos que se van a administrar.</param>
        public ObjectiveController(IEnumerable<ObjectiveBase> objectives)
        {
            SetObjectives(objectives);
        }

        /// <summary>
        /// Obtiene los objetivos registrados.
        /// </summary>
        public IReadOnlyList<ObjectiveBase> Objectives => objectives;

        /// <summary>
        /// Obtiene el objetivo activo actual, o null cuando no hay un objetivo activo.
        /// </summary>
        public ObjectiveBase CurrentObjective
        {
            get
            {
                if (currentObjectiveIndex < 0 || currentObjectiveIndex >= objectives.Count)
                {
                    return null;
                }

                return objectives[currentObjectiveIndex];
            }
        }

        /// <summary>
        /// Obtiene el indice de base cero del objetivo actual.
        /// </summary>
        public int CurrentObjectiveIndex => currentObjectiveIndex;

        /// <summary>
        /// Obtiene un valor que indica si todos los objetivos de la secuencia fueron completados.
        /// </summary>
        public bool HasCompletedAllObjectives =>
            objectives.Count > 0 && currentObjectiveIndex >= objectives.Count;

        /// <summary>
        /// Reemplaza la secuencia actual de objetivos.
        /// </summary>
        /// <param name="newObjectives">La nueva secuencia de objetivos.</param>
        public void SetObjectives(IEnumerable<ObjectiveBase> newObjectives)
        {
            if (newObjectives == null)
            {
                throw new ArgumentNullException(nameof(newObjectives));
            }

            UnsubscribeFromCurrentObjective();
            objectives.Clear();

            foreach (ObjectiveBase objective in newObjectives)
            {
                AddObjective(objective);
            }

            currentObjectiveIndex = -1;
        }

        /// <summary>
        /// Agrega un objetivo al final de la secuencia.
        /// </summary>
        /// <param name="objective">El objetivo que se va a agregar.</param>
        public void AddObjective(ObjectiveBase objective)
        {
            if (objective == null)
            {
                throw new ArgumentNullException(nameof(objective));
            }

            objectives.Add(objective);
        }

        /// <summary>
        /// Inicializa e inicia el primer objetivo de la secuencia.
        /// </summary>
        public void Begin()
        {
            if (objectives.Count == 0)
            {
                return;
            }

            MoveToObjective(0);
        }

        /// <summary>
        /// Cancela el objetivo actual sin avanzar la secuencia.
        /// </summary>
        public void CancelCurrentObjective()
        {
            CurrentObjective?.Cancel();
        }

        /// <summary>
        /// Reinicia todos los objetivos y devuelve el controlador a su posicion inicial.
        /// </summary>
        public void Reset()
        {
            UnsubscribeFromCurrentObjective();

            foreach (ObjectiveBase objective in objectives)
            {
                objective.Reset();
            }

            currentObjectiveIndex = -1;
            CurrentObjectiveChanged?.Invoke(null);
        }

        private void MoveToObjective(int objectiveIndex)
        {
            UnsubscribeFromCurrentObjective();

            if (objectiveIndex >= objectives.Count)
            {
                currentObjectiveIndex = objectives.Count;
                CurrentObjectiveChanged?.Invoke(null);
                AllObjectivesCompleted?.Invoke();
                return;
            }

            currentObjectiveIndex = objectiveIndex;
            ObjectiveBase objective = CurrentObjective;

            objective.Completed += HandleCurrentObjectiveCompleted;
            objective.Initialize();

            CurrentObjectiveChanged?.Invoke(objective);
            objective.Begin();
        }

        private void HandleCurrentObjectiveCompleted(ObjectiveBase objective)
        {
            if (objective != CurrentObjective)
            {
                return;
            }

            MoveToObjective(currentObjectiveIndex + 1);
        }

        private void UnsubscribeFromCurrentObjective()
        {
            ObjectiveBase objective = CurrentObjective;

            if (objective == null)
            {
                return;
            }

            objective.Completed -= HandleCurrentObjectiveCompleted;
        }
    }
}
