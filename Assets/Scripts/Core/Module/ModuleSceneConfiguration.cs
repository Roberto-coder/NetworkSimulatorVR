using System.Collections.Generic;
using GameData.Modules;
using GameData.Objectives;
using Presentacion.NPC;
using Presentacion.Tutorial;
using UnityEngine;
using Waypoints;

namespace Core.Module
{
    /// <summary>
    /// Punto de composición común para una escena de módulo. No ejecuta lógica:
    /// documenta y concentra las referencias que cada módulo debe cablear.
    /// </summary>
    public sealed class ModuleSceneConfiguration : MonoBehaviour
    {
        [Header("Definición y contenido")]
        [SerializeField] private ModuleDefinition moduleDefinition;
        [SerializeField] private TutorialPlan tutorialPlan;
        [SerializeField] private List<ObjectiveData> objectiveOverrides = new();
        [SerializeField] private List<ScriptableObject> additionalData = new();

        [Header("Flujo del módulo")]
        [Tooltip("Manager que crea e inicia el flujo de objetivos.")]
        [SerializeField] private MonoBehaviour moduleManager;
        [Tooltip("Controlador que conecta el flujo con UI, tutorial y finalización.")]
        [SerializeField] private MonoBehaviour presentationController;
        [Tooltip("Componentes propios de la mecánica que notifican avances.")]
        [SerializeField] private List<MonoBehaviour> gameplayControllers = new();

        [Header("Tutorial y NPC")]
        [SerializeField] private bool tutorialEnabled = true;
        [SerializeField] private TutorialDirector tutorialDirector;
        [SerializeField] private NPCMovementController npcMovement;
        [SerializeField] private NPCDialogueController npcDialogue;
        [SerializeField] private NPCReactionController npcReactions;
        [SerializeField] private List<Waypoint> tutorialWaypoints = new();

        [Header("Presentación y servicios opcionales")]
        [SerializeField] private List<MonoBehaviour> userInterfaces = new();
        [SerializeField] private List<MonoBehaviour> sceneServices = new();

        public ModuleDefinition ModuleDefinition => moduleDefinition;
        public TutorialPlan TutorialPlan => tutorialPlan;
        public IReadOnlyList<ObjectiveData> ObjectiveOverrides => objectiveOverrides;
        public bool TutorialEnabled => tutorialEnabled;
        public TutorialDirector TutorialDirector => tutorialDirector;
    }

}
