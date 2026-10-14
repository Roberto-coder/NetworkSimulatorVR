using System.Collections;
using System;
using System.Collections.Generic;
using Systems.Auth;
using Systems.Scenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Save
{
    /// <summary>Controla el prefab de aviso; no construye elementos de interfaz.</summary>
    public sealed class SyncStatusDialog : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text message;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text retryLabel;
        private SaveManager manager;
        private bool visible;
        private Action retryOperation;
        private bool operationNotice;
        private readonly Dictionary<GameObject, bool> previousContent = new();

        private void Awake() => panel.SetActive(false);

        private void OnEnable() => StartCoroutine(BindManager());

        private IEnumerator BindManager()
        {
            // Esperar la selección inicial de PanelController/CanvasController.
            yield return null;
            while (SaveManager.Instance == null) yield return null;
            manager = SaveManager.Instance;
            retryButton.onClick.AddListener(Retry);
            closeButton.onClick.AddListener(Close);
            manager.StatusChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (manager != null) manager.StatusChanged -= Refresh;
            retryButton?.onClick.RemoveListener(Retry);
            closeButton?.onClick.RemoveListener(Close);
            manager = null;
            Close();
        }

        public static void ShowNotice(string heading, string detail, Action retry)
        {
            SyncStatusDialog dialog = FindFirstObjectByType<SyncStatusDialog>();
            if (dialog == null) { Debug.LogWarning(detail); return; }
            dialog.retryOperation = retry;
            dialog.operationNotice = true;
            dialog.visible = true;
            dialog.HidePreviousContent();
            dialog.title.text = heading;
            dialog.message.text = detail;
            dialog.retryButton.interactable = retry != null;
            dialog.retryLabel.text = "Reintentar";
            dialog.panel.SetActive(true);
        }

        public static void DismissNotice()
        {
            SyncStatusDialog dialog = FindFirstObjectByType<SyncStatusDialog>();
            if (dialog != null) dialog.Close();
        }

        private void Refresh()
        {
            if (operationNotice) return;
            if (manager == null || !SessionContext.CanSyncToFirebase) { Close(); return; }
            bool needsAttention = manager.State == SaveManager.SyncState.Offline ||
                manager.State == SaveManager.SyncState.Error || manager.State == SaveManager.SyncState.WaitingForServer;
            if (needsAttention && !visible)
            {
                visible = true;
                HidePreviousContent();
            }
            if (manager.State == SaveManager.SyncState.Synced || manager.State == SaveManager.SyncState.Local)
                Close();
            if (!visible) return;
            bool busy = manager.IsSyncInProgress;
            title.text = manager.State == SaveManager.SyncState.Offline ? "Conexión necesaria" :
                busy ? "Sincronización en curso" : "No se pudo sincronizar";
            message.text = manager.State == SaveManager.SyncState.Offline
                ? "Necesitas conexión a Internet para sincronizar con Firebase. Tu progreso local se conserva. Conéctate y pulsa Reintentar."
                : manager.StatusMessage;
            retryButton.interactable = !busy;
            retryLabel.text = busy ? "Esperando confirmación…" : "Reintentar";
            panel.SetActive(!SceneTransitionManager.IsLoading);
        }

        private void Retry()
        {
            if (retryOperation != null)
            {
                Action retry = retryOperation;
                Close();
                retry();
                return;
            }
            if (manager == null || manager.IsSyncInProgress || !SessionContext.CanSyncToFirebase) return;
            manager.SyncLocalToFirebase();
        }

        private void Close()
        {
            visible = false;
            retryOperation = null;
            operationNotice = false;
            panel.SetActive(false);
            RestorePreviousContent();
        }

        private void HidePreviousContent()
        {
            if (previousContent.Count != 0 || transform.parent == null) return;
            // Conservar el canvas y sus componentes de interacción; ocultar solo
            // sus ramas de UI. El aviso es otro hijo de ese mismo canvas.
            foreach (Transform sibling in transform.parent)
            {
                if (sibling == transform || !(sibling is RectTransform) ||
                    sibling.GetComponent<Oculus.Interaction.PointableCanvas>() != null) continue;
                previousContent.Add(sibling.gameObject, sibling.gameObject.activeSelf);
                sibling.gameObject.SetActive(false);
            }
        }

        private void RestorePreviousContent()
        {
            foreach (var entry in previousContent)
                if (entry.Key != null) entry.Key.SetActive(entry.Value);
            previousContent.Clear();
        }

        private void LateUpdate()
        {
            if (!visible) return;
            foreach (var entry in previousContent)
                if (entry.Key != null && entry.Key.activeSelf) entry.Key.SetActive(false);
            panel.SetActive(!SceneTransitionManager.IsLoading);
        }
    }
}
