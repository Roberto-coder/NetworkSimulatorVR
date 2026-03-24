using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

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