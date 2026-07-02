using Core.Objectives;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module01_CableMaking.Factories;
using NetworkVR.Core.Objectives;
using UnityEngine;

namespace Framework.Debug
{
    /// <summary>
    /// Inicia el primer objetivo configurado para el Modulo 1.
    /// </summary>
    public sealed class Module1Runner : MonoBehaviour
    {
        [SerializeField]
        private ModuleDefinition moduleDefinition;

        private ObjectiveBase currentObjective;

        private void Start()
        {
            if (moduleDefinition == null)
            {
                UnityEngine.Debug.LogError("No se asigno un ModuleDefinition.", this);
                return;
            }

            if (moduleDefinition.Objectives.Count == 0)
            {
                UnityEngine.Debug.LogError("El modulo no contiene objetivos.", this);
                return;
            }

            ObjectiveData objectiveData = moduleDefinition.Objectives[0];
            ObjectiveFactory objectiveFactory = new ObjectiveFactory();

            currentObjective = objectiveFactory.Create(objectiveData);
            currentObjective.Initialize();
            currentObjective.Begin();

            UnityEngine.Debug.Log($"Objetivo actual: {objectiveData.Title}", this);
        }
    }
}