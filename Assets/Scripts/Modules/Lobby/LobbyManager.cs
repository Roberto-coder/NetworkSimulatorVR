using GameData.Modules;
using Modules.Lobby.Flow;
using Systems.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using Systems.Scenes;

namespace Modules.Lobby
{
    public sealed class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        [SerializeField] private ModuleDefinition moduleDefinition;

        public LobbyFlowController FlowController { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (!SessionContext.IsDebugSession)
                SessionContext.RestorePersistedFirebaseSession();
            if (moduleDefinition == null)
            {
                Debug.LogError("LobbyManager necesita un ModuleDefinition.", this);
                return;
            }
            FlowController = new LobbyFlowController(moduleDefinition);
        }

        private void Start()
        {
            if (!SessionContext.IsAuthenticated)
            {
                Debug.LogWarning("Lobby requiere una sesión activa; regresando al Login.", this);
                SceneTransitionManager.LoadScene("Menu");
                return;
            }

            SaveManager.Instance?.LoadFromLocal();
            FlowController?.Begin();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
