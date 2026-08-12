using System;
using System.Collections.Generic;
using Core.Objectives;
using Framework.Interaction.Tools;
using Framework.Interaction.Tools.Interfaces;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Factories;

namespace Modules.Module01_CableMaking.Flow
{
    /// <summary>
    /// Coordina la ejecucion de los objetivos configurados para un modulo.
    ///     - Inicia objetivos
    ///     - Escucha eventos importantes del módulo
    ///     - Completa objetivos
    ///     - Decide cuándo mostrar el quiz
    ///     - Decide cuándo termina el módulo
    /// </summary>
    public sealed class ModuleFlowController
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ObjectiveController objectiveController;
        private bool hasStarted;
        private TesterDockerController testerDocker;

        /// <summary>
        /// Se produce cuando cambia el objetivo activo.
        /// </summary>
        public event Action<ObjectiveData> CurrentObjectiveChanged;

        /// <summary>
        /// Se produce cuando todos los objetivos del modulo han finalizado.
        /// </summary>
        public event Action ModuleCompleted;
        
        public event Action FinalQuizRequested;

        /// <summary>
        /// Inicializa el flujo y crea los objetivos definidos para el modulo.
        /// </summary>
        /// <param name="moduleDefinition">Definicion del modulo que se ejecutara.</param>
        public ModuleFlowController(ModuleDefinition moduleDefinition)
        {
            this.moduleDefinition = moduleDefinition
                ?? throw new ArgumentNullException(nameof(moduleDefinition));

            ObjectiveFactory objectiveFactory = new ObjectiveFactory();
            List<ObjectiveBase> objectives = new List<ObjectiveBase>(
                moduleDefinition.Objectives.Count);

            foreach (ObjectiveData objectiveData in moduleDefinition.Objectives)
            {
                if (objectiveData == null)
                {
                    throw new ArgumentException(
                        "La definicion del modulo contiene un objetivo nulo.",
                        nameof(moduleDefinition));
                }

                objectives.Add(objectiveFactory.Create(objectiveData));
            }

            objectiveController = new ObjectiveController(objectives);
            objectiveController.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
            objectiveController.AllObjectivesCompleted += HandleAllObjectivesCompleted;

            CableEvents.CablePeeled += HandleCablePeeled;
            CableEvents.CableOrdered += HandleCableOrdered;
            CableEvents.CableCrimped += HandleCableCrimped;
            CableEvents.CableValidated += HandleCableValidated;
        }

        /// <summary>
        /// Obtiene la definicion del modulo en ejecucion.
        /// </summary>
        public ModuleDefinition ModuleDefinition => moduleDefinition;

        /// <summary>
        /// Obtiene el objetivo activo actual.
        /// </summary>
        public ObjectiveBase CurrentObjective => objectiveController.CurrentObjective;
        
        public int CurrentObjectiveIndex => objectiveController.CurrentObjectiveIndex;

        /// <summary>
        /// Obtiene los datos del objetivo activo actual.
        /// </summary>
        public ObjectiveData CurrentObjectiveData
        {
            get
            {
                int index = objectiveController.CurrentObjectiveIndex;

                if (index < 0 || index >= moduleDefinition.Objectives.Count)
                {
                    return null;
                }

                return moduleDefinition.Objectives[index];
            }
        }

        /// <summary>
        /// Obtiene un valor que indica si el modulo ha finalizado.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Inicia el primer objetivo del modulo.
        /// </summary>
        public void Begin()
        {
            if (hasStarted)
            {
                return;
            }

            hasStarted = true;

            if (moduleDefinition.Objectives.Count == 0)
            {
                HandleAllObjectivesCompleted();
                return;
            }

            objectiveController.Begin();
        }

        public void RegisterTesterDocker(TesterDockerController docker)
        {
            if (testerDocker == docker)
                return;

            UnregisterTesterDocker(testerDocker);
            testerDocker = docker;

            if (testerDocker != null)
                testerDocker.BothEndsConnected += HandleBothEndsConnected;

            CompleteConnectTesterIfReady();
        }

        public void UnregisterTesterDocker(TesterDockerController docker)
        {
            if (docker == null || testerDocker != docker)
                return;

            testerDocker.BothEndsConnected -= HandleBothEndsConnected;
            testerDocker = null;
        }

        private void HandleCurrentObjectiveChanged(ObjectiveBase objective)
        {
            ObjectiveData current = CurrentObjectiveData;
            UnityEngine.Debug.Log(
                current == null
                    ? "[ModuleFlow] No hay objetivo activo."
                    : $"[ModuleFlow] Objetivo activo {CurrentObjectiveIndex + 1}/" +
                      $"{moduleDefinition.Objectives.Count}: {current.Id} ({current.Title}).");

            CurrentObjectiveChanged?.Invoke(CurrentObjectiveData);
            CompleteConnectTesterIfReady();
        }

        private void HandleAllObjectivesCompleted()
        {
            IsCompleted = true;
            ModuleCompleted?.Invoke();
            FinalQuizRequested?.Invoke();
        }

        private void HandleCablePeeled(CableEnd end) =>
            CompleteEndObjective(end, "strip_left_end", "strip_right_end");

        private void HandleCableOrdered(CableEnd end) =>
            CompleteEndObjective(end, "order_left_t568b", "order_right_t568b");

        private void HandleCableCrimped(CableEnd end) =>
            CompleteEndObjective(end, "crimp_left_end", "crimp_right_end");

        private void HandleCableValidated() =>
            CompleteCurrentObjective("validate_cable");

        private void HandleBothEndsConnected() =>
            CompleteCurrentObjective("connect_tester");

        private void CompleteConnectTesterIfReady()
        {
            if (testerDocker != null && testerDocker.AreBothConnected)
                HandleBothEndsConnected();
        }

        private void CompleteEndObjective(
            CableEnd end,
            string leftObjectiveId,
            string rightObjectiveId)
        {
            CompleteCurrentObjective(
                end == CableEnd.Left ? leftObjectiveId : rightObjectiveId);
        }

        private void CompleteCurrentObjective(string expectedObjectiveId)
        {
            if (!hasStarted || IsCompleted || CurrentObjectiveData?.Id != expectedObjectiveId)
                return;

            UnityEngine.Debug.Log(
                $"[ModuleFlow] Objetivo completado: {expectedObjectiveId}.");
            CurrentObjective?.Complete();
        }
        
        public IReadOnlyList<ToolData> AvailableTools
        {
            get
            {
                return moduleDefinition.availableTools;
            }
        }
    }
}
