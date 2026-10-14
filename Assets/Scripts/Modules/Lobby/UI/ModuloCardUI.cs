using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Systems.Scenes;

namespace Modules.Lobby.UI
{
    public class ModuloCardUI : MonoBehaviour
    {
        public TMP_Text tituloText;
        public TMP_Text descripcionText;
        public TMP_Text objetivosText;
        public Image imagen;
        public Button playButton;
        [SerializeField] private TMP_Text playLabel;
        private string escena;
        private UnityAction repeatTutorial;
        private bool isTutorial;
        private ModuloData currentData;

        public void Configurar(ModuloData data, UnityAction onRepeatTutorial = null)
        {
            if (data == null) { playButton.interactable = false; return; }
            currentData = data;
            tituloText.text = data.titulo?.Trim();
            descripcionText.text = data.descripcion?.Trim();
            objetivosText.text = "<b>OBJETIVOS DE LA PRÁCTICA</b>\n\n" + data.objetivos?.Trim();
            imagen.sprite = data.imagen;
            imagen.preserveAspect = true;
            imagen.enabled = data.imagen != null;
            escena = data.escena?.Trim();
            isTutorial = data.repetirTutorial;
            repeatTutorial = onRepeatTutorial;
            if (playLabel == null) playLabel = playButton.GetComponentInChildren<TMP_Text>(true);
            bool available = isTutorial ? repeatTutorial != null :
                !string.IsNullOrWhiteSpace(escena) && Application.CanStreamedLevelBeLoaded(escena);
            bool unlocked = ModuleAccessPolicy.CanEnterScene(SaveManager.Instance?.ActiveProgress, escena,
                Systems.Auth.SessionContext.IsDebugSession);
            available &= unlocked;
            playButton.interactable = available;
            if (playLabel != null) playLabel.text = available
                ? (isTutorial ? "Repetir tutorial" : "Comenzar módulo") : unlocked ? "Próximamente" : "Módulo bloqueado";
            playButton.onClick.RemoveListener(CargarEscena);
            playButton.onClick.AddListener(CargarEscena);
        }

        private void CargarEscena()
        {
            if (!playButton.interactable || SceneTransitionManager.IsLoading) return;
            if (isTutorial) { repeatTutorial?.Invoke(); return; }
            if (!ModuleAccessPolicy.CanEnterScene(SaveManager.Instance?.ActiveProgress, escena,
                Systems.Auth.SessionContext.IsDebugSession)) return;
            if (!string.IsNullOrWhiteSpace(escena) && Application.CanStreamedLevelBeLoaded(escena))
                SceneTransitionManager.LoadScene(escena);
        }

        private void OnDestroy() { if (playButton != null) playButton.onClick.RemoveListener(CargarEscena); }

        private void OnEnable()
        {
            if (SaveManager.Instance != null) SaveManager.Instance.StatusChanged += RefreshAvailability;
            RefreshAvailability();
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null) SaveManager.Instance.StatusChanged -= RefreshAvailability;
        }

        private void RefreshAvailability()
        {
            if (currentData != null) Configurar(currentData, repeatTutorial);
        }
    }
}
