using Modules.Module02_RackInstallation.Exploration;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Presentation
{
    public enum InfoCardState { Hidden, Focusing, Open, Pinned }

    public sealed class InfoCardController : MonoBehaviour
    {
        [SerializeField] private InfoFocusDetector focusDetector;
        [SerializeField] private InfoCardView view;
        [SerializeField] private Camera playerCamera;
        [Min(0.4f)] [SerializeField] private float distanceFromCamera = 0.85f;
        [SerializeField] private float horizontalOffset = 0.48f;
        [SerializeField] private float verticalOffset = -0.05f;
        [Min(0.2f)] [SerializeField] private float closeDelay = 1.5f;
        private RackInfoTarget displayedTarget;
        private int page;
        private float closeAt = -1f;
        public InfoCardState State { get; private set; }

        private void Awake()
        {
            if (focusDetector == null) focusDetector = FindFirstObjectByType<InfoFocusDetector>();
            if (view == null) view = GetComponentInChildren<InfoCardView>(true);
            if (playerCamera == null) playerCamera = Camera.main;
            Close();
        }

        private void OnEnable()
        {
            if (focusDetector == null || view == null) return;
            focusDetector.FocusProgressChanged += HandleProgress;
            focusDetector.CardRequested += HandleCardRequested;
            focusDetector.FocusCleared += HandleFocusCleared;
            if (view.CloseButton != null) view.CloseButton.onClick.AddListener(Close);
            if (view.PinButton != null) view.PinButton.onClick.AddListener(TogglePin);
            if (view.NextButton != null) view.NextButton.onClick.AddListener(NextPage);
            if (view.PreviousButton != null) view.PreviousButton.onClick.AddListener(PreviousPage);
        }

        private void OnDisable()
        {
            if (focusDetector != null)
            {
                focusDetector.FocusProgressChanged -= HandleProgress;
                focusDetector.CardRequested -= HandleCardRequested;
                focusDetector.FocusCleared -= HandleFocusCleared;
            }
            if (view != null)
            {
                if (view.CloseButton != null) view.CloseButton.onClick.RemoveListener(Close);
                if (view.PinButton != null) view.PinButton.onClick.RemoveListener(TogglePin);
                if (view.NextButton != null) view.NextButton.onClick.RemoveListener(NextPage);
                if (view.PreviousButton != null) view.PreviousButton.onClick.RemoveListener(PreviousPage);
            }
            Close();
        }

        private void Update()
        {
            if (State != InfoCardState.Open && State != InfoCardState.Pinned) return;
            if (displayedTarget == null || !displayedTarget.isActiveAndEnabled) { Close(); return; }
            if (State == InfoCardState.Pinned) return;
            if (view.IsPointerOverCard || focusDetector.CurrentTarget == displayedTarget)
            {
                closeAt = -1f;
                return;
            }
            if (closeAt < 0f) closeAt = Time.unscaledTime + Mathf.Max(0.2f, closeDelay);
            if (Time.unscaledTime >= closeAt) Hide(false);
        }

        public void Close()
        {
            Hide(true);
        }

        private void Hide(bool suppressFocus)
        {
            displayedTarget = null;
            page = 0;
            closeAt = -1f;
            State = InfoCardState.Hidden;
            view?.HideCard();
            view?.HideDwell();
            if (suppressFocus) focusDetector?.SuppressCurrent();
        }

        public void TogglePin()
        {
            if (displayedTarget == null) return;
            bool pin = State != InfoCardState.Pinned;
            State = pin ? InfoCardState.Pinned : InfoCardState.Open;
            closeAt = -1f;
            view.SetPinned(pin);
            if (!pin) focusDetector.RestartCurrent();
        }

        public void NextPage() => ChangePage(1);
        public void PreviousPage() => ChangePage(-1);

        private void ChangePage(int delta)
        {
            if (displayedTarget == null) return;
            page = Mathf.Clamp(page + delta, 0, displayedTarget.Information.Sections.Count);
            view.Show(displayedTarget.Information, page);
            view.SetPinned(State == InfoCardState.Pinned);
        }

        private void HandleProgress(RackInfoTarget target, float progress)
        {
            if (State == InfoCardState.Pinned || view.IsPointerOverCard || displayedTarget != null) return;
            State = InfoCardState.Focusing;
            PositionInFrontOfPlayer();
            view.ShowDwell(progress);
        }

        private void HandleCardRequested(RackInfoTarget target)
        {
            if (State == InfoCardState.Pinned || view.IsPointerOverCard || target.Information == null) return;
            if (displayedTarget == target) return;
            displayedTarget = target;
            page = 0;
            closeAt = -1f;
            State = InfoCardState.Open;
            PositionInFrontOfPlayer();
            view.Show(target.Information, page);
            view.SetPinned(false);
        }

        private void HandleFocusCleared(RackInfoTarget target)
        {
            view.HideDwell();
            if (State == InfoCardState.Focusing) State = InfoCardState.Hidden;
            // Update da tiempo para pasar el ray desde el objeto hacia el Canvas.
        }

        private void PositionInFrontOfPlayer()
        {
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null || view == null) return;
            Transform head = playerCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.ProjectOnPlane(head.up, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 position = head.position + forward * Mathf.Max(0.4f, distanceFromCamera) +
                               right * horizontalOffset + Vector3.up * verticalOffset;
            view.transform.SetPositionAndRotation(position, Quaternion.LookRotation(position - head.position, Vector3.up));
            // Permanece inmóvil mientras se lee, incluso al mover la cabeza.
        }
    }
}
