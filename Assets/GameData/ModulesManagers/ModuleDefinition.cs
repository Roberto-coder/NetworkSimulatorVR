using System.Collections.Generic;
using Core;
using Core.Module;
using Framework.Interaction.Tools;
using GameData.Objectives;
using GameData.Quiz;
using UnityEngine;
using Presentacion.GlobalUI.RadialSelectorTool;
using GameData.Achievements;

namespace GameData.Modules
{
    /// <summary>
    /// Contiene la informacion configurable de un modulo de entrenamiento.
    /// </summary>
    
    [CreateAssetMenu(fileName = "ModuleDefinition", menuName = "Scriptable Objects/ModuleDefinition")]
    public class ModuleDefinition : ScriptableObject
    {
        [SerializeField]
        private string moduleId;

        [SerializeField]
        private string moduleName;

        [SerializeField]
        private string description;

        [SerializeField]
        private List<ObjectiveData> objectives = new();

        [Header("Final Quiz")]
        [SerializeField]
        private QuizData finalQuiz;

        [Header("Completion Achievement")]
        [SerializeField] private AchievementDefinition completionAchievement;

        /// <summary>
        /// Obtiene el identificador unico del modulo.
        /// </summary>
        public string ModuleId => moduleId;

        /// <summary>
        /// Obtiene el nombre del modulo.
        /// </summary>
        public string ModuleName => moduleName;

        /// <summary>
        /// Obtiene la descripcion del modulo.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Obtiene la secuencia de objetivos configurada para el modulo.
        /// </summary>
        public IReadOnlyList<ObjectiveData> Objectives => objectives;

        /// <summary>
        /// Obtiene el quiz presentado al finalizar el modulo.
        /// </summary>
        public QuizData FinalQuiz => finalQuiz;
        public AchievementDefinition CompletionAchievement => completionAchievement;
        
        /// <summary>
        /// Obtiene la secuencia de objetivos configurada para el modulo.
        /// </summary>
        [Header("Available Tools")]
        public List<ToolData> availableTools;
        
        [Header("Input")]
        public ModuleInputSettings inputSettings;
        
    }
}
