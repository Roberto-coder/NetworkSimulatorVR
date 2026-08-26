using UnityEngine;

namespace Systems.Scenes
{
    [CreateAssetMenu(fileName = "SceneTransitionSettings", menuName = "Network Simulator/Scene Transition Settings")]
    public sealed class SceneTransitionSettings : ScriptableObject
    {
        [SerializeField] private GameObject loadingCanvasPrefab;

        public GameObject LoadingCanvasPrefab => loadingCanvasPrefab;
    }
}
