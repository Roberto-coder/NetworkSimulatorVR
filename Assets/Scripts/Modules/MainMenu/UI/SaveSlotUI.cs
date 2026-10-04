using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Systems.Scenes;

namespace Modules.MainMenu.UI
{
    public class SaveSlotUI : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI date;
        public TextMeshProUGUI playtime;

        [FormerlySerializedAs("slotButton")]
        public Button loadButton;
        public Button deleteButton;
        public Button saveButton;

        public int slotID;
        private bool isEmpty;
        private bool deleteConfirmationPending;
        private SaveManager boundManager;

        private void OnEnable() => BindStatus();
        private void OnDisable()
        {
            if (boundManager != null) boundManager.StatusChanged -= RefreshSyncStatus;
            boundManager = null;
        }
        private void BindStatus()
        {
            if (boundManager != null) boundManager.StatusChanged -= RefreshSyncStatus;
            boundManager = SaveManager.Instance;
            if (boundManager != null) boundManager.StatusChanged += RefreshSyncStatus;
        }
        private void RefreshSyncStatus()
        {
            var manager = SaveManager.Instance;
            if (manager == null) return;
            var slot = manager.saveFile?.slots?.Find(item => item.slotID == slotID);
            if (title != null)
            {
                title.text = title.text.Replace("! ", string.Empty);
                if (slot != null && slot.needsCloudSync) title.text = "! " + title.text;
            }
            if (date != null)
            {
                date.text = manager.StatusMessage;
                date.color = manager.State == SaveManager.SyncState.Synced ? Color.green : Color.white;
            }
            if (saveButton != null)
            {
                saveButton.interactable = !manager.IsSyncInProgress && (!isEmpty || manager.HasPendingChanges);
                var label = saveButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = "Sincronizar todo";
            }
        }

        public void Setup(SaveSlot slot)
        {
            BindStatus();
            RemoveListeners();
            slotID = slot.slotID;
            isEmpty = slot.data == null;
            if (isEmpty)
            {
                SetupEmpty(slotID);
                return;
            }

            string slotTitle = string.IsNullOrWhiteSpace(slot.moduleTitle)
                ? $"Partida {slotID + 1}"
                : slot.moduleTitle;
            bool isActive = SaveManager.Instance?.saveFile?.activeSlotId == slotID;
            string activeIndicator = isActive ? "● " : string.Empty;
            string pendingIndicator = slot.needsCloudSync ? "! " : string.Empty;
            title.text = $"{activeIndicator}{pendingIndicator}{slotTitle}";
            date.text = string.IsNullOrWhiteSpace(slot.lastSave) ? "---" : slot.lastSave;
            TimeSpan elapsed = TimeSpan.FromSeconds(slot.playTime);
            int achievementCount = slot.data?.achievements?.Count ?? 0;
            playtime.text = $"{elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s  |  Insignias: {achievementCount}";
            deleteConfirmationPending = false;
            BindListeners();
            SetActionAvailability(true);
            RefreshSyncStatus();
        }

        public void SetupEmpty(int id)
        {
            BindStatus();
            RemoveListeners();
            slotID = id;
            isEmpty = true;
            title.text = $"Nueva partida {id + 1}";
            date.text = "---";
            date.color = Color.white;
            playtime.text = "0h 0m 0s";
            deleteConfirmationPending = false;
            BindListeners();
            SetActionAvailability(false);
            RefreshSyncStatus();
        }

        private void BindListeners()
        {
            loadButton?.onClick.AddListener(LoadGame);
            saveButton?.onClick.AddListener(SyncSlot);
            deleteButton?.onClick.AddListener(DeleteSlot);
        }

        private void RemoveListeners()
        {
            loadButton?.onClick.RemoveListener(LoadGame);
            saveButton?.onClick.RemoveListener(SyncSlot);
            deleteButton?.onClick.RemoveListener(DeleteSlot);
        }

        private void SetActionAvailability(bool hasData)
        {
            if (saveButton != null)
                saveButton.interactable = hasData;
            if (deleteButton != null)
                deleteButton.interactable = hasData;
        }

        private void LoadGame()
        {
            if (SaveManager.Instance == null)
                return;

            deleteConfirmationPending = false;

            if (isEmpty)
                SaveManager.Instance.SaveGame(slotID, "Nueva partida", 0f);
            else
                SaveManager.Instance.SelectSlot(slotID);

            Debug.Log($"Partida cargada desde slot: {slotID}", this);
            if (SceneManager.GetActiveScene().name == "Menu")
                SceneTransitionManager.LoadScene("Lobby");
        }

        private void SyncSlot()
        {
            if (SaveManager.Instance == null)
                return;

            deleteConfirmationPending = false;

            date.text = "Sincronizando...";
            date.color = Color.white;
            SaveManager.Instance.SyncLocalToFirebase();
        }

        private void DeleteSlot()
        {
            if (SaveManager.Instance == null || isEmpty)
                return;

            if (!deleteConfirmationPending)
            {
                deleteConfirmationPending = true;
                date.text = "Pulsa eliminar otra vez para confirmar";
                date.color = new Color(1f, 0.65f, 0f);
                return;
            }

            SaveManager.Instance.DeleteSlot(slotID);
            SetupEmpty(slotID);
            date.text = "Eliminado en este dispositivo. Pulsa Sincronizar todo para actualizar Firebase.";
        }
    }
}
