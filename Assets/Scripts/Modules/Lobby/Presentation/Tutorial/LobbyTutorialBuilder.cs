using Presentacion.NPC;
using Presentacion.Tutorial;
using Waypoints;

namespace Modules.Lobby.Presentation.Tutorial
{
    public sealed class LobbyTutorialBuilder
    {
        public TutorialSequence Build(
            Waypoint mainPanel, 
            Waypoint museumArea, 
            Waypoint final)
        {
            TutorialSequence sequence = new();
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep("Hola, soy tu asistente. Presiona el botón B para avanzar el diálogo.", voiceId: "lobby_intro_controls"));
            sequence.AddStep(new DialogueStep("Como ingeniero de los laboratorios de red, te acompañaré en tu aprendizaje, comenzando por la capa física del modelo OSI.", voiceId: "lobby_intro_role"));
            sequence.AddStep(new DialogueStep("Presiona Y para abrir la rueda, apunta a la herramienta que quieras y usa R2 para utilizarla.", voiceId: "lobby_tools"));
            sequence.AddStep(new DialogueStep("Al girar la muñeca izquierda verás el menú de objetivos de cada módulo. En el Lobby tu único objetivo es completar este tutorial.", voiceId: "lobby_objectives"));
            sequence.AddStep(new DialogueStep("Sígueme para comenzar el recorrido.", voiceId: "lobby_follow_me"));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(mainPanel));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep("En este panel puedes gestionar tu almacenamiento. Guarda tu progreso cada vez que completes un módulo.", voiceId: "lobby_storage"));
            sequence.AddStep(new DialogueStep("También puedes consultar tu perfil, insignias y módulos completados.", voiceId: "lobby_profile"));
            sequence.AddStep(new DialogueStep("Desde aquí podrás desbloquear módulos nuevos o repetir los que ya completaste.", voiceId: "lobby_modules"));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(museumArea));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            sequence.AddStep(new DialogueStep("En el museo, los dispositivos con contorno amarillo pueden manipularse para conocer su información y características.", voiceId: "lobby_museum_devices"));
            sequence.AddStep(new DialogueStep("Puedes interactuar con ellos usando el trigger de los controles o mediante gestos de hand tracking.", voiceId: "lobby_museum_controls"));
            sequence.AddStep(new DialogueStep("Buena suerte. Volveré a acompañarte más adelante en otros módulos.", voiceId: "lobby_farewell"));
            sequence.AddStep(new LookAtStep(NPCLookMode.MovementDirection));
            sequence.AddStep(new MoveNpcStep(final));
            sequence.AddStep(new LookAtStep(NPCLookMode.Player));
            return sequence;
        }
    }
}
