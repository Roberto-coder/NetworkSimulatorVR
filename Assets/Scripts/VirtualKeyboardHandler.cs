using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
// --- NUEVOS NAMESPACES ---
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class VirtualKeyboardHandler : MonoBehaviour
{
    public NonNativeKeyboard keyboard;
    public TMP_InputField separateInputField;
    bool isTextSubmited = false;

    // Referencia de Firebase
    DatabaseReference dbReference;

    void Start()
    {
        // Inicializamos Firebase al arrancar
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase listo para recibir datos.");
        });
    }

    private void OnEnable()
    {
        if (keyboard != null)
        {
            keyboard.OnTextSubmitted += HandleTextSubmitted;
            keyboard.OnTextUpdated += HandleTextUpdated;
        }
    }

    private void OnDisable()
    {
        if (keyboard != null)
        {
            keyboard.OnTextSubmitted -= HandleTextSubmitted;
            keyboard.OnTextUpdated -= HandleTextUpdated;
        }
    }

    void HandleTextUpdated(string text)
    {
        if (!isTextSubmited && separateInputField != null)
        {
            separateInputField.text = text;
        }
        isTextSubmited = false;
    }

    void HandleTextSubmitted(object sender, EventArgs e)
    {
        isTextSubmited = true;
        string username = keyboard.InputField.text;
        separateInputField.text = username;

        if (!string.IsNullOrEmpty(username))
        {
            LoginOrRegister(username);
        }
    }

    // --- FUNCIÓN DE FIREBASE ---
    void LoginOrRegister(string userName)
    {
        Debug.Log("Intentando registrar/loguear a: " + userName);

        // Guardamos o actualizamos al usuario en la DB
        // Ruta: users -> NombreUsuario -> lastLogin
        dbReference.Child("users").Child(userName).Child("lastLogin").SetValueAsync(DateTime.Now.ToString());
        
        // También podrías inicializar un puntaje si es nuevo
        dbReference.Child("users").Child(userName).Child("score").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists) {
                    // Si no existe, es registro: creamos puntaje 0
                    dbReference.Child("users").Child(userName).Child("score").SetValueAsync(0);
                    Debug.Log("Nuevo usuario registrado!");
                } else {
                    Debug.Log("Bienvenido de nuevo, tu puntaje es: " + snapshot.Value);
                }
            }
        });
    }
}