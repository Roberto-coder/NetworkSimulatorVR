using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>Interfaz de selección y destino; no necesita arrastrar bloques pequeños en VR.</summary>
    public sealed class TerminalPuzzleView : MonoBehaviour
    {
        [SerializeField] private ConsoleTerminalTool terminal;
        [SerializeField] private RectTransform screen;
        private Module02SwitchConfiguration configuration;
        private readonly Button[] sourceButtons = new Button[4], slotButtons = new Button[4];
        private readonly TMP_Text[] slotTexts = new TMP_Text[4];
        private static readonly string[] Names = { "Acceder a configuración", "Asignar IP de gestión", "Definir gateway", "Habilitar puertos" };
        private static readonly string[] Details = { "modo de configuración", "192.168.10.2 /24", "192.168.10.1", "Gi01–Gi04: habilitar" };
        private static readonly Color Green = new(0.25f, 1f, 0.5f);
        private static readonly Color Dark = new(0.025f, 0.07f, 0.045f);
        private GameObject panel;
        private TMP_Text hint;
        private Button apply;
        private int selected = -1;
        private bool lastReady;

        private void Awake()
        {
            if (terminal == null) terminal = GetComponent<ConsoleTerminalTool>();
            if (screen == null) { enabled = false; return; }
            // El raycaster XRI permite usar el mismo rayo de UI del resto del módulo.
            if (screen.GetComponent<TrackedDeviceGraphicRaycaster>() == null) screen.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            panel = Rect("Puzzle", screen, Vector2.zero, new Vector2(520, 420)).gameObject;
            Text("Title", panel.transform, new Vector2(0, 118), new Vector2(490, 32), "SW1 > CONFIGURACIÓN GUIADA", 21);
            Text("Instructions", panel.transform, new Vector2(0, 88), new Vector2(490, 28), "Selecciona un bloque y después su destino", 16);
            // Orden inicial deliberadamente distinto a la solución; los IDs nunca dependen del texto.
            int[] order = { 2, 0, 3, 1 };
            for (int i = 0; i < 4; i++)
            {
                int block = order[i], slot = i;
                sourceButtons[block] = MakeButton(panel.transform, new Vector2(-128, 43 - i * 51), new Vector2(240, 44),
                    Names[block] + "\n" + Details[block], () => { selected = block; Refresh(); });
                slotButtons[i] = MakeButton(panel.transform, new Vector2(128, 43 - i * 51), new Vector2(240, 44), "", () => Assign(slot));
                slotTexts[i] = slotButtons[i].GetComponentInChildren<TMP_Text>();
            }
            apply = MakeButton(panel.transform, new Vector2(0, -157), new Vector2(490, 37), "[ APLICAR CONFIGURACIÓN ]", Apply);
            hint = Text("Hint", panel.transform, new Vector2(0, -192), new Vector2(490, 28), "", 15);
            panel.SetActive(false);
        }
        private void LateUpdate()
        {
            var current = terminal.Power != null ? terminal.Power.GetComponent<Module02SwitchConfiguration>() : null;
            if (configuration != current)
            {
                if (configuration != null) configuration.ConfigurationChanged -= Changed;
                configuration = current;
                if (configuration != null) configuration.ConfigurationChanged += Changed;
                selected = -1; Refresh();
            }
            if (lastReady != terminal.IsReady) { lastReady = terminal.IsReady; Refresh(); }
        }
        private void OnDisable()
        {
            if (configuration != null) configuration.ConfigurationChanged -= Changed;
            configuration = null;
        }
        private void Changed() { selected = -1; Refresh(); }
        private void Assign(int slot)
        {
            if (selected < 0 || configuration == null) return;
            configuration.Place(terminal, selected, slot);
        }
        private void Apply()
        {
            if (configuration == null || !terminal.IsReady) return;
            bool success = configuration.Apply(terminal);
            hint.text = success ? "> Configuración aplicada. Interfaces habilitadas." :
                "> Revisa el paso " + (configuration.Puzzle.FirstIncorrectSlot() + 1) + ": " +
                Names[Mathf.Clamp(configuration.Puzzle.FirstIncorrectSlot(), 0, 3)];
        }
        private void Refresh()
        {
            if (panel == null) return;
            bool ready = terminal.IsReady && configuration != null;
            panel.SetActive(ready);
            if (!ready) return;
            bool solved = configuration.IsConfigured;
            for (int i = 0; i < 4; i++)
            {
                sourceButtons[i].interactable = !solved;
                sourceButtons[i].GetComponent<Image>().color = selected == i ? new Color(0.05f, 0.25f, 0.12f) : Dark;
                slotButtons[i].interactable = !solved && selected >= 0;
                int block = configuration.Puzzle.GetBlock(i);
                slotTexts[i].text = (i + 1) + ". " + (block < 0 ? "[ seleccionar destino ]" : Names[block]);
            }
            apply.interactable = !solved;
            hint.text = solved ? "> Configuración aplicada. Interfaces habilitadas." : "> Esperando secuencia…";
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static TMP_Text Text(string name, Transform parent, Vector2 position, Vector2 size, string value, float fontSize)
        {
            var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value; text.color = Green; text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
        }
        private static Button MakeButton(Transform parent, Vector2 position, Vector2 size, string label, UnityEngine.Events.UnityAction callback)
        {
            var rect = Rect("Block", parent, position, size);
            var image = rect.gameObject.AddComponent<Image>(); image.color = Dark;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(0.65f, 1f, 0.7f); colors.pressedColor = Green;
            colors.disabledColor = new Color(0.5f, 0.6f, 0.5f); button.colors = colors;
            Text("Text", rect, Vector2.zero, size - new Vector2(10, 4), label, 16);
            button.onClick.AddListener(callback); return button;
        }
    }
}
