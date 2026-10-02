using System.Linq;
using Modules.Module03_Diagnostics.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module03_Diagnostics.Editor
{
    // Autoría: controles persistentes, sin materiales instanciados ni jerarquías creadas en Play.
    public static class DiagnosticConsoleStyle
    {
        private const string Atlas = "Assets/Universal Stylized UI/Atlases/Complete_Stylized_UI_elements_";
        private static Sprite Sprite(string atlas, int index) => AssetDatabase.LoadAllAssetsAtPath(Atlas + atlas + ".png")
            .OfType<Sprite>().FirstOrDefault(s => s.name == "Complete_Stylized_UI_elements_" + atlas + "_" + index);

        private static void Place(Component component, float x, float y, float w, float h)
        {
            var r = (RectTransform)component.transform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h);
        }

        private static TMP_Text Label(Transform parent, string name)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            var label = obj.GetComponent<TMP_Text>();
            label.fontSize = 18; label.color = Color.white; label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private static void Style(Button button, string text, float x, float y, float width = 190)
        {
            Place(button, x, y, width, 56);
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null) { label.text = text; label.fontSize = 18; Place(label, 0, 0, width - 16, 44); }
            var image = button.GetComponent<Image>();
            image.sprite = Sprite("buttons", 37); image.color = Color.white; image.material = null;
            image.type = Image.Type.Simple;
            button.transition = Selectable.Transition.ColorTint;
        }

        private static void Arrow(Button button, bool previous, float y)
        {
            Place(button, previous ? -202 : 202, y, 60, 54);
            foreach (var text in button.GetComponentsInChildren<TMP_Text>(true)) text.gameObject.SetActive(false);
            var image = button.GetComponent<Image>();
            image.sprite = Sprite("icons_1", previous ? 15 : 14);
            image.color = Color.white; image.material = null; image.preserveAspect = true;
            button.transition = Selectable.Transition.ColorTint;
        }

        public static void Apply(DiagnosticScreenView view)
        {
            var admin = view.GetComponent<NetworkAdministrationView>();
            if (admin == null) return;
            ((RectTransform)view.transform).sizeDelta = new Vector2(1440, 880);
            Place(view.title, -70, 385, 1240, 48); view.title.fontSize = 28;
            var topology = view.GetComponentsInChildren<ScrollRect>(true).First(s => s.name == "Topology");
            Place(topology, -275, 20, 840, 560);
            var results = view.outputRect.GetComponentInParent<ScrollRect>();
            Place(results, 435, 10, 520, 620); results.horizontal = false;
            Place(view.animationStatus, -275, -285, 840, 30);
            // Compatible con los Canvas generados y con la jerarquía organizada a mano.
            var mapTitle = view.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "DescriptionText");
            if (mapTitle != null) { Place(mapTitle, -275, 326, 840, 38); mapTitle.text = "TOPOLOGÍA · Selecciona un destino"; }
            string[] names = { "Ping", "Consultar ARP", "IP / máscara / MAC", "Puertos del switch", "Bitácora" };
            float[] positions = { -600, -400, -190, 30, 250 };
            for (int i = 0; i < 5; i++) Style(view.commands[i], names[i], positions[i], -380);
            Style(view.commands[5], "Cerrar", 650, 385, 100);
            Style(admin.open, "Modificar IP", 480, -380, 210);
            Place(view.xRayButton, -275, -330, 260, 48);
            view.xRayLabel.transform.SetParent(view.xRayButton.transform, false);
            Place(view.xRayLabel, 35, 0, 190, 44);
            if (view.xRayIndicator == null)
            {
                var icon = new GameObject("XRay indicator", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(view.xRayButton.transform, false);
                view.xRayIndicator = icon.GetComponent<Image>();
            }
            Place(view.xRayIndicator, -98, 0, 76, 40);
            view.xRayIndicator.preserveAspect = true; view.xRayIndicator.raycastTarget = false;
            view.xRayOffSprite = Sprite("buttons", 44); view.xRayOnSprite = Sprite("buttons", 41); view.SetXRay(false);
            Place(admin.panel.transform, 435, 10, 520, 620);
            if (admin.previousPort == null) admin.previousPort = Object.Instantiate(admin.nextPort, admin.panel.transform);
            if (admin.previousAddress == null) admin.previousAddress = Object.Instantiate(admin.nextAddress, admin.panel.transform);
            admin.previousPort.name = "Previous port"; admin.previousAddress.name = "Previous address";
            Arrow(admin.previousPort, true, -92); Arrow(admin.nextPort, false, -92);
            Arrow(admin.previousAddress, true, -154); Arrow(admin.nextAddress, false, -154);
            if (admin.portLabel == null) admin.portLabel = Label(admin.panel.transform, "Port value");
            if (admin.addressLabel == null) admin.addressLabel = Label(admin.panel.transform, "Address value");
            Place(admin.portLabel, 0, -92, 330, 54); Place(admin.addressLabel, 0, -154, 330, 54);
            admin.portLabel.text = "PUERTO"; admin.addressLabel.text = "IP PROPUESTA";
            Style(admin.applyAddress, "Aplicar IP", 0, -218, 230);
            Style(admin.enablePort, "Habilitar puerto", -124, -218, 232);
            Style(admin.disablePort, "Deshabilitar puerto", 124, -218, 232);
            Style(admin.verify, "Comprobar reparación", -124, -280, 232);
            Style(admin.back, "Volver a consola", 124, -280, 232);
            if (admin.nextPrefix != null) admin.nextPrefix.gameObject.SetActive(false);
            if (admin.detailsRect == null)
            {
                var viewport = new GameObject("Administration details", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
                viewport.transform.SetParent(admin.panel.transform, false);
                Place(viewport.transform, 0, 115, 480, 330);
                viewport.GetComponent<Image>().color = new Color(.045f, .075f, .12f);
                var content = new GameObject("Content", typeof(RectTransform));
                content.transform.SetParent(viewport.transform, false);
                admin.detailsRect = (RectTransform)content.transform;
                admin.detailsRect.anchorMin = new Vector2(0, 1); admin.detailsRect.anchorMax = new Vector2(1, 1);
                admin.detailsRect.pivot = new Vector2(0, 1); admin.detailsRect.sizeDelta = new Vector2(0, 330);
                admin.details.transform.SetParent(content.transform, false);
                var r = admin.details.rectTransform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(12, 12); r.offsetMax = new Vector2(-12, -12);
                var scroll = viewport.GetComponent<ScrollRect>(); scroll.viewport = (RectTransform)viewport.transform;
                scroll.content = admin.detailsRect; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            }
            admin.details.fontSize = 18;
        }
    }
}
