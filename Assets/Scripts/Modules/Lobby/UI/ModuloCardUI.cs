using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Modules.Lobby.UI
{
    public class ModuloCardUI : MonoBehaviour
    {
        public TMP_Text tituloText;
        public TMP_Text descripcionText;
        public TMP_Text objetivosText;
        public Image imagen;
        public Button playButton;

        private string escena;

        public void Configurar(ModuloData data)
        {
            tituloText.text = data.titulo;
            descripcionText.text = data.descripcion;
            objetivosText.text = data.objetivos;
            imagen.sprite = data.imagen;
            escena = data.escena;

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(CargarEscena);
        }

        void CargarEscena()
        {
            SceneManager.LoadScene(escena);
        }
    }
}