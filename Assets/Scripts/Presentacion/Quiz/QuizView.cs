using System;
using System.Collections.Generic;
using Core.Quiz.Domain;
using GameData.Quiz;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.Quiz
{
    /// <summary>
    /// Presenta el quiz y traduce las interacciones de UI a eventos.
    /// No contiene reglas de navegacion ni de puntuacion.
    /// </summary>
    public sealed class QuizView : MonoBehaviour
    {
        [Header("Question")]
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text statementText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private QuizAnswerOptionView optionPrefab;

        [Header("Navigation")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button submitButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text messageText;

        [Header("Result")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultScoreText;
        [SerializeField] private Button retryButton;

        private readonly List<QuizAnswerOptionView> optionViews = new();

        public event Action PreviousRequested;
        public event Action NextRequested;
        public event Action SubmitRequested;
        public event Action RetryRequested;
        public event Action<int> AnswerSelected;

        private void Awake()
        {
            previousButton?.onClick.AddListener(HandlePrevious);
            nextButton?.onClick.AddListener(HandleNext);
            submitButton?.onClick.AddListener(HandleSubmit);
            retryButton?.onClick.AddListener(HandleRetry);
        }

        private void OnDestroy()
        {
            previousButton?.onClick.RemoveListener(HandlePrevious);
            nextButton?.onClick.RemoveListener(HandleNext);
            submitButton?.onClick.RemoveListener(HandleSubmit);
            retryButton?.onClick.RemoveListener(HandleRetry);
        }

        public void ShowQuestion(
            string quizTitle,
            QuizQuestion question,
            int questionIndex,
            int questionCount,
            int selectedOption)
        {
            SetActive(questionPanel, true);
            SetActive(resultPanel, false);

            if (titleText != null)
                titleText.text = quizTitle;

            if (progressText != null)
                progressText.text = $"Pregunta {questionIndex + 1} de {questionCount}";

            if (statementText != null)
                statementText.text = question.Statement;

            ClearMessage();
            BuildOptions(question, selectedOption);
            SetNavigation(questionIndex > 0, questionIndex == questionCount - 1);
        }

        public void SetSelectedOption(int selectedOption)
        {
            for (int index = 0; index < optionViews.Count; index++)
                optionViews[index].SetSelected(index == selectedOption);

            ClearMessage();
        }

        public void ShowMessage(string message)
        {
            if (messageText == null)
                return;

            messageText.gameObject.SetActive(true);
            messageText.text = message;
        }

        public void ShowResult(QuizResult result)
        {
            SetActive(questionPanel, false);
            SetActive(resultPanel, true);

            if (resultTitleText != null)
                resultTitleText.text = result.Passed ? "Quiz aprobado" : "Quiz no aprobado";

            if (resultScoreText != null)
            {
                resultScoreText.text =
                    $"Aciertos: {result.CorrectAnswers} de {result.TotalQuestions}\n" +
                    $"Puntuacion: {result.Percentage:0}%";
            }
        }

        private void BuildOptions(QuizQuestion question, int selectedOption)
        {
            if (optionsContainer == null || optionPrefab == null)
            {
                Debug.LogError("Falta configurar el contenedor o prefab de opciones.", this);
                return;
            }

            EnsureOptionCount(question.Options.Count);

            for (int index = 0; index < optionViews.Count; index++)
            {
                bool isVisible = index < question.Options.Count;
                optionViews[index].gameObject.SetActive(isVisible);

                if (isVisible)
                {
                    optionViews[index].Setup(
                        index,
                        question.Options[index],
                        index == selectedOption,
                        HandleAnswerSelected);
                }
            }
        }

        private void EnsureOptionCount(int requiredCount)
        {
            while (optionViews.Count < requiredCount)
                optionViews.Add(Instantiate(optionPrefab, optionsContainer));
        }

        private void SetNavigation(bool canGoPrevious, bool isLastQuestion)
        {
            if (previousButton != null)
                previousButton.interactable = canGoPrevious;

            SetActive(nextButton == null ? null : nextButton.gameObject, !isLastQuestion);
            SetActive(submitButton == null ? null : submitButton.gameObject, isLastQuestion);
        }

        private void ClearMessage()
        {
            if (messageText != null)
            {
                messageText.text = string.Empty;
                messageText.gameObject.SetActive(false);
            }
        }

        private void HandleAnswerSelected(int optionIndex) => AnswerSelected?.Invoke(optionIndex);
        private void HandlePrevious() => PreviousRequested?.Invoke();
        private void HandleNext() => NextRequested?.Invoke();
        private void HandleSubmit() => SubmitRequested?.Invoke();
        private void HandleRetry() => RetryRequested?.Invoke();

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null)
                target.SetActive(value);
        }
    }
}
