using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameData.Module02
{
    /// <summary>Guion editable por objetivo; ObjectiveData aporta instrucciones de respaldo.</summary>
    [CreateAssetMenu(menuName = "Network Simulator/Module 02/Tutorial", fileName = "Module02Tutorial")]
    public sealed class Module02TutorialData : ScriptableObject
    {
        [TextArea(2, 8)] [SerializeField] private string introduction;
        [TextArea(2, 8)] [SerializeField] private string completion;
        [TextArea(2, 8)] [SerializeField] private string quizInstruction;
        [SerializeField] private List<Module02TutorialObjective> objectives = new();
        [SerializeField] private GameData.NPC.DialogueAudio introductionAudio = new();
        [SerializeField] private GameData.NPC.DialogueAudio completionAudio = new();
        [SerializeField] private GameData.NPC.DialogueAudio quizInstructionAudio = new();
        public GameData.NPC.DialogueAudio IntroductionAudio => introductionAudio;
        public GameData.NPC.DialogueAudio CompletionAudio => completionAudio;
        public GameData.NPC.DialogueAudio QuizInstructionAudio => quizInstructionAudio;
        [SerializeField] private List<GameData.NPC.NPCDialogueLine> reactions = new();
        public GameData.NPC.NPCDialogueLine FindReaction(string id) => reactions.Find(line => line != null && line.id == id);
        public string Introduction => introduction;
        public string Completion => completion;
        public string QuizInstruction => quizInstruction;
        public Module02TutorialObjective Find(string id) => objectives.Find(entry => entry != null && entry.objectiveId == id);
    }

    [Serializable]
    public sealed class Module02TutorialObjective
    {
        public string objectiveId;
        public GameData.NPC.DialogueAudio explanationAudio = new();
        public GameData.NPC.DialogueAudio instructionAudio = new();
        [TextArea(3, 8)] public string explanation;
        [TextArea(2, 6)] public string instruction;
    }
}
