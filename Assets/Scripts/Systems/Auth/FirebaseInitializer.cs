using Firebase;
using Firebase.Extensions;
using UnityEngine;

namespace Systems.Auth
{
    public class FirebaseInitializer : MonoBehaviour
    {
        void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var status = task.Result;

                if (status == DependencyStatus.Available)
                {
                    Debug.Log("Firebase listo");
                }
                else
                {
                    Debug.LogError("Firebase error: " + status);
                }
            });
        }
    }
}