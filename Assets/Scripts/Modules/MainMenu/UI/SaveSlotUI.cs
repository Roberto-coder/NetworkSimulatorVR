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

        public void Setup(SaveSlot slot)
        {
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
        }

        public void SetupEmpty(int id)
        {
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
            if (SaveManager.Instance == null || isEmpty)
                return;

            deleteConfirmationPending = false;

            date.text = "Sincronizando...";
            date.color = Color.white;
            SaveManager.Instance.SyncLocalToFirebase((success, message) =>
            {
                date.text = message;
                date.color = success ? Color.green : new Color(1f, 0.65f, 0f);
                if (success)
                    title.text = title.text.Replace("! ", string.Empty);
            });
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
            date.text = "Slot eliminado localmente";
            SaveManager.Instance.SyncLocalToFirebase((success, message) =>
            {
                date.text = success ? "Slot eliminado y sincronizado" : message;
                date.color = success ? Color.green : new Color(1f, 0.65f, 0f);
            });
        }
    }
}
