using System;
using System.Collections.Generic;
using Core.Objectives;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module01_CableMaking.Factories;
using Presentacion.GlobalUI.RadialSelectorTool;

namespace Modules.Module01_CableMaking.Flow
{
    /// <summary>
    /// Coordina la ejecucion de los objetivos configurados para un modulo.
    /// </summary>
    public sealed class ModuleFlowController
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ObjectiveController objectiveController;
        private bool hasStarted;

        /// <summary>
        /// Se produce cuando cambia el objetivo activo.
        /// </summary>
        public event Action<ObjectiveData> CurrentObjectiveChanged;

        /// <summary>
        /// Se produce cuando todos los objetivos del modulo han finalizado.
        /// </summary>
        public event Action ModuleCompleted;

        /// <summary>
        /// Inicializa el flujo y crea los objetivos definidos para el modulo.
        /// </summary>
        /// <param name="moduleDefinition">Definicion del modulo que se ejecutara.</param>
        public ModuleFlowController(ModuleDefinition moduleDefinition)
        {
            this.moduleDefinition = moduleDefinition
                ?? throw new ArgumentNullException(nameof(moduleDefinition));

            ObjectiveFactory objectiveFactory = new ObjectiveFactory();
            List<ObjectiveBase> objectives = new List<ObjectiveBase>(
                moduleDefinition.Objectives.Count);

            foreach (ObjectiveData objectiveData in moduleDefinition.Objectives)
            {
                if (objectiveData == null)
                {
                    throw new ArgumentException(
                        "La definicion del modulo contiene un objetivo nulo.",
                        nameof(moduleDefinition));
                }

                objectives.Add(objectiveFactory.Create(objectiveData));
            }

            objectiveController = new ObjectiveController(objectives);
            objectiveController.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
            objectiveController.AllObjectivesCompleted += HandleAllObjectivesCompleted;
        }

        /// <summary>
        /// Obtiene la definicion del modulo en ejecucion.
        /// </summary>
        public ModuleDefinition ModuleDefinition => moduleDefinition;

        /// <summary>
        /// Obtiene el objetivo activo actual.
        /// </summary>
        public ObjectiveBase CurrentObjective => objectiveController.CurrentObjective;

        /// <summary>
        /// Obtiene los datos del objetivo activo actual.
        /// </summary>
        public ObjectiveData CurrentObjectiveData
        {
            get
            {
                int index = objectiveController.CurrentObjectiveIndex;

                if (index < 0 || index >= moduleDefinition.Objectives.Count)
                {
                    return null;
                }

                return moduleDefinition.Objectives[index];
            }
        }

        /// <summary>
        /// Obtiene un valor que indica si el modulo ha finalizado.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Inicia el primer objetivo del modulo.
        /// </summary>
        public void Begin()
        {
            if (hasStarted)
            {
                return;
            }

            hasStarted = true;

            if (moduleDefinition.Objectives.Count == 0)
            {
                HandleAllObjectivesCompleted();
                return;
            }

            objectiveController.Begin();
        }

        private void HandleCurrentObjectiveChanged(ObjectiveBase objective)
        {
            CurrentObjectiveChanged?.Invoke(CurrentObjectiveData);
        }

        private void HandleAllObjectivesCompleted()
        {
            IsCompleted = true;
            ModuleCompleted?.Invoke();
        }
        
        public IReadOnlyList<ToolData> AvailableTools
        {
            get
            {
                return moduleDefinition.availableTools;
            }
        }
    }
}
