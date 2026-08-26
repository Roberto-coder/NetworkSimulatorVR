using Core.Objectives;
using GameData.Objectives;

namespace Modules.Lobby.Objectives
{
    public sealed class CompleteLobbyTutorialObjective : ObjectiveBase
    {
        public const string ObjectiveId = "lobby_tutorial";

        public CompleteLobbyTutorialObjective(ObjectiveData data) : base(data)
        {
        }
    }
}
