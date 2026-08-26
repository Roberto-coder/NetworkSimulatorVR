using System;
using Framework.Spawning;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Flow;
using Presentacion.Quiz;
using SFX;
using UnityEngine;

namespace Modules.Module01_CableMaking.Presentation
{
    /// <summary>
    /// Sincroniza las dos representaciones visuales del cable con su dominio.
    /// Cada extremo conserva sus propios objetos visuales; el flujo del modulo
    /// no necesita saber como estan construidos en la escena.
    /// </summary>
    public sealed class Module01PresentationController : MonoBehaviour
    {
        [Serializable]
        private sealed class EndVisuals
        {
            [SerializeField] private CableEnd end;
            [SerializeField] private GameObject wholeVisual;
            [SerializeField] private GameObject peeledVisual;
            [SerializeField] private GameObject rj45Visual;
            [SerializeField] private GameObject crimpedVisual;
            [SerializeField] private DebrisSpawner debrisSpawner;

            public CableEnd End => end;

            public void Show(CableState state)
            {
                SetActive(wholeVisual, state == CableState.Whole);
                SetActive(peeledVisual, state == CableState.Peeled);
                SetActive(rj45Visual, state is CableState.Rj45Disordered or CableState.Rj45Ordered);
                SetActive(crimpedVisual, state == CableState.Rj45Crimped);
            }

            public void SpawnDebris() => debrisSpawner?.Spawn();

            public void HideAll()
            {
                SetActive(wholeVisual, false);
                SetActive(peeledVisual, false);
                SetActive(rj45Visual, false);
                SetActive(crimpedVisual, false);
            }

            private static void SetActive(GameObject target, bool value)
            {
                if (target != null)
                    target.SetActive(value);
            }
        }

        [Header("Cable")]
        [SerializeField] private CableStateController cableState;
        [SerializeField] private EndVisuals[] endVisuals;

        [Header("Shared stations")]
        [SerializeField] private ObjectSpawner wirePuzzle;
        [SerializeField] private ObjectSpawner quizSpawner;

        private CableEnd? activePuzzleEnd;
        private ModuleFlowController flowController;
        private QuizController activeQuizController;

        private void OnEnable()
        {
            if (cableState != null)
                cableState.StateChanged += HandleCableStateChanged;
        }

        private void Start()
        {
            RenderInitialState();

            if (SimulationManager.Instance == null)
                return;

            flowController = SimulationManager.Instance.FlowController;
            flowController.FinalQuizRequested += ShowQuiz;
        }

        private void OnDisable()
        {
            if (cableState != null)
                cableState.StateChanged -= HandleCableStateChanged;

            if (flowController != null)
            {
                flowController.FinalQuizRequested -= ShowQuiz;
            }

            UnsubscribeFromQuiz();
        }

        private void RenderInitialState()
        {
            if (cableState == null || endVisuals == null)
                return;

            foreach (EndVisuals visuals in endVisuals)
                visuals?.Show(cableState.GetState(visuals.End));
        }

        private void HandleCableStateChanged(CableEnd end, CableState state)
        {
            Debug.Log($"[Module01Presentation] {end} cambio a {state}.", this);

            EndVisuals visuals = FindVisuals(end);
            if (visuals == null)
            {
                Debug.LogWarning($"No hay referencias visuales configuradas para el extremo {end}.", this);
                return;
            }

            visuals.Show(state);

            if (state == CableState.Peeled)
                visuals.SpawnDebris();

            if (state == CableState.Rj45Disordered)
                SpawnPuzzle(end);
            else if (state == CableState.Rj45Ordered && activePuzzleEnd == end)
                DespawnPuzzle();
        }

        private void SpawnPuzzle(CableEnd end)
        {
            if (wirePuzzle == null)
            {
                Debug.LogWarning("No hay un ObjectSpawner configurado para el puzzle RJ45.", this);
                return;
            }

            if (activePuzzleEnd.HasValue && activePuzzleEnd != end)
            {
                Debug.LogWarning("Ya hay un puzzle RJ45 activo para el otro extremo.", this);
                return;
            }

            wirePuzzle.Spawn();
            activePuzzleEnd = end;

            Debug.Log($"[Module01Presentation] Puzzle RJ45 generado para {end}.", this);

            RJ45PuzzleController controller =
                wirePuzzle.CurrentInstance?.GetComponentInChildren<RJ45PuzzleController>(true);
            if (controller == null)
            {
                Debug.LogError(
                    "El prefab generado no contiene RJ45PuzzleController.",
                    this);
                return;
            }

            controller.Configure(end, cableState);
        }

        private void DespawnPuzzle()
        {
            wirePuzzle?.Despawn();
            activePuzzleEnd = null;
        }

        private void ShowQuiz()
        {
            UnsubscribeFromQuiz();

            if (endVisuals != null)
            {
                foreach (EndVisuals visuals in endVisuals)
                    visuals?.HideAll();
            }

            if (quizSpawner == null)
            {
                Debug.LogWarning("No hay un ObjectSpawner configurado para el quiz.", this);
                return;
            }

            quizSpawner.Spawn();

            QuizController quizController =
                quizSpawner.CurrentInstance?.GetComponentInChildren<QuizController>(true);

            if (quizController == null)
            {
                Debug.LogError("El prefab generado no contiene QuizController.", this);
                return;
            }

            activeQuizController = quizController;
            ModuleCompletionCoordinator completionCoordinator =
                activeQuizController.GetComponent<ModuleCompletionCoordinator>() ??
                activeQuizController.gameObject.AddComponent<ModuleCompletionCoordinator>();
            completionCoordinator.Configure(activeQuizController, flowController.ModuleDefinition, flowController);
            activeQuizController.Configure(flowController.ModuleDefinition.FinalQuiz);
        }

        private void UnsubscribeFromQuiz()
        {
            if (activeQuizController == null)
                return;

            activeQuizController = null;
        }

        private EndVisuals FindVisuals(CableEnd end)
        {
            if (endVisuals == null)
                return null;

            foreach (EndVisuals visuals in endVisuals)
            {
                if (visuals != null && visuals.End == end)
                    return visuals;
            }

            return null;
        }
    }
}
