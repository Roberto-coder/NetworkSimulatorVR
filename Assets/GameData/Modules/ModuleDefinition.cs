using System.Collections.Generic;
using GameData.Objectives;
using UnityEngine;
using NetworkVR.Core.Objectives;

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
    }
}
