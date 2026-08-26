using System;
using System.Collections.Generic;
using Core.Objectives;
using Framework.Interaction.Tools;
using GameData.Modules;
using GameData.Objectives;
using Modules.Lobby.Factories;
using Modules.Lobby.Objectives;

namespace Modules.Lobby.Flow
{
    public sealed class LobbyFlowController
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ObjectiveController objectiveController;
        private bool hasStarted;

        public LobbyFlowController(ModuleDefinition moduleDefinition)
        {
            this.moduleDefinition = moduleDefinition
                ?? throw new ArgumentNullException(nameof(moduleDefinition));

            LobbyObjectiveFactory factory = new();
            List<ObjectiveBase> objectives = new(moduleDefinition.Objectives.Count);
            foreach (ObjectiveData data in moduleDefinition.Objectives)
                objectives.Add(factory.Create(data));

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
        public ObjectiveData CurrentObjectiveData =>
            CurrentObjectiveIndex >= 0 && CurrentObjectiveIndex < moduleDefinition.Objectives.Count
                ? moduleDefinition.Objectives[CurrentObjectiveIndex]
                : null;
        public IReadOnlyList<ToolData> AvailableTools => moduleDefinition.availableTools;
        public bool IsCompleted { get; private set; }

        public void Begin()
        {
            if (hasStarted)
                return;
            hasStarted = true;
            objectiveController.Begin();
        }

        public void CompleteTutorial()
        {
            if (!hasStarted || IsCompleted ||
                CurrentObjectiveData?.Id != CompleteLobbyTutorialObjective.ObjectiveId)
                return;

            ObjectiveData completed = CurrentObjectiveData;
            CurrentObjective?.Complete();
            ObjectiveCompleted?.Invoke(completed);
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
