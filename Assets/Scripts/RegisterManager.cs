using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

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

        // VALIDACIONES

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
        Debug.Log(FirebaseAuthManager.Instance);

        FirebaseAuthManager.Instance.Register(username,email,password,(success,msg)=>
        {
            if(success)
            {
                ShowMessage("Operación exitosa",Color.green);
                StartCoroutine(RedirectLogin());
            }
            else
            {
                ShowMessage(msg,Color.red);
            }
        });
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