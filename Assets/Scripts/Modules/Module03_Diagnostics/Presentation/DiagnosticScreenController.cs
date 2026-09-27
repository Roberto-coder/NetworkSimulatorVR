using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using UnityEngine;
using UnityEngine.Events;
using Systems.Input;
using Presentacion.GlobalUI.RadialSelectorTool;
using UnityEngine.UI;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Activa vistas fijas existentes. No construye ni reposiciona Canvas en Play.</summary>
    public sealed class DiagnosticScreenController : MonoBehaviour
    {
        public event Action<ProbeResult, DiagnosticScreenView> ProbeExecuted;
        public event Action ScreenClosed;
        public event Action CommandExecuting;
        public event Action XRayRequested;
        private bool xRayEnabled;
        public void SetXRayIndicator(bool value)
        {
            xRayEnabled = value;
            if (targets == null) return;
            foreach (var target in targets)
                if (target.screen != null && target.screen.xRayLabel != null)
                    target.screen.xRayLabel.text = value ? "Rayos X: ON" : "Rayos X: OFF";
        }
        public Module03NetworkScene networkScene;
        public DiagnosticUiSettings settings;
        public Camera playerCamera;
        public Transform rightAim;
        public GameObject radialCanvas;
        public LayerMask interactionMask = ~0;
        public GameObject[] modalBlockers = Array.Empty<GameObject>();
        [Header("Diagnóstico de interacción (Play)")]
        [SerializeField, TextArea(2, 4)] private string interactionStatus = "Esperando inicialización";
        public bool drawInteractionRay;
        private RadialMenuController radialMenu;
        private bool InteractPressed => VRInputManager.Instance != null && VRInputManager.Instance.InteractPressed;
        private DiagnosticWorkspace workspace;
        private DiagnosticInteractionTarget currentTarget;
        private DiagnosticInteractionTarget[] targets;
        private readonly List<(Button button, UnityAction callback)> listeners = new();
        private long shownRevision = -1;
        private string selected;
        private DiagnosticScreenView View => currentTarget != null ? currentTarget.screen : null;
        public bool IsOpen => View != null && View.gameObject.activeSelf;
        private bool Blocked => Time.timeScale <= 0 || (radialMenu != null ? radialMenu.IsOpen : radialCanvas != null && radialCanvas.activeInHierarchy) ||
            modalBlockers.Any(g => g != null && g.activeInHierarchy);

        private void Awake()
        {
            if (settings == null) { interactionStatus = "Falta DiagnosticUiSettings"; enabled = false; return; }
            // RadialSelection permanece activo para escuchar Y; sólo la rueda abierta bloquea.
            if (radialCanvas != null)
            {
                radialMenu = radialCanvas.GetComponentInParent<RadialMenuController>();
                if (radialMenu == null) radialMenu = radialCanvas.GetComponentInChildren<RadialMenuController>(true);
            }
        }
        private void OnDisable() { Close(); HideHints(); }
        private void OnDestroy()
        {
            foreach (var pair in listeners) if (pair.button != null) pair.button.onClick.RemoveListener(pair.callback);
        }
        private void Start()
        {
            if (VRInputManager.Instance == null)
                Debug.LogError("M3: falta VRInputManager activo para recibir A (Interact).", this);
            if (networkScene == null || networkScene.Session == null || playerCamera == null || rightAim == null)
            {
                // Reportar la dependencia exacta evita confundir una sesión fallida con el puntero.
                interactionStatus = networkScene == null ? "Falta Network Scene." :
                    networkScene.Session == null ? "La sesión de red no se creó. Revisar el error de Module03NetworkScene en Console." :
                    playerCamera == null ? "Falta Player Camera." : "Falta Right Aim.";
                Debug.LogError("M3: " + interactionStatus, this); enabled = false; return;
            }
            targets = networkScene.GetComponentsInChildren<DiagnosticInteractionTarget>(true);
            var definition = networkScene.initialState.CopyDefinition();
            // Compatibilidad con escenas ya generadas: ocultar las vistas antiguas, sin construir ni borrar UI.
            foreach (var target in targets)
            {
                if (target.screen != null) target.screen.gameObject.SetActive(false);
                if (target.focusHint != null) target.focusHint.gameObject.SetActive(false);
            }
            targets = targets.Where(t => t.isActiveAndEnabled && DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)).ToArray();
            var used = new HashSet<DiagnosticScreenView>();
            foreach (var target in targets)
            {
                var view = target.screen;
                if (view == null || !used.Add(view) || view.title == null || view.output == null || view.outputRect == null ||
                    view.commands == null || view.commands.Length != 6 || view.commands.Any(b => b == null) ||
                    view.nodes == null || view.nodes.Any(n => n == null || n.button == null || n.label == null))
                { interactionStatus = $"Vista incompleta o compartida: {target.name}. Revisar Console."; Debug.LogError("M3: cada dispositivo necesita una vista fija completa y exclusiva.", target); enabled = false; return; }
            }
            foreach (var target in targets)
            {
                var view = target.screen;
                view.gameObject.SetActive(false);
                if (view.xRayButton != null) Bind(view.xRayButton, () => { if (IsOpen && View == view && !Blocked) XRayRequested?.Invoke(); });
                for (int i = 0; i < view.commands.Length; i++)
                { int command = i; Bind(view.commands[i], () => Command(view, command)); }
                foreach (var node in view.nodes)
                { string id = node.deviceId; Bind(node.button, () => SelectNode(view, id)); }
            }
            workspace = new DiagnosticWorkspace(networkScene.Session, networkScene.Diagnostics, networkScene.initialState.CopyDefinition(), settings.sourcePortId);
            HideHints();
        }
        private void Bind(Button button, UnityAction callback)
        { button.onClick.AddListener(callback); listeners.Add((button, callback)); }
        private void HideHints()
        {
            if (targets == null) return;
            foreach (var target in targets) if (target.focusHint != null) target.focusHint.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (workspace == null) { interactionStatus = "Sin workspace: revisar errores de inicialización en Console."; return; }
            HideHints();
            if (Blocked)
            {
                interactionStatus = Time.timeScale <= 0 ? "Bloqueado: pausa" :
                    modalBlockers.Any(g => g != null && g.activeInHierarchy) ? "Bloqueado: Modal Blockers activo" : "Bloqueado: rueda/Radial Canvas activo";
                Close(); return;
            }
            if (IsOpen)
            {
                interactionStatus = $"Canvas abierto: {currentTarget.DeviceId}";
                if (!currentTarget.isActiveAndEnabled || Vector3.Distance(playerCamera.transform.position, currentTarget.InteractionPosition) > settings.interactionRange || InteractPressed)
                { Close(); return; }
                if (shownRevision != networkScene.Session.Revision)
                { RefreshNodes(); View.ShowOutput("La red cambió. Repite la consulta; las observaciones anteriores están obsoletas."); }
                return;
            }
            // Este rayo usa el eje Z de Right Aim; el dibujo permite compararlo con el puntero XRI.
            if (drawInteractionRay)
                Debug.DrawRay(rightAim.position, rightAim.forward * settings.interactionRange, Color.cyan);
            if (!Physics.Raycast(rightAim.position, rightAim.forward, out var hit, settings.interactionRange, interactionMask, QueryTriggerInteraction.Collide))
            { interactionStatus = "Sin impacto: revisar Right Aim, alcance, colliders e Interaction Mask."; return; }
            var target = hit.collider.GetComponentInParent<DiagnosticInteractionTarget>();
            if (target == null)
            { interactionStatus = $"El primer impacto es {hit.collider.name}, sin DiagnosticInteractionTarget en sus padres."; return; }
            if (!targets.Contains(target) || target.device == null || target.screen == null || !target.isActiveAndEnabled ||
                !target.device.transform.IsChildOf(networkScene.transform))
            { interactionStatus = $"Target no habilitado/registrado o referencias inválidas: {target.name}"; return; }
            float distance = Vector3.Distance(playerCamera.transform.position, target.InteractionPosition);
            if (distance > settings.interactionRange)
            { interactionStatus = $"{target.DeviceId}: cámara a {distance:F2} m; máximo {settings.interactionRange:F2} m."; return; }
            if (target.focusHint != null)
            {
                target.focusHint.gameObject.SetActive(true);
                interactionStatus = target.focusHint.gameObject.activeInHierarchy
                    ? $"Hint activado: {target.DeviceId}. Esperando A. Si no es visible, revisar posición/orientación/render del texto."
                    : $"Hint activado pero un padre está desactivado: {target.focusHint.name}";
            }
            else interactionStatus = $"Falta Focus Hint: {target.DeviceId}";
            if (InteractPressed) Open(target);
        }
        private void Open(DiagnosticInteractionTarget target)
        {
            currentTarget = target; workspace.OpenLocal(target.DeviceId);
            // Solo visibilidad y datos: posición, escala y orientación son las guardadas en escena.
            View.gameObject.SetActive(true);
            SetXRayIndicator(xRayEnabled);
            View.title.text = $"{target.DeviceId} · contexto local | Ping desde {settings.sourcePortId}";
            HideHints(); RefreshNodes();
            View.ShowOutput(target.DeviceId == settings.laptopDeviceId ? settings.mapTitle : workspace.LocalConfiguration());
        }
        public void Close()
        { if (View != null) { View.gameObject.SetActive(false); ScreenClosed?.Invoke(); } currentTarget = null; }
        private void SelectNode(DiagnosticScreenView view, string id)
        {
            if (view != View || !IsOpen || Blocked) return;
            selected = id; workspace.SelectDestination(id); RefreshNodes();
            view.ShowOutput(id + "\nIP documentada: " + (workspace.ExpectedAddress(id) ?? "No aplica (pasivo)") + "\n" + workspace.Observation(id));
        }
        private void Command(DiagnosticScreenView view, int index)
        {
            if (view != View || !IsOpen || Blocked) return;
            CommandExecuting?.Invoke();
            try
            {
                switch (index)
                {
                    case 0:
                        var result = workspace.PingSelected();
                        view.ShowOutput($"ping {result.DestinationIp}\nOrigen: {result.SourcePort}\n{result.Status} · {result.Received}/{result.Sent}\n{result.Message}\nRespondió: {result.ResponderPort}\nRevisión: {result.Revision}");
                        RefreshNodes(); ProbeExecuted?.Invoke(result, view); break;
                    case 1: view.ShowOutput("arp -a · " + settings.sourcePortId + "\n" + workspace.Neighbours()); break;
                    case 2: view.ShowOutput(workspace.LocalConfiguration()); break;
                    case 3: view.ShowOutput(workspace.SwitchPorts(selected)); break;
                    case 4: view.ShowOutput(workspace.History()); break;
                    case 5: Close(); break;
                }
            }
            catch (Exception e) { view.ShowOutput(e.Message); }
        }
        private void RefreshNodes()
        {
            shownRevision = networkScene.Session.Revision;
            foreach (var node in View.nodes)
            {
                // Compatibilidad con Canvas anteriores que sólo tenían un texto.
                var statusLabel = node.status != null ? node.status : node.label;
                statusLabel.text = (node.status == null ? node.deviceId + "\n" : "") + workspace.Observation(node.deviceId);
                if (node.selectionOutline != null) node.selectionOutline.enabled = selected == node.deviceId;
            }
        }
    }
}
