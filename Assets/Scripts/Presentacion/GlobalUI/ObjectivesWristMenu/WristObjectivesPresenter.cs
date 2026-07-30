using System.Collections.Generic;
using Core.Objectives;
using GameData.Objectives;
using Modules.Module01_CableMaking;
using Modules.Module01_CableMaking.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
    /// <summary>
    /// Su responsabilidad sería únicamente:
    /// - construir la lista una vez
    /// - escuchar eventos
    /// - actualizar la apariencia
    /// Nada de lógica de objetivos.
    /// </summary>
    public class WristObjectivesPresenter : MonoBehaviour {
        
    [SerializeField] private Transform content;
    [SerializeField] private ObjectiveItemUI prefab;
    
    private readonly List<ObjectiveItemUI> items = new();

    private ModuleFlowController flow;

    private void Start()
    {
        if (SimulationManager.Instance == null)
        {
            Debug.LogError("No existe SimulationManager en la escena.");
            return;
        }

        flow = SimulationManager.Instance.FlowController;

        flow.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
        flow.ModuleCompleted += HandleModuleCompleted;

        CreateItems();
    }

    private void OnDestroy()
    {
        if (flow != null)
        {
            flow.CurrentObjectiveChanged -= HandleCurrentObjectiveChanged;
            flow.ModuleCompleted -= HandleModuleCompleted;
        }
    }

    private void CreateItems()
    {
        Debug.Log("Numero de objetivos:"+flow.ModuleDefinition.Objectives.Count);
        foreach (Transform child in content)
            Destroy(child.gameObject);

        items.Clear();

        foreach (var objective in flow.ModuleDefinition.Objectives)
        {
            items.Add(Instantiate(prefab, content));
        }

        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        int current = flow.CurrentObjectiveIndex;

        for (int i = 0; i < items.Count; i++)
        {
            ObjectiveVisualState state;

            if (i < current)
            {
                state = ObjectiveVisualState.Completed;
            }
            else if (i == current)
            {
                state = ObjectiveVisualState.Current;
            }
            else
            {
                state = ObjectiveVisualState.Pending;
            }

            items[i].Setup(
                flow.ModuleDefinition.Objectives[i].Title,
                state);
        }
    }

    private void HandleCurrentObjectiveChanged(ObjectiveData _)
    {
        RefreshVisualState();
    }

    private void HandleModuleCompleted()
    {
        RefreshVisualState();
    }
}
}