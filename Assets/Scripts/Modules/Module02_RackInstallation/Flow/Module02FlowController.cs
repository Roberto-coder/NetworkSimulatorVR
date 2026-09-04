using System;
using System.Collections.Generic;
using Core.Objectives;
using Framework.Interaction.Tools;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Objectives;

namespace Modules.Module02_RackInstallation.Flow
{
    /// <summary>
    /// Flujo secuencial inicial del módulo. TryCompleteCurrent es el punto de
    /// conexión para tarjetas, montaje, tornillos, cableado y configuración.
    /// </summary>
    public sealed class Module02FlowController
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ObjectiveController objectiveController;
        private bool hasStarted;

        public Module02FlowController(ModuleDefinition definition)
        {
            moduleDefinition = definition ?? throw new ArgumentNullException(nameof(definition));
            List<ObjectiveBase> objectives = new(definition.Objectives.Count);
            foreach (ObjectiveData data in definition.Objectives)
            {
                if (data == null)
                    throw new ArgumentException("El módulo 2 contiene un objetivo nulo.", nameof(definition));
                objectives.Add(new Module02Objective(data));
            }

            objectiveController = new ObjectiveController(objectives);
            objectiveController.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
            objectiveController.AllObjectivesCompleted += HandleAllObjectivesCompleted;
        }

        public event Action<ObjectiveData> CurrentObjectiveChanged;
        public event Action<ObjectiveData> ObjectiveCompleted;
        public event Action ModuleCompleted;

        public ModuleDefinition ModuleDefinition => moduleDefinition;
        public IReadOnlyList<ObjectiveBase> Objectives => objectiveController.Objectives;
        public ObjectiveBase CurrentObjective => objectiveController.CurrentObjective;
        public int CurrentObjectiveIndex => objectiveController.CurrentObjectiveIndex;
        public ObjectiveData CurrentObjectiveData => CurrentObjectiveIndex >= 0 &&
            CurrentObjectiveIndex < moduleDefinition.Objectives.Count
                ? moduleDefinition.Objectives[CurrentObjectiveIndex]
                : null;
        public IReadOnlyList<ToolData> AvailableTools => moduleDefinition.availableTools;
        public bool IsCompleted { get; private set; }

        public void Begin()
        {
            if (hasStarted)
                return;
            hasStarted = true;
            if (moduleDefinition.Objectives.Count == 0)
                HandleAllObjectivesCompleted();
            else
                objectiveController.Begin();
        }

        public bool TryCompleteCurrent(string objectiveId)
        {
            if (!hasStarted || IsCompleted || string.IsNullOrWhiteSpace(objectiveId) ||
                CurrentObjectiveData?.Id != objectiveId)
                return false;

            ObjectiveData completed = CurrentObjectiveData;
            ObjectiveBase objective = CurrentObjective;
            objective?.Complete();
            if (objective == null || !objective.IsCompleted)
                return false;

            ObjectiveCompleted?.Invoke(completed);
            return true;
        }

        private void HandleCurrentObjectiveChanged(ObjectiveBase objective) =>
            CurrentObjectiveChanged?.Invoke(objective?.Data);

        private void HandleAllObjectivesCompleted()
        {
            if (IsCompleted)
                return;
            IsCompleted = true;
            ModuleCompleted?.Invoke();
        }
    }
}
