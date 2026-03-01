using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class DatabaseManager : MonoBehaviour
{
    DatabaseReference reference;

    void Start()
    {
        // Inicializar Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            
            Debug.Log("Firebase conectado correctamente");
            SaveData("Jugador1", 100);
        });
    }

    public void SaveData(string userId, int score)
    {
        // Crea un objeto simple para guardar
        reference.Child("users").Child(userId).Child("score").SetValueAsync(score);
        Debug.Log("Puntaje guardado!");
    }
}