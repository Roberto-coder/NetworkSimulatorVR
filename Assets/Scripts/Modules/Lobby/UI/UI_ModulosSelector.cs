using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Modules.Lobby.Presentation;

namespace Modules.Lobby.UI
{
    public class UI_ModulosSelector : MonoBehaviour
    {
        [System.Serializable]
        public class ModuloEntry
        {
            public Toggle toggle;
            public ModuloData data;
        }
        [SerializeField] private List<ModuloEntry> modulos;
        [SerializeField] private GameObject moduleCardPrefab;
        [SerializeField] private Transform displayParent;
        [SerializeField] private LobbyPresentationController lobbyPresentation;
        private ModuloCardUI currentCard;
        private readonly List<(Toggle toggle, UnityAction<bool> action)> subscriptions = new();

        private void Start()
        {
            if (moduleCardPrefab == null || displayParent == null || modulos == null)
            {
                Debug.LogError("MainPanel requiere prefab, contenedor y módulos.", this);
                return;
            }
            if (lobbyPresentation == null)
                lobbyPresentation = FindFirstObjectByType<LobbyPresentationController>();
            ModuloEntry selected = null;
            foreach (var entry in modulos)
            {
                if (entry == null || entry.toggle == null || entry.data == null) continue;
                var data = entry.data;
                UnityAction<bool> action = isOn => { if (isOn) MostrarModulo(data); };
                entry.toggle.onValueChanged.AddListener(action);
                subscriptions.Add((entry.toggle, action));
                if (selected == null || entry.toggle.isOn) selected = entry;
            }
            if (selected != null)
            {
                selected.toggle.SetIsOnWithoutNotify(true);
                MostrarModulo(selected.data);
            }
        }

        private void MostrarModulo(ModuloData data)
        {
            if (currentCard == null)
            {
                var instance = Instantiate(moduleCardPrefab, displayParent, false);
                currentCard = instance.GetComponent<ModuloCardUI>();
                if (currentCard == null)
                {
                    Debug.LogError("ModuleCard necesita ModuloCardUI en su raíz.", instance);
                    Destroy(instance);
                    return;
                }
                var rect = instance.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }
            currentCard.Configurar(data, lobbyPresentation != null
                ? new UnityAction(lobbyPresentation.RepeatTutorial) : null);
        }

        private void OnDestroy()
        {
            foreach (var subscription in subscriptions)
                if (subscription.toggle != null) subscription.toggle.onValueChanged.RemoveListener(subscription.action);
        }
    }
}
