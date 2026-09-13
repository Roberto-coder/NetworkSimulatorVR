using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameData.Module02
{
    /// <summary>Texto narrativo editable. Las instrucciones de objetivos se leen de ObjectiveData.</summary>
    [CreateAssetMenu(menuName = "Network Simulator/Module 02/Tutorial", fileName = "Module02Tutorial")]
    public sealed class Module02TutorialData : ScriptableObject
    {
        [TextArea(2, 8)] [SerializeField] private string introduction;
        [TextArea(2, 8)] [SerializeField] private string completion;
        [SerializeField] private List<Module02TutorialObjective> objectives = new();
        public string Introduction => introduction;
        public string Completion => completion;
        public Module02TutorialObjective Find(string id) => objectives.Find(entry => entry != null && entry.objectiveId == id);
    }

    [Serializable]
    public sealed class Module02TutorialObjective
    {
        public string objectiveId;
        [TextArea(3, 8)] public string explanation;
        [TextArea(2, 6)] public string instruction;
    }
}
