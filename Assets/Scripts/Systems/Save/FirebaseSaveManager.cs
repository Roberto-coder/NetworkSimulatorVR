using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using Firebase.Extensions;

public class FirebaseSaveManager : MonoBehaviour
{
    public static FirebaseSaveManager Instance;

    DatabaseReference db;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    string GetUserID()
    {
        return FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    }

    // SUBIR SAVE (guardado manual)
    public void UploadSave(SaveFile saveFile)
    {
        string uid = GetUserID();

        string json = JsonUtility.ToJson(saveFile);

        db.Child("simulador_redes_vr")
            .Child("users")
            .Child(uid)
            .Child("saveData")
            .SetRawJsonValueAsync(json)
            .ContinueWith(task =>
          {
              if(task.IsCompleted)
              {
                  Debug.Log("Save subido a Firebase");
              }
              else
              {
                  Debug.LogError("Error subiendo save");
              }
          });
    }

    // DESCARGAR SAVE (login)
    public void DownloadSave(System.Action<string> callback)
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        Debug.Log("Intentando descargar save para UID: " + uid);

        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("saveData")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("Firebase respondió");

                if(task.IsCompleted && !task.IsFaulted)
                {
                    var snapshot = task.Result;

                    if(snapshot.Exists)
                    {
                        Debug.Log("Save encontrado");

                        string json = snapshot.GetRawJsonValue();

                        callback?.Invoke(json);
                    }
                    else
                    {
                        Debug.Log("No existe save aún");

                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError("Error descargando save");

                    callback?.Invoke(null);
                }
            });
    }
}