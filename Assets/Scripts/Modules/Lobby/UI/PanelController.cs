using UnityEngine;
using UnityEngine.SceneManagement;
using Systems.Scenes;

namespace Modules.Lobby.UI
{
    public class PanelController : MonoBehaviour
    {
        [Header("Paneles de UI")]
        public GameObject contentMain; // Arrastra tu ContentMain aquí en el Inspector
        public GameObject contentSave;
        public GameObject contentModules;
        public GameObject contentProfile;

        void Start()
        {
            // Al iniciar, nos aseguramos de que solo ContentMain esté visible
            ShowMainContent();
        }

        public void ShowMainContent()
        {
            SetActive(contentMain, true);
            SetActive(contentSave, false);
            SetActive(contentModules, false);
            SetActive(contentProfile, false);
        }

        public void ShowCanvas(string canvas)
        {
            SetActive(contentMain, false);
            SetActive(contentModules, false);
            SetActive(contentSave, false);
            SetActive(contentProfile, false);

            switch (canvas)
            {
                case "ContentMain":
                    SetActive(contentMain, true);
                    break;

                case "ContentModules":
                    SetActive(contentModules, true);
                    break;

                case "ContentSave":
                    SetActive(contentSave, true);
                    break;
                
                case "ContentProfile":
                    SetActive(contentProfile, true);
                    break;

                case "BackToMain":
                    SetActive(contentMain, true);
                    break;
            }
        }

        public void ReturnToMenu()
        {
            // Reemplaza "NombreDeTuEscenaMenu" por el nombre exacto 
            // de tu escena de menú en el Build Settings
            SceneTransitionManager.LoadScene("Menu");
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null)
                target.SetActive(value);
        }
    }
}
