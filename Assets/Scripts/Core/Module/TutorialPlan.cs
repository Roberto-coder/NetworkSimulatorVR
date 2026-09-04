using System;
using System.Collections.Generic;
using UnityEngine;
using Waypoints;

namespace Core.Module
{
    [CreateAssetMenu(fileName = "TutorialPlan", menuName = "Network Simulator/Tutorial Plan")]
    public sealed class TutorialPlan : ScriptableObject
    {
        [TextArea(3, 8)] [SerializeField] private string notes;
        [SerializeField] private List<TutorialStepDefinition> steps = new();

        public string Notes => notes;
        public IReadOnlyList<TutorialStepDefinition> Steps => steps;
    }

    public enum TutorialStepKind
    {
        Dialogue,
        Wait,
        MoveNpc,
        LookAt,
        GuidedObjective,
        Custom
    }

    [Serializable]
    public sealed class TutorialStepDefinition
    {
        [SerializeField] private string label;
        [SerializeField] private TutorialStepKind kind;
        [TextArea(2, 5)] [SerializeField] private string dialogueOrInstruction;
        [SerializeField] private string voiceId;
        [SerializeField] private string objectiveId;
        [SerializeField] private Waypoint waypoint;
        [SerializeField] private float waitSeconds;
        [Tooltip("Componente que implementará un paso no cubierto por los tipos anteriores.")]
        [SerializeField] private MonoBehaviour customHandler;

        public string Label => label;
        public TutorialStepKind Kind => kind;
        public string DialogueOrInstruction => dialogueOrInstruction;
        public string VoiceId => voiceId;
        public string ObjectiveId => objectiveId;
        public Waypoint Waypoint => waypoint;
        public float WaitSeconds => waitSeconds;
        public MonoBehaviour CustomHandler => customHandler;
    }
}
