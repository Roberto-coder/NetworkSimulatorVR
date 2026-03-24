using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using Firebase.Auth;
using Firebase.Database;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;

    public TextMeshProUGUI messageText;

    public CanvasController canvasController;

    public void Register()
    {
        string username = usernameField.text;
        string email = emailField.text;
        string password = passwordField.text;
        string confirm = confirmPasswordField.text;

        if(username.Length < 8)
        {
            ShowMessage("Usuario mínimo 8 caracteres",Color.red);
            return;
        }

        if(password.Length < 8)
        {
            ShowMessage("Contraseña mínimo 8 caracteres",Color.red);
            return;
        }

        if(password != confirm)
        {
            ShowMessage("Las contraseñas no coinciden",Color.red);
            return;
        }

        if(!Regex.IsMatch(username,@"^[a-zA-Z0-9]+$"))
        {
            ShowMessage("Sin caracteres especiales",Color.red);
            return;
        }

        FirebaseAuthManager.Instance.Register(username,email,password,(success,msg)=>
        {
            if(success)
            {
                ShowMessage("Operación exitosa",Color.green);

                CreateUserData(username,email);
                
                Debug.Log("Intentando abrir Login");
                canvasController.ShowCanvas("Login");
                //StartCoroutine(RedirectLogin());
            }
            else
            {
                ShowMessage(msg,Color.red);
            }
        });
    }

    void CreateUserData(string username,string email)
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;

        // PERFIL
        UserProfile profile = new UserProfile(username,email);

        string profileJson = JsonUtility.ToJson(profile);

        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("perfil")
            .SetRawJsonValueAsync(profileJson);

        // SAVE INICIAL
        SaveFile save = new SaveFile();

        // crear 4 slots vacíos
        for(int i = 0; i < 4; i++)
        {
            SaveSlot slot = new SaveSlot();
            slot.slotID = i;
            slot.moduleTitle = "";
            slot.lastSave = "";
            slot.playTime = 0;

            save.slots.Add(slot);
        }

        string saveJson = JsonUtility.ToJson(save);

        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("saveData")
            .SetRawJsonValueAsync(saveJson);

        Debug.Log("Perfil y save inicial creados");
    }

    IEnumerator RedirectLogin()
    {
        yield return new WaitForSeconds(5);

        canvasController.ShowCanvas("Login");
    }

    void ShowMessage(string msg,Color color)
    {
        messageText.text = msg;
        messageText.color = color;
    }
}