using UnityEngine;

namespace GameData.Achievements
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "Game Data/Achievement")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        [SerializeField] private string achievementId;
        [SerializeField] private string title;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        public string AchievementId => achievementId;
        public string Title => title;
        public string Description => description;
        public Sprite Icon => icon;
    }
}
