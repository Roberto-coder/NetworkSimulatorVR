using UnityEngine;
using TMPro;
using System.Collections;

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

                FirebaseSaveManager.Instance.DownloadSave((json)=>
                {
                    Debug.Log("Callback DownloadSave ejecutado");

                    if(json != null)
                    {
                        string path = Application.persistentDataPath + "/save.json";

                        Debug.Log("Ruta save: " + path);

                        System.IO.File.WriteAllText(path,json);
                    }

                    SaveManager.Instance.LoadFromLocal();

                    
                    Debug.Log("Intentando abrir MainMenu");
                    canvasController.ShowCanvas("MainMenu");
                });
            }
            else
            {
                ShowMessage(msg,Color.red);
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
        FirebaseAuthManager.Instance.Logout();
        canvasController.ShowCanvas("Logout");
    }
    

    void ShowMessage(string msg,Color color)
    {
        messageText.text = msg;
        messageText.color = color;
    }
}