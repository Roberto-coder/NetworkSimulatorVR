using System;
using Core.Objectives;
using GameData.Objectives;
using Modules.Lobby.Objectives;

namespace Modules.Lobby.Factories
{
    public sealed class LobbyObjectiveFactory
    {
        public ObjectiveBase Create(ObjectiveData objectiveData)
        {
            if (objectiveData == null)
                throw new ArgumentNullException(nameof(objectiveData));

            return objectiveData.Id switch
            {
                CompleteLobbyTutorialObjective.ObjectiveId =>
                    new CompleteLobbyTutorialObjective(objectiveData),
                _ => throw new ArgumentException(
                    $"No existe un objetivo del Lobby asociado al identificador '{objectiveData.Id}'.",
                    nameof(objectiveData))
            };
        }
    }
}
