using TMPro;
using UnityEngine;

namespace Modules.Lobby.Tutorial
{
    public class WorldSpaceUI : MonoBehaviour
    {
        public TextMeshProUGUI dialogText;
        public Canvas canvas;
        public Transform player;

        private void Update()
        {
            transform.LookAt(player);
        }

        public void Show(string text)
        {
            dialogText.text = text;
            canvas.enabled = true;
        }

        public void Hide()
        {
            canvas.enabled = false;
        }
    }
}