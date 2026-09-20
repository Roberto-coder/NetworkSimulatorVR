using UnityEngine;

namespace Systems.Settings
{
    /// <summary>Referencia al mismo prefab de audio para cualquier escena inicial.</summary>
    [CreateAssetMenu(fileName = "GlobalAudioBootstrap", menuName = "Settings/Global Audio Bootstrap")]
    public sealed class GlobalAudioBootstrap : ScriptableObject
    {
        [SerializeField] private GameObject globalSystemsPrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (GlobalSettingsManager.Instance != null)
                return;

            var configuration = Resources.Load<GlobalAudioBootstrap>("GlobalAudioBootstrap");
            if (configuration == null || configuration.globalSystemsPrefab == null)
            {
                Debug.LogError("Falta Resources/GlobalAudioBootstrap o su prefab GlobalSystems.");
                return;
            }

            var root = Instantiate(configuration.globalSystemsPrefab);
            root.name = "GlobalSystems";
        }
    }
}
