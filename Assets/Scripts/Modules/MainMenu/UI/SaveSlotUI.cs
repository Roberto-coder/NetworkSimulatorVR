using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.MainMenu.UI
{
    public class SaveSlotUI : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI date;
        public TextMeshProUGUI playtime;

        public Button slotButton;

        public int slotID;
        bool isEmpty;

        public void Setup(SaveSlot slot)
        {
            slotID = slot.slotID;
            isEmpty = false;

            title.text = slot.moduleTitle;
            date.text = slot.lastSave;

            TimeSpan t = TimeSpan.FromSeconds(slot.playTime);

            playtime.text =
                t.Hours + "h " +
                t.Minutes + "m " +
                t.Seconds + "s";

            slotButton.onClick.AddListener(OnClickSlot);
        }

        public void SetupEmpty(int id)
        {
            slotID = id;
            isEmpty = true;

            title.text = "Nueva Partida";
            date.text = "---";
            playtime.text = "0h 0m 0s";

            slotButton.onClick.AddListener(OnClickSlot);
        }

        void OnClickSlot()
        {
            if(isEmpty)
            {
                CreateNewGame();
            }
            else
            {
                LoadGame();
            }
        }

        void CreateNewGame()
        {
            Debug.Log("Crear nueva partida en slot: " + slotID);

            SaveManager.Instance.SaveGame(
                slotID,
                "Cableado / Modulo1",
                0
            );
        }

        void LoadGame()
        {
            Debug.Log("Cargar partida slot: " + slotID);

            // aquí cargarías la escena del juego
        }
    }
}