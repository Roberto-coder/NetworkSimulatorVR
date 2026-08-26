using System.Collections;
using TMPro;
using UnityEngine;
using Systems.Auth;
using Systems.Scenes;

namespace Modules.MainMenu
{
    public class CanvasController : MonoBehaviour
    {
        public GameObject MainMenu;
        public GameObject Settings;
        public GameObject Login;
        public GameObject Registro;
        public GameObject SelectorPartidas;

        private TMP_Text newGameStatusText;
        private Coroutine statusMessageRoutine;

        void Start()
        {
            SessionBootstrap bootstrap = GetComponent<SessionBootstrap>();
            if (bootstrap == null)
                bootstrap = gameObject.AddComponent<SessionBootstrap>();
            bootstrap.Begin(this);
        }

        public void ShowCanvas(string canvas)
        {
            MainMenu.SetActive(false);
            Settings.SetActive(false);
            Login.SetActive(false);
            Registro.SetActive(false);
            SelectorPartidas.SetActive(false);

            switch (canvas)
            {
                case "MainMenu":
                    MainMenu.SetActive(true);
                    break;
            
                case "Login":
                    Login.SetActive(true);
                    break;

                case "Registro":
                    Registro.SetActive(true);
                    break;

                case "SelectorPartidas":
                    SelectorPartidas.SetActive(true);
                    break;
            
                case "Settings":
                    Settings.SetActive(true);
                    break;
            
                case "Logout":
                    FirebaseAuthManager.Instance?.Logout();
                    SaveManager.Instance?.ClearInMemorySession();
                    Login.SetActive(true);
                    break;
            }
        }
    
        public void QuitGame()
        {
            Debug.Log("Cerrando juego...");

            Application.Quit();
        }

        public void StartNewGame()
        {
            if (SaveManager.Instance == null)
            {
                ShowNewGameStatus("No se pudo acceder al sistema de guardado.");
                return;
            }

            for (int slotId = 0; slotId < 4; slotId++)
            {
                if (!SaveManager.Instance.IsSlotEmpty(slotId))
                    continue;

                SaveManager.Instance.SaveGame(slotId, "Lobby", 0f);
                SceneTransitionManager.LoadScene("Lobby");
                return;
            }

            ShowNewGameStatus("Los 4 espacios están ocupados. Elimina una partida para liberar espacio.");
        }

        private void ShowNewGameStatus(string message)
        {
            if (newGameStatusText == null)
                newGameStatusText = CreateNewGameStatusText();

            newGameStatusText.text = message;
            newGameStatusText.gameObject.SetActive(true);

            if (statusMessageRoutine != null)
                StopCoroutine(statusMessageRoutine);
            statusMessageRoutine = StartCoroutine(HideNewGameStatusAfterDelay());
        }

        private TMP_Text CreateNewGameStatusText()
        {
            GameObject textObject = new GameObject(
                "NewGameStatusMessage",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(MainMenu.transform, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.08f);
            rect.anchorMax = new Vector2(0.5f, 0.08f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(620f, 70f);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color32(255, 190, 85, 255);
            text.fontSize = 24f;
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private IEnumerator HideNewGameStatusAfterDelay()
        {
            yield return new WaitForSecondsRealtime(4f);
            if (newGameStatusText != null)
                newGameStatusText.gameObject.SetActive(false);
            statusMessageRoutine = null;
        }
    
    
    }
}
