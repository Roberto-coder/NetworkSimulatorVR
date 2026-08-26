using System.Collections;
using Modules.MainMenu;
using TMPro;
using UnityEngine;

namespace Systems.Auth
{
    public class LoginManager : MonoBehaviour
    {
        public TMP_InputField userField;
        public TMP_InputField passwordField;

        public TextMeshProUGUI messageText;

        public CanvasController canvasController;

        public void Login()
        {
            string user = userField.text;
            string password = passwordField.text;

            if(user == "" || password == "")
            {
                ShowMessage("Completa todos los campos",Color.red);
                return;
            }

            FirebaseAuthManager.Instance.Login(user,password,(success,msg)=>
            {
                if(success)
                {
                    ShowMessage("Login exitoso",Color.green);

                    if (SessionContext.IsDebugSession)
                    {
                        SaveManager.Instance.LoadFromLocal();
                        canvasController.ShowCanvas("MainMenu");
                        return;
                    }

                    FirebaseSaveManager.Instance.DownloadSave((json)=>
                    {
                        Debug.Log("Callback DownloadSave ejecutado");
                        RestoreSaveAndShowMenu(json);
                    });
                }
                else
                {
                    ShowMessage(msg,Color.red);
                }
            });
        }
    
        public void OnGoogleLoginClick()
        {
            ShowMessage("Conectando con Google...", Color.white);

            FirebaseAuthManager.Instance.LoginWithGoogle((success, msg) => {
                if (success)
                {
                    ShowMessage(msg, Color.green);
            
                    // Reutilizamos tu lógica de descarga de datos
                    FirebaseSaveManager.Instance.DownloadSave((json) => {
                        RestoreSaveAndShowMenu(json);
                    });
                }
                else
                {
                    ShowMessage(msg, Color.red);
                }
            });
        }

        IEnumerator RedirectMenu()
        {
            yield return new WaitForSeconds(3);

            canvasController.ShowCanvas("MainMenu");
        }
    
        public void Logout()
        {
            Debug.Log("Cerrando sesión...");
            canvasController.ShowCanvas("Logout");
            passwordField.text = string.Empty;
        }

        private void RestoreSaveAndShowMenu(string remoteJson)
        {
            bool hasPendingChanges = SaveManager.Instance.RestoreSessionData(remoteJson);
            canvasController.ShowCanvas("MainMenu");
            if (hasPendingChanges)
                SaveManager.Instance.SyncLocalToFirebase();
        }
    

        void ShowMessage(string msg,Color color)
        {
            messageText.text = msg;
            messageText.color = color;
        }
    }
}
