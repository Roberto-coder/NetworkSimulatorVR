using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Lobby.UI
{
    public class PanelController : MonoBehaviour
    {
        [Header("Paneles de UI")]
        public GameObject contentMain; // Arrastra tu ContentMain aquí en el Inspector
        public GameObject contentSave;
        public GameObject contentModules;

        void Start()
        {
            // Al iniciar, nos aseguramos de que solo ContentMain esté visible
            ShowMainContent();
        }

        public void ShowMainContent()
        {
            contentMain.SetActive(true);
            contentSave.SetActive(false);
            contentModules.SetActive(false);
        }

        public void ShowCanvas(string canvas)
        {
            contentMain.SetActive(false);
            contentModules.SetActive(false);
            contentSave.SetActive(false);

            switch (canvas)
            {
                case "ContentMain":
                    contentMain.SetActive(true);
                    break;

                case "ContentModules":
                    contentModules.SetActive(true);
                    break;

                case "ContentSave":
                    contentSave.SetActive(true);
                    break;

                case "BackToMain":
                    contentMain.SetActive(true);
                    break;
            }
        }

        public void ReturnToMenu()
        {
            // Reemplaza "NombreDeTuEscenaMenu" por el nombre exacto 
            // de tu escena de menú en el Build Settings
            SceneManager.LoadScene("Menu");
        }
    }
}
