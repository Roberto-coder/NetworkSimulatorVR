using UnityEngine;

namespace GameData.Objectives
{
    [CreateAssetMenu(fileName = "ObjectiveData", menuName = "Scriptable Objects/ObjectiveData")]
    public class ObjectiveData : ScriptableObject
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string title;

        [SerializeField]
        private string description;

        [SerializeField]
        private string introductionDialogue;

        [SerializeField]
        private string reminderDialogue;

        [SerializeField]
        private float reminderInterval = 20f;

        /// <summary>
        /// Obtiene el identificador unico del objetivo.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Obtiene el titulo mostrado para el objetivo.
        /// </summary>
        public string Title => title;

        /// <summary>
        /// Obtiene la descripcion del objetivo.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Obtiene el dialogo presentado al iniciar el objetivo.
        /// </summary>
        public string IntroductionDialogue => introductionDialogue;

        /// <summary>
        /// Obtiene el dialogo utilizado como recordatorio.
        /// </summary>
        public string ReminderDialogue => reminderDialogue;

        /// <summary>
        /// Obtiene el intervalo en segundos entre recordatorios.
        /// </summary>
        public float ReminderInterval => reminderInterval;

    }
}
