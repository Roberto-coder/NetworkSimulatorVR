using System.Collections;
using Modules.MainMenu;
using UnityEngine;
using Systems.Save;

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
        private bool retryInProgress;

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
            if (!sessionRestored && Systems.Save.NetworkAvailability.IsOffline)
                sessionRestored = SessionContext.RestorePersistedFirebaseSession();
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
            bool downloadSucceeded = false;
            string downloadError = null;
            FirebaseSaveManager.Instance.DownloadSaveWithStatus((success, json, detail) =>
            {
                remoteJson = json;
                downloadSucceeded = success;
                downloadError = detail;
                downloadFinished = true;
            });

            timeoutAt = Time.realtimeSinceStartup + 10f;
            while (!downloadFinished && Time.realtimeSinceStartup < timeoutAt)
                yield return null;
            SaveManager.Instance.RestoreSessionData(remoteJson);
            canvasController.ShowCanvas("MainMenu");
            if (!downloadSucceeded)
                SyncStatusDialog.ShowNotice("Copia local disponible", (downloadError ?? "Firebase tarda en responder.") + " Puedes cargar tus partidas locales sin Internet.", RetryDownload);


        }

        private void RetryDownload()
        {
            if (retryInProgress || !SessionContext.CanSyncToFirebase) return;
            retryInProgress = true;
            FirebaseSaveManager.Instance.DownloadSaveWithStatus((success, json, detail) =>
            {
                retryInProgress = false;
                if (success) SaveManager.Instance.RestoreSessionData(json);
                else SyncStatusDialog.ShowNotice("No se pudo consultar Firebase", detail + " Tu copia local se conserva.", RetryDownload);
            });
        }
    }
}
