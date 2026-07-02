using System;
using GameData.Objectives;
using NetworkVR.Core.Objectives;

namespace Core.Objectives
{
    /// <summary>
    /// Define el ciclo de vida reutilizable que comparten todos los objetivos del simulador.
    /// </summary>
    public abstract class ObjectiveBase
    {
        protected ObjectiveBase(ObjectiveData data)
        {
            UnityEngine.Debug.Log("Objetivo creado");
        }

        /// <summary>
        /// Se produce cuando el objetivo comienza a ejecutarse.
        /// </summary>
        public event Action<ObjectiveBase> Started;

        /// <summary>
        /// Se produce cuando el objetivo finaliza correctamente.
        /// </summary>
        public event Action<ObjectiveBase> Completed;

        /// <summary>
        /// Se produce cuando el objetivo se cancela antes de completarse.
        /// </summary>
        public event Action<ObjectiveBase> Cancelled;

        /// <summary>
        /// Obtiene el estado actual del ciclo de vida del objetivo.
        /// </summary>
        public ObjectiveState State { get; private set; } = ObjectiveState.Waiting;

        /// <summary>
        /// Obtiene un valor que indica si el objetivo finalizo correctamente.
        /// </summary>
        public bool IsCompleted => State == ObjectiveState.Completed;

        /// <summary>
        /// Obtiene un valor que indica si el objetivo esta en ejecucion.
        /// </summary>
        public bool IsRunning => State == ObjectiveState.Running;

        /// <summary>
        /// Prepara el objetivo antes de que comience.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Inicia el objetivo si esta en espera o cancelado.
        /// </summary>
        public virtual void Begin()
        {
            if (State == ObjectiveState.Running || State == ObjectiveState.Completed)
            {
                return;
            }

            State = ObjectiveState.Running;
            Started?.Invoke(this);
        }

        /// <summary>
        /// Marca el objetivo como completado si esta en ejecucion.
        /// </summary>
        public virtual void Complete()
        {
            if (State != ObjectiveState.Running)
            {
                return;
            }

            State = ObjectiveState.Completed;
            Completed?.Invoke(this);
        }

        /// <summary>
        /// Cancela el objetivo si aun no ha sido completado.
        /// </summary>
        public virtual void Cancel()
        {
            if (State == ObjectiveState.Completed || State == ObjectiveState.Cancelled)
            {
                return;
            }

            State = ObjectiveState.Cancelled;
            Cancelled?.Invoke(this);
        }

        /// <summary>
        /// Devuelve el objetivo a su estado inicial de espera.
        /// </summary>
        public virtual void Reset()
        {
            State = ObjectiveState.Waiting;
        }
    }
}
