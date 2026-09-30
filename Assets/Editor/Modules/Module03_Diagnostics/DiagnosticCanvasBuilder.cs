using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Modules.Module03_Diagnostics.Editor
{
    // Solo se ejecuta desde el configurador del Editor. La jerarquÃ­a resultante se guarda en escena.
    public static class DiagnosticCanvasBuilder
    {
        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var obj = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        private static TMP_Text Text(Transform parent, string text, Vector2 size, Vector2 position, int fontSize)
        {
            var rect = Rect("Text", parent, size, position); var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text; label.fontSize = fontSize; label.color = Color.white; label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Center; return label;
        }
        private static Button Button(Transform parent, string text, Vector2 size, Vector2 position)
        {
            var rect = Rect(text, parent, size, position); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.12f, 0.23f, 0.34f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text(rect, text, size - new Vector2(8, 6), Vector2.zero, 20); return button;
        }
        private static RectTransform Scroll(Transform parent, string name, Vector2 size, Vector2 position, Vector2 contentSize)
        {
            var rect = Rect(name, parent, size, position); rect.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.055f, 0.08f);
            rect.gameObject.AddComponent<RectMask2D>(); var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var content = Rect("Content", rect, contentSize, Vector2.zero); content.anchorMin = content.anchorMax = new Vector2(0, 1); content.pivot = new Vector2(0, 1);
            scroll.viewport = rect; scroll.content = content; scroll.movementType = ScrollRect.MovementType.Clamped;
            return content;
        }
        // AutorÃ­a Ãºnicamente: se aplica tambiÃ©n a Canvas/prefabs creados antes del sprint 5.
        public static void AddTraceControls(DiagnosticScreenView view)
        {
            // El contorno usa el fondo completo: incluye icono, nombre y estado.
            foreach (var node in view.nodes)
            {
                if (node.button == null || node.selectionOutline != null) continue;
                var background = node.button.targetGraphic;
                if (background == null) continue;
                node.selectionOutline = background.GetComponent<UnityEngine.UI.Outline>() ?? background.gameObject.AddComponent<UnityEngine.UI.Outline>();
                node.selectionOutline.effectColor = new Color(0.1f, 0.85f, 1);
                node.selectionOutline.effectDistance = new Vector2(3, -3);
                node.selectionOutline.enabled = false;
            }
            if (view.xRayButton == null)
            {
                view.xRayButton = Button(view.transform, "Rayos X: OFF", new Vector2(245, 48), new Vector2(130, -312));
                view.xRayLabel = view.xRayButton.GetComponentInChildren<TMP_Text>();
            }
            if (view.animationStatus == null)
            {
                view.animationStatus = Text(view.transform, "Traza de ping Â· velocidad didÃ¡ctica", new Vector2(1030, 30), new Vector2(0, -145), 15);
                view.animationStatus.name = "Trace status";
            }
        }
        public static void AddAdministrationControls(DiagnosticScreenView view)
        {
            var existing = view.GetComponent<NetworkAdministrationView>();
            if (existing != null)
            {
                if (existing.nextPrefix != null) existing.nextPrefix.gameObject.SetActive(false);
                if (existing.applyAddress != null) existing.applyAddress.GetComponentInChildren<TMP_Text>().text = "Aplicar IP";
                DiagnosticConsoleStyle.Apply(view);
                return;
            }
            var admin = view.gameObject.AddComponent<NetworkAdministrationView>();
            admin.open = Button(view.transform, "Administrar", new Vector2(245, 48), new Vector2(390, -312));
            var panel = Rect("Administration", view.transform, new Vector2(1040, 540), new Vector2(0, 35));
            panel.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.035f, 0.055f);
            admin.panel = panel.gameObject;
            admin.details = Text(panel, "Administración de red", new Vector2(1000, 330), new Vector2(0, 90), 18);
            admin.details.alignment = TextAlignmentOptions.TopLeft;
            admin.nextPort = Button(panel, "Siguiente puerto", new Vector2(310, 45), new Vector2(-330, -105));
            admin.nextAddress = Button(panel, "Siguiente IP", new Vector2(310, 45), new Vector2(0, -105));
            admin.applyAddress = Button(panel, "Aplicar IP", new Vector2(310, 45), new Vector2(-330, -165));
            admin.enablePort = Button(panel, "Habilitar puerto", new Vector2(310, 45), new Vector2(0, -165));
            admin.disablePort = Button(panel, "Deshabilitar puerto", new Vector2(310, 45), new Vector2(330, -165));
            admin.verify = Button(panel, "Verificar incidentes", new Vector2(310, 45), new Vector2(-165, -225));
            admin.back = Button(panel, "Volver al mapa", new Vector2(310, 45), new Vector2(165, -225));
            panel.gameObject.SetActive(false);
            DiagnosticConsoleStyle.Apply(view);
        }

        public static DiagnosticScreenView Build(Transform parent, Module03NetworkScene networkScene, DiagnosticUiSettings settings, Camera playerCamera)
        {
            var root = Rect("M03 Diagnostic Screen", parent, new Vector2(1100, 730), Vector2.zero);
            var panel = root.gameObject; panel.layer = 5; var view = panel.AddComponent<DiagnosticScreenView>();
            root.localScale = Vector3.one * settings.panelScale;
            var canvas = panel.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = playerCamera;
            panel.AddComponent<TrackedDeviceGraphicRaycaster>(); panel.AddComponent<GraphicRaycaster>();
            panel.AddComponent<Image>().color = new Color(0.025f, 0.035f, 0.055f);
            view.title = Text(root, "", new Vector2(1060, 45), new Vector2(0, 320), 24);
            Text(root, settings.mapTitle, new Vector2(580, 40), new Vector2(-230, 265), 20).name = "DescriptionText";
            var n = networkScene.initialState.CopyDefinition();
            int columns = Mathf.Max(3, Mathf.CeilToInt(Mathf.Sqrt(n.devices.Count)));
            float mapWidth = Mathf.Max(550, columns * 180), mapHeight = Mathf.Max(420, Mathf.Ceil(n.devices.Count / (float)columns) * 110);
            var map = Scroll(root, "Topology", new Vector2(1040, 380), new Vector2(0, 45), new Vector2(mapWidth, mapHeight));
            var positions = new Dictionary<string, Vector2>();
            bool authored = n.devices.All(d => networkScene.layout.devices.Any(p => p.deviceId == d.id)) &&
                networkScene.layout.devices.Select(p => p.mapPosition).Distinct().Count() == networkScene.layout.devices.Count;
            var poses = networkScene.layout.devices;
            float minX = authored ? poses.Min(p => p.mapPosition.x) : 0, minY = authored ? poses.Min(p => p.mapPosition.y) : 0;
            float rangeX = authored ? Mathf.Max(1, poses.Max(p => p.mapPosition.x) - minX) : 1;
            float rangeY = authored ? Mathf.Max(1, poses.Max(p => p.mapPosition.y) - minY) : 1;
            for (int i = 0; i < n.devices.Count; i++)
            {
                var pose = poses.FirstOrDefault(p => p.deviceId == n.devices[i].id);
                positions[n.devices[i].id] = authored ? new Vector2(85 + (pose.mapPosition.x - minX) / rangeX * (mapWidth - 170),
                    -55 - (pose.mapPosition.y - minY) / rangeY * (mapHeight - 110)) : new Vector2(85 + i % columns * 180, -55 - i / columns * 110);
            }
            // Las posiciones del asset UI prevalecen sobre la distribuciÃ³n automÃ¡tica.
            foreach (var node in settings.mapNodes)
                if (positions.ContainsKey(node.deviceId)) positions[node.deviceId] = node.position;
            map.sizeDelta = new Vector2(Mathf.Max(mapWidth, positions.Values.Max(p => p.x) + 85),
                Mathf.Max(mapHeight, -positions.Values.Min(p => p.y) + 85));
            var ports = n.ports.ToDictionary(p => p.id);
            var drawnLinks = new HashSet<string>();
            foreach (var cable in n.cables.Where(c => !string.IsNullOrEmpty(c.portA) && !string.IsNullOrEmpty(c.portB)))
            {
                string deviceA = ports[cable.portA].deviceId, deviceB = ports[cable.portB].deviceId;
                // Varios cables entre switch y patch panel comparten un enlace visual abstracto.
                string key = string.CompareOrdinal(deviceA, deviceB) < 0 ? deviceA + "|" + deviceB : deviceB + "|" + deviceA;
                if (!drawnLinks.Add(key)) continue;
                var a = positions[deviceA]; var b = positions[deviceB];
                var line = Rect("Documented link", map, new Vector2(Vector2.Distance(a, b), 3), (a + b) / 2);
                line.anchorMin = line.anchorMax = new Vector2(0, 1); line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                var image = line.gameObject.AddComponent<Image>(); image.color = Color.gray; image.raycastTarget = false;
            }
            foreach (var d in n.devices)
            {
                string id = d.id;
                var data = settings.mapNodes.FirstOrDefault(node => node.deviceId == id);
                var rect = Rect(id, map, new Vector2(120, 110), positions[id]);
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                var background = rect.gameObject.AddComponent<Image>(); background.color = new Color(0.035f, 0.055f, 0.08f);
                var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = background;
                var iconRect = Rect("Icon", rect, new Vector2(112, 60), new Vector2(0, 20));
                var icon = iconRect.gameObject.AddComponent<Image>(); icon.sprite = data?.sprite;
                icon.preserveAspect = true; icon.raycastTarget = false; icon.enabled = icon.sprite != null;
                var label = Text(rect, string.IsNullOrEmpty(data?.displayName) ? id : data.displayName,
                    new Vector2(120, 24), new Vector2(0, -24), 16);
                label.name = "Device name";
                var status = Text(rect, "Sin comprobar", new Vector2(120, 24), new Vector2(0, -46), 12);
                status.name = "Observation";
                view.nodes.Add(new DiagnosticNodeBinding { deviceId = id, button = button, label = label, status = status });
            }
            var outputContent = Scroll(root, "Results", new Vector2(1040, 80), new Vector2(0, -195), new Vector2(1020, 440));
            var output = Text(outputContent, "", new Vector2(1020, 440), Vector2.zero, 20);
            output.alignment = TextAlignmentOptions.TopLeft; view.outputRect = outputContent; view.output = output;
            var textRect = output.rectTransform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10); textRect.offsetMax = new Vector2(-10, -10);
            outputContent.anchorMin = new Vector2(0, 1); outputContent.anchorMax = new Vector2(1, 1); outputContent.sizeDelta = new Vector2(0, 440);
            view.commands = new Button[6]; for (int i = 0; i < 6; i++)
            {
                int command = i;
                view.commands[i] = Button(root, settings.commands[i], new Vector2(245, 48), new Vector2(-390 + i % 4 * 260, -250 - i / 4 * 62));
            }
            AddTraceControls(view);
            AddAdministrationControls(view);
            panel.SetActive(false); return view;
        }

    }
}
