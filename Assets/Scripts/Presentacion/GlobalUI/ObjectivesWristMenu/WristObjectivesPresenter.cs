using System.Collections.Generic;
using Core.Objectives;
using GameData.Objectives;
using Modules.Module01_CableMaking;
using Modules.Module01_CableMaking.Flow;
using Modules.Lobby;
using Modules.Lobby.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.ObjectivesWristMenu
{
    /// <summary>Builds the ordered objective list and mirrors its runtime state.</summary>
    public sealed class WristObjectivesPresenter : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private ObjectiveItemUI prefab;

        [Header("Compact wrist layout")]
        [SerializeField] private float rowHeight = 20f;
        [SerializeField] private float rowSpacing = 3f;

        private readonly List<ObjectiveItemUI> items = new List<ObjectiveItemUI>();
        private ModuleFlowController flow;
        private LobbyFlowController lobbyFlow;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Explicit initialization is needed because the wrist controller hides
        /// this GameObject before Unity can call Start on it.
        /// </summary>
        public void Initialize()
        {
            ResolveRuntimeContent();

            ModuleFlowController currentFlow = SimulationManager.Instance != null
                ? SimulationManager.Instance.FlowController
                : null;
            LobbyFlowController currentLobby = currentFlow == null
                ? LobbyManager.Instance?.FlowController
                : null;

            if ((currentFlow == null && currentLobby == null) || content == null || prefab == null)
                return;

            if (flow != currentFlow || lobbyFlow != currentLobby)
            {
                Unsubscribe();
                flow = currentFlow;
                lobbyFlow = currentLobby;
                if (flow != null)
                {
                    flow.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
                    flow.ModuleCompleted += HandleModuleCompleted;
                }
                if (lobbyFlow != null)
                {
                    lobbyFlow.CurrentObjectiveChanged += HandleCurrentObjectiveChanged;
                    lobbyFlow.ModuleCompleted += HandleModuleCompleted;
                }
                CreateItems();
            }
            else if (items.Count != ActiveObjectives.Count)
            {
                CreateItems();
            }

            RefreshVisualState();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void CreateItems()
        {
            ConfigureContentArea();

            foreach (Transform child in content)
                Destroy(child.gameObject);

            items.Clear();
            IReadOnlyList<ObjectiveBase> objectives = ActiveObjectives;
            for (int i = 0; i < objectives.Count; i++)
            {
                ObjectiveItemUI item = Instantiate(prefab, content);
                RectTransform itemRect = (RectTransform)item.transform;
                itemRect.anchorMin = new Vector2(0f, 1f);
                itemRect.anchorMax = new Vector2(1f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = new Vector2(0f, -i * (rowHeight + rowSpacing));
                itemRect.sizeDelta = new Vector2(0f, rowHeight);
                items.Add(item);
            }

            RectTransform contentRect = (RectTransform)content;
            float height = objectives.Count == 0
                ? 0f
                : objectives.Count * rowHeight + (objectives.Count - 1) * rowSpacing;
            contentRect.sizeDelta = new Vector2(0f, height);
        }

        private void ConfigureContentArea()
        {
            RectTransform panel = transform.Find("ObjectivesPanel") as RectTransform;
            if (panel == null)
                return;

            // The old ScrollView had a zero-height viewport in the nested player
            // prefab. The objective list is small, so a fixed panel area is more
            // predictable and readable on the wrist.
            if (content.parent != panel)
                content.SetParent(panel, false);

            RectTransform contentRect = (RectTransform)content;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = new Vector2(0f, -50f);
            contentRect.sizeDelta = new Vector2(-24f, 0f);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                layout.enabled = false;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null)
                fitter.enabled = false;
        }

        private void ResolveRuntimeContent()
        {
            if (content != null && content.gameObject.scene == gameObject.scene)
                return;

            RectTransform[] descendants = GetComponentsInChildren<RectTransform>(true);
            foreach (RectTransform candidate in descendants)
            {
                if (candidate != null && candidate.name == "Content" &&
                    candidate.gameObject.scene == gameObject.scene)
                {
                    content = candidate;
                    break;
                }
            }

            if (content == null)
                Debug.LogError(
                    "WristObjectivesPresenter no encontró un Content dentro de su propia instancia.",
                    this);
        }

        private void RefreshVisualState()
        {
            IReadOnlyList<ObjectiveBase> objectives = ActiveObjectives;
            if (objectives.Count == 0)
                return;

            int count = Mathf.Min(items.Count, objectives.Count);
            for (int i = 0; i < count; i++)
            {
                ObjectiveBase objective = objectives[i];
                ObjectiveVisualState visualState = ToVisualState(objective.State);
                items[i].Setup(objective.Data.Title, visualState);
            }
        }

        private static ObjectiveVisualState ToVisualState(ObjectiveState state)
        {
            switch (state)
            {
                case ObjectiveState.Completed:
                    return ObjectiveVisualState.Completed;
                case ObjectiveState.Running:
                    return ObjectiveVisualState.Current;
                default:
                    return ObjectiveVisualState.Pending;
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

        private IReadOnlyList<ObjectiveBase> ActiveObjectives => flow != null
            ? flow.Objectives
            : lobbyFlow != null ? lobbyFlow.Objectives : System.Array.Empty<ObjectiveBase>();

        private void Unsubscribe()
        {
            if (flow != null)
            {
                flow.CurrentObjectiveChanged -= HandleCurrentObjectiveChanged;
                flow.ModuleCompleted -= HandleModuleCompleted;
            }
            if (lobbyFlow != null)
            {
                lobbyFlow.CurrentObjectiveChanged -= HandleCurrentObjectiveChanged;
                lobbyFlow.ModuleCompleted -= HandleModuleCompleted;
            }
            flow = null;
            lobbyFlow = null;
        }
    }
}
