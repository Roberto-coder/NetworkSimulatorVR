using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        private GameObject currentCard;

        void Start()
        {
            ConfigurarToggles();
        }

        void ConfigurarToggles()
        {
            foreach (var modulo in modulos)
            {
                modulo.toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                        MostrarModulo(modulo.data);
                });
            }
        }

        void MostrarModulo(ModuloData data)
        {
            if (currentCard != null) Destroy(currentCard);

            currentCard = Instantiate(moduleCardPrefab, displayParent);
    
            // Forzar posición local para evitar que aparezca "detrás" del canvas
            currentCard.transform.localPosition = Vector3.zero;
            currentCard.transform.localRotation = Quaternion.identity;
            currentCard.transform.localScale = Vector3.one;

            ModuloCardUI cardUI = currentCard.GetComponent<ModuloCardUI>();
            cardUI.Configurar(data);
        }
    }
}