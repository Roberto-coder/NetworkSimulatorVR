using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using Google;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance;

    FirebaseAuth auth;
    DatabaseReference db;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // REGISTRO
    public void Register(string username, string email, string password, Action<bool,string> callback)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    callback(false,"Error al registrar usuario");
                    return;
                }

                AuthResult result = task.Result;
                FirebaseUser user = result.User;

                SaveUserProfile(user.UserId, username, email);

                callback(true,"Registro exitoso");
            });
    }

    // LOGIN
    public void Login(string username, string password, Action<bool,string> callback)
    {
        
        if(username == "userdebug" && password == "userdebug")
        {
            callback(true,"Login debug");
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            callback(false,"Sin conexión a internet");
            return;
        }
        
        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .OrderByChild("perfil/usuario")
            .EqualTo(username)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted || task.Result.ChildrenCount == 0)
                {
                    callback(false,"Usuario no encontrado");
                    return;
                }

                foreach (var user in task.Result.Children)
                {
                    string email = user.Child("perfil").Child("gmail").Value.ToString();

                    auth.SignInWithEmailAndPasswordAsync(email,password)
                        .ContinueWithOnMainThread(loginTask =>
                        {
                            if (loginTask.IsCanceled || loginTask.IsFaulted)
                            {
                                callback(false,"Contraseña incorrecta");
                                return;
                            }

                            callback(true,"Login exitoso");
                        });

                    break;
                }
            });
    }

    public void LoginWithGoogle(Action<bool, string> callback)
    {
        Debug.Log("SISTEMA: Iniciando proceso de Google Sign-In");
        try
        {
            // Configuración de Google Sign-In
            GoogleSignInConfiguration configuration = new GoogleSignInConfiguration {
                //TU_WEB_CLIENT_ID_DE_FIREBASE.apps.googleusercontent.com
                WebClientId = "648301969088-u27b596rol2n0nmffu0b7922ndu5ivph.apps.googleusercontent.com",
                RequestIdToken = true
            };
            
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task => {
                Debug.Log("SISTEMA: Respuesta recibida de Google");
                if (task.IsFaulted || task.IsCanceled) {
                    callback(false, "Error en Google Sign-In");
                    return;
                }

                // Obtener el ID Token de Google
                string idToken = task.Result.IdToken;
                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                // Autenticar en Firebase con esa credencial
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(firebaseTask => {
                    if (firebaseTask.IsFaulted || firebaseTask.IsCanceled) {
                        callback(false, "Error al vincular con Firebase");
                        return;
                    }

                    FirebaseUser newUser = firebaseTask.Result;
            
                    // Lógica del username: correo sin @gmail.com
                    string email = newUser.Email;
                    string generatedUsername = email.Split('@')[0];

                    // Verificar si el perfil ya existe en la DB, si no, crearlo
                    CheckAndCreateProfile(newUser.UserId, generatedUsername, email, callback);
                });
            });

 
        }
        catch (Exception e)
        {
            Debug.LogError("SISTEMA ERROR: " + e.Message);
        }
        
    }

    private void CheckAndCreateProfile(string uid, string username, string email, Action<bool, string> callback)
    {
        db.Child("simulador_redes_vr").Child("usuarios").Child(uid).Child("perfil").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && !task.Result.Exists)
            {
                // Si el perfil no existe en la base de datos (usuario nuevo), lo creamos
                SaveUserProfile(uid, username, email);
            }
            callback(true, "Login Google exitoso");
        });
    }
    
    // GUARDAR PERFIL
    void SaveUserProfile(string uid,string username,string email)
    {
        UserProfile profile = new UserProfile(username,email);

        string json = JsonUtility.ToJson(profile);

        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("perfil")
            .SetRawJsonValueAsync(json);
    }

    // LOGOUT
    public void Logout()
    {
        auth.SignOut();
        
    }
}