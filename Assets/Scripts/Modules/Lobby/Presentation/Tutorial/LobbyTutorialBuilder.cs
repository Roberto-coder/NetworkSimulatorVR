using GameData.NPC;
using Presentacion.NPC;
using Presentacion.Tutorial;
using Waypoints;

namespace Modules.Lobby.Presentation.Tutorial
{
    public sealed class LobbyTutorialBuilder
    {
        public TutorialSequence Build(
            NPCDialogueData data,
            Waypoint mainPanel, 
            Waypoint museumArea, 
            Waypoint final)
        {
            TutorialSequence sequence = new();
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep(data.Get("lobby_intro_controls")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_intro_role")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_tools")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_objectives")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_follow_me")));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(mainPanel));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep(data.Get("lobby_storage")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_profile")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_modules")));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(museumArea));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep(data.Get("lobby_museum_devices")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_museum_controls")));
            sequence.AddStep(new DialogueStep(data.Get("lobby_farewell")));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(final));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            return sequence;
        }
    }
}
