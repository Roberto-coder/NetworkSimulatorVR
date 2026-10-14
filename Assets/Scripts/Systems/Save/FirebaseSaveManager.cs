using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using Firebase.Extensions;
using Systems.Auth;

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
        return SessionContext.CanSyncToFirebase ? SessionContext.UserId : null;
    }

    // SUBIR SAVE (guardado manual)
    public void UploadSave(SaveFile saveFile, Action<bool, string> callback = null)
    {
        string uid = GetUserID();

        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("No se puede sincronizar sin un usuario autenticado.");
            callback?.Invoke(false, "No hay una sesión de Firebase activa");
            return;
        }

        string json = JsonUtility.ToJson(saveFile);

        try
        {
        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("saveData")
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
          {
              if(task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
              {
                  Debug.Log("Save subido a Firebase");
                  callback?.Invoke(true, "Guardado en la nube");
              }
              else
              {
                  Debug.LogError("Error subiendo save");
                  callback?.Invoke(false, "Error al guardar en Firebase");
              }
          });
        }
        catch (Exception exception)
        {
            Debug.LogError("No se pudo iniciar la subida: " + exception.Message);
            callback?.Invoke(false, "No se pudo iniciar la sincronizacion");
        }
    }

    // DESCARGAR SAVE (login)
    public void DownloadSave(System.Action<string> callback)
    {
        DownloadSaveWithStatus((success, json, _) => callback?.Invoke(success ? json : null));
    }

    public void DownloadSaveWithStatus(Action<bool, string, string> callback)
    {
        string uid = GetUserID();
        if (string.IsNullOrEmpty(uid)) { callback?.Invoke(false, null, "No hay una sesión de Firebase activa."); return; }
        if (Systems.Save.NetworkAvailability.IsOffline)
        {
            callback?.Invoke(false, null, "Necesitas conexión a Internet para consultar el progreso de Firebase.");
            return;
        }

        Debug.Log("Intentando descargar save para UID: " + uid);
        try
        {
        db.Child("simulador_redes_vr")
            .Child("usuarios")
            .Child(uid)
            .Child("saveData")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (uid != GetUserID()) return;
                Debug.Log("Firebase respondió");

                if(task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    var snapshot = task.Result;

                    if(snapshot.Exists)
                    {
                        Debug.Log("Save encontrado");

                        string json = snapshot.GetRawJsonValue();

                        callback?.Invoke(true, json, null);
                    }
                    else
                    {
                        Debug.Log("No existe save aún");

                        callback?.Invoke(true, null, null);
                    }
                }
                else
                {
                    Debug.LogError("Error descargando save");

                    callback?.Invoke(false, null, "No se pudo consultar Firebase. Comprueba la conexión y vuelve a intentar.");
                }
            });
        }
        catch (Exception exception)
        {
            Debug.LogError("No se pudo iniciar la descarga: " + exception.Message);
            callback?.Invoke(false, null, "No se pudo iniciar la consulta de Firebase.");
        }
    }
}
