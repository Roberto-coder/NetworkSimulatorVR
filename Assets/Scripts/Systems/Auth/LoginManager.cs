using System.Collections;
using Modules.MainMenu;
using TMPro;
using UnityEngine;
using Systems.Save;

namespace Systems.Auth
{
    public class LoginManager : MonoBehaviour
    {
        public TMP_InputField userField;
        public TMP_InputField passwordField;

        public TextMeshProUGUI messageText;

        public CanvasController canvasController;
        private bool submitting;

        public void ClearForm()
        {
            if (userField != null)
            {
                userField.DeactivateInputField();
                userField.SetTextWithoutNotify(string.Empty);
            }
            if (passwordField != null)
            {
                passwordField.DeactivateInputField();
                passwordField.SetTextWithoutNotify(string.Empty);
            }
            if (messageText != null) messageText.text = string.Empty;
        }

        public void Login()
        {
            if (submitting) return;
            string user = userField.text;
            string password = passwordField.text;

            if(user == "" || password == "")
            {
                ShowMessage("Completa todos los campos",Color.red);
                return;
            }

            SyncStatusDialog.DismissNotice();
            if (!(user == "userdebug" && password == "userdebug") &&
                Systems.Save.NetworkAvailability.IsOffline)
            {
                SyncStatusDialog.ShowNotice("Conexión necesaria", "Necesitas conexión a Internet para iniciar sesión con Firebase. Conéctate y pulsa Reintentar.", Login);
                return;
            }
            submitting = true;

            FirebaseAuthManager.Instance.Login(user,password,(success,msg)=>
            {
                submitting = false;
                if(success)
                {
                    ShowMessage("Login exitoso",Color.green);

                    if (SessionContext.IsDebugSession)
                    {
                        SaveManager.Instance.LoadFromLocal();
                        canvasController.ShowCanvas("MainMenu");
                        return;
                    }

                    DownloadProgress();
                }
                else
                {
                    ShowMessage(msg,Color.red);
                    SyncStatusDialog.ShowNotice("No se pudo iniciar sesión", msg, Login);
                }
            });
        }
    
        public void OnGoogleLoginClick()
        {
            if (submitting) return;
            SyncStatusDialog.DismissNotice();
            if (Systems.Save.NetworkAvailability.IsOffline)
            {
                SyncStatusDialog.ShowNotice("Conexión necesaria", "Necesitas conexión a Internet para iniciar sesión con Google. Conéctate y pulsa Reintentar.", OnGoogleLoginClick);
                return;
            }
            submitting = true;
            ShowMessage("Conectando con Google...", Color.white);

            FirebaseAuthManager.Instance.LoginWithGoogle((success, msg) => {
                submitting = false;
                if (success)
                {
                    ShowMessage(msg, Color.green);
            
                    // Reutilizamos tu lógica de descarga de datos
                    DownloadProgress();
                }
                else
                {
                    ShowMessage(msg, Color.red);
                    SyncStatusDialog.ShowNotice("No se pudo iniciar sesión", msg, OnGoogleLoginClick);
                }
            });
        }

        private void DownloadProgress()
        {
            if (submitting) return;
            submitting = true;
            SyncStatusDialog.DismissNotice();
            FirebaseSaveManager.Instance.DownloadSaveWithStatus((success, json, detail) =>
            {
                submitting = false;
                if (success) RestoreSaveAndShowMenu(json);
                else
                {
                    SaveManager.Instance.LoadFromLocal();
                    canvasController.ShowCanvas("MainMenu");
                    SyncStatusDialog.ShowNotice("No se pudo consultar Firebase", detail + " Se usa la copia local disponible; cargar una partida local no requiere conexión.", DownloadProgress);
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
        }

        private void RestoreSaveAndShowMenu(string remoteJson)
        {
            SaveManager.Instance.RestoreSessionData(remoteJson);
            canvasController.ShowCanvas("MainMenu");

        }
    

        void ShowMessage(string msg,Color color)
        {
            if (messageText == null || !userField.gameObject.activeInHierarchy) return;
            messageText.text = msg;
            messageText.color = color;
        }
    }
}
