using System.Collections;
using Modules.MainMenu;
using UnityEngine;

namespace Systems.Auth
{
    /// <summary>
    /// Restaura la sesión persistida por Firebase y decide la pantalla inicial.
    /// Firebase conserva/refresca el token; este bootstrap recupera perfil y save.
    /// </summary>
    public sealed class SessionBootstrap : MonoBehaviour
    {
        private CanvasController canvasController;
        private bool started;

        public void Begin(CanvasController controller)
        {
            if (started) return;
            started = true;
            canvasController = controller;
            StartCoroutine(RestoreSession());
        }

        private IEnumerator RestoreSession()
        {
            yield return null;

            if (FirebaseAuthManager.Instance == null || SaveManager.Instance == null)
            {
                Debug.LogError("No están disponibles los managers necesarios para restaurar la sesión.", this);
                canvasController.ShowCanvas("Login");
                yield break;
            }

            bool authFinished = false;
            bool sessionRestored = false;
            FirebaseAuthManager.Instance.RestoreCurrentSession((success, _) =>
            {
                sessionRestored = success;
                authFinished = true;
            });

            float timeoutAt = Time.realtimeSinceStartup + 10f;
            while (!authFinished && Time.realtimeSinceStartup < timeoutAt)
                yield return null;

            if (!authFinished && SessionContext.RestorePersistedFirebaseSession())
                sessionRestored = true;
            if (!sessionRestored)
            {
                SaveManager.Instance.ClearInMemorySession();
                canvasController.ShowCanvas("Login");
                yield break;
            }

            if (FirebaseSaveManager.Instance == null)
            {
                SaveManager.Instance.LoadFromLocal();
                canvasController.ShowCanvas("MainMenu");
                yield break;
            }

            bool downloadFinished = false;
            string remoteJson = null;
            FirebaseSaveManager.Instance.DownloadSave(json =>
            {
                remoteJson = json;
                downloadFinished = true;
            });

            timeoutAt = Time.realtimeSinceStartup + 10f;
            while (!downloadFinished && Time.realtimeSinceStartup < timeoutAt)
                yield return null;
            bool hasPendingChanges = SaveManager.Instance.RestoreSessionData(remoteJson);
            canvasController.ShowCanvas("MainMenu");

            if (hasPendingChanges)
                SaveManager.Instance.SyncLocalToFirebase();
        }
    }
}
