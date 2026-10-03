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
        public event Action<string> ScreenOpened;
        public event Action<int> CommandInvoked;
        public event Action CommandExecuting;
        public event Action XRayRequested;
        private bool xRayEnabled;
        public void SetXRayIndicator(bool value)
        {
            xRayEnabled = value;
            if (targets == null) return;
            foreach (var target in targets)
                if (target.screen != null) target.screen.SetXRay(value);
        }
        public Module03NetworkScene networkScene;
        public DiagnosticUiSettings settings;
        public NetworkAdministrationSettings administration;
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
            CollectTargets();
            if (!ValidateViews()) return;
            BindViews();
            workspace = new DiagnosticWorkspace(networkScene.Session, networkScene.Diagnostics, administration != null && administration.targetRules != null ? administration.targetRules.DocumentedInventory(networkScene.initialState) : networkScene.initialState.CopyDefinition(), settings.sourcePortId);
            HideHints();
        }

        private void CollectTargets()
        {
            targets = networkScene.GetComponentsInChildren<DiagnosticInteractionTarget>(true);
            var definition = networkScene.initialState.CopyDefinition();
            // Compatibilidad con escenas ya generadas: ocultar las vistas antiguas, sin construir ni borrar UI.
            foreach (var target in targets)
            {
                if (target.screen != null) target.screen.gameObject.SetActive(false);
                if (target.focusHint != null) target.FocusHintRoot.SetActive(false);
            }
            targets = targets.Where(t => t.isActiveAndEnabled && DiagnosticWorkspace.HasLocalScreen(definition, t.DeviceId)).ToArray();
        }

        private bool ValidateViews()
        {
            var used = new HashSet<DiagnosticScreenView>();
            foreach (var target in targets)
            {
                var view = target.screen;
                if (view == null || !used.Add(view) || view.title == null || view.output == null || view.outputRect == null ||
                    view.commands == null || view.commands.Length != 6 || view.commands.Any(b => b == null) ||
                    view.nodes == null || view.nodes.Any(n => n == null || n.button == null || n.label == null))
                { interactionStatus = $"Vista incompleta o compartida: {target.name}. Revisar Console."; Debug.LogError("M3: cada dispositivo necesita una vista fija completa y exclusiva.", target); enabled = false; return false; }
            }
            return true;
        }

        private void BindViews()
        {
            foreach (var target in targets)
            {
                var view = target.screen;
                view.gameObject.SetActive(false);
                BindAdministration(view);
                if (view.xRayButton != null) Bind(view.xRayButton, () => { if (IsOpen && View == view && !Blocked) XRayRequested?.Invoke(); });
                for (int i = 0; i < view.commands.Length; i++)
                {
                    int command = i; // Capturar el índice de este botón, no el contador mutable del bucle.
                    Bind(view.commands[i], () => Command(view, command));
                }
                foreach (var node in view.nodes)
                {
                    string id = node.deviceId;
                    Bind(node.button, () => SelectNode(view, id));
                }
            }
        }

        private void Bind(Button button, UnityAction callback)
        { if (button == null) return; button.onClick.AddListener(callback); listeners.Add((button, callback)); }
        private void HideHints()
        {
            if (targets == null) return;
            foreach (var target in targets) if (target.focusHint != null) target.FocusHintRoot.SetActive(false);
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
            if (IsOpen) UpdateOpenScreen();
            else UpdatePointedDevice();
        }

        // Mantener una pantalla abierta sólo requiere alcance, disponibilidad y revisión vigente.
        private void UpdateOpenScreen()
        {
            interactionStatus = $"Canvas abierto: {currentTarget.DeviceId}";
            bool outsideRange = Vector3.Distance(playerCamera.transform.position, currentTarget.InteractionPosition) > settings.interactionRange;
            if (!currentTarget.isActiveAndEnabled || outsideRange || InteractPressed)
            {
                Close();
                return;
            }
            if (shownRevision == networkScene.Session.Revision) return;
            RefreshNodes();
            var admin = View.GetComponent<NetworkAdministrationView>();
            if (admin != null && admin.panel.activeSelf) RefreshAdministration(admin);
            View.ShowOutput("La red cambió. Repite la consulta; las observaciones anteriores están obsoletas.");
        }

        private void UpdatePointedDevice()
        {
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
            var flow = Modules.Module03_Diagnostics.Flow.Module03GuidedFlow.Instance;
            if (flow != null && !flow.Allows(DiagnosticStage.OpenLaptop))
            { interactionStatus = "Espera la introducción del instructor para abrir la terminal."; return; }
            if (target.focusHint != null)
            {
                target.FocusHintRoot.SetActive(true);
                interactionStatus = target.FocusHintRoot.activeInHierarchy
                    ? $"Hint activado: {target.DeviceId}. Esperando A. Si no es visible, revisar posición/orientación/render del texto."
                    : $"Hint activado pero un padre está desactivado: {target.focusHint.name}";
            }
            else interactionStatus = $"Falta Focus Hint: {target.DeviceId}";
            if (InteractPressed) Open(target);
        }
        private void Open(DiagnosticInteractionTarget target)
        {
            var flow = Modules.Module03_Diagnostics.Flow.Module03GuidedFlow.Instance;
            if (flow != null && !flow.Allows(DiagnosticStage.OpenLaptop)) return;
            currentTarget = target; workspace.OpenLocal(target.DeviceId);
            ScreenOpened?.Invoke(target.DeviceId);
            // Solo visibilidad y datos: posición, escala y orientación son las guardadas en escena.
            View.gameObject.SetActive(true);
            SetXRayIndicator(xRayEnabled);
            View.title.text = $"{target.DeviceId} · contexto local | Ping desde {workspace.LocalSourcePortId}";
            var admin = View.GetComponent<NetworkAdministrationView>();
            if (admin != null) admin.panel.SetActive(false);
            HideHints(); RefreshNodes(); RefreshContext();
            View.ShowOutput(target.DeviceId == settings.laptopDeviceId ? settings.mapTitle : workspace.LocalConfiguration());
        }
        public void Close()
        {
            if (View != null)
            {
                View.gameObject.SetActive(false);
                ScreenClosed?.Invoke();
            }
            currentTarget = null;
        }
        private void SelectNode(DiagnosticScreenView view, string id)
        {
            if (view != View || !IsOpen || Blocked) return;
            selected = id; workspace.SelectDestination(id); RefreshNodes();
            var admin = view.GetComponent<NetworkAdministrationView>();
            if (admin != null) { admin.portIndex = 0; if (!workspace.CanManageSwitch && !workspace.CanEditLocalAddress) admin.panel.SetActive(false); }
            RefreshContext();
            if (admin != null && admin.panel.activeSelf) RefreshAdministration(admin);
            view.ShowOutput(id + "\nIP documentada: " + (workspace.ExpectedAddress(id) ?? "No aplica (pasivo)") + "\n" + workspace.Observation(id));
        }
        private void Command(DiagnosticScreenView view, int index)
        {
            if (view != View || !IsOpen || Blocked || !view.commands[index].gameObject.activeSelf) return;
            CommandExecuting?.Invoke();
            CommandInvoked?.Invoke(index);
            try
            {
                switch (index)
                {
                    case 0: ExecutePing(view); break;
                    case 1: ShowConsole(); view.ShowOutput(workspace.Neighbours()); break;
                    case 2:
                        if (IsManagingSwitch) view.GetComponent<NetworkAdministrationView>().details.text = workspace.SwitchPorts(selected);
                        else { ShowConsole(); view.ShowOutput(workspace.LocalConfiguration()); }
                        break;
                    case 3:
                        var admin = view.GetComponent<NetworkAdministrationView>();
                        if (admin != null && workspace.CanManageSwitch)
                            AdminAction(view, admin, () => admin.panel.SetActive(true));
                        break;
                    case 4: ShowConsole(); view.ShowOutput(workspace.History()); break;
                    case 5: Close(); break;
                }
            }
            catch (Exception e) { view.ShowOutput(e.Message); }
        }
        private void ExecutePing(DiagnosticScreenView view)
        {
            RequireStage(DiagnosticStage.FirstPing);
            ShowConsole();
            var result = workspace.PingSelected();
            view.ShowOutput(workspace.PingOutput(result));
            RefreshNodes();
            ProbeExecuted?.Invoke(result, view);
        }

        private void ShowConsole()
        {
            var admin = View.GetComponent<NetworkAdministrationView>();
            if (admin != null) admin.panel.SetActive(false);
            RefreshContext();
        }


        private void BindAdministration(DiagnosticScreenView view)
        {
            var admin = view.GetComponent<NetworkAdministrationView>();
            if (admin == null) return; // Las escenas de sprints anteriores siguen funcionando.
            admin.panel.SetActive(false);
            Bind(admin.open, () => AdminAction(view, admin, () => { if (!workspace.CanEditLocalAddress && !workspace.CanManageSwitch) return; admin.portIndex = 0; admin.panel.SetActive(true); }));
            Bind(admin.back, () => AdminAction(view, admin, () => admin.panel.SetActive(false)));
            Bind(admin.previousPort, () => AdminAction(view, admin, () => admin.portIndex--));
            Bind(admin.previousAddress, () => AdminAction(view, admin, () => admin.addressIndex--));
            Bind(admin.nextPort, () => AdminAction(view, admin, () => admin.portIndex++));
            Bind(admin.nextAddress, () => AdminAction(view, admin, () => admin.addressIndex++));
            // Compatibilidad con prefabs ya guardados: ocultar el selector antiguo.
            if (admin.nextPrefix != null) admin.nextPrefix.gameObject.SetActive(false);
            var applyLabel = admin.applyAddress.GetComponentInChildren<TMPro.TMP_Text>();
            if (applyLabel != null) applyLabel.text = "Aplicar IP";
            var verifyLabel = admin.verify.GetComponentInChildren<TMPro.TMP_Text>();
            if (verifyLabel != null) verifyLabel.text = "Comprobar reparación";
            Bind(admin.applyAddress, () => AdminAction(view, admin, () =>
            {
                RequireStage(DiagnosticStage.Addresses);
                var port = AdminPort(admin);
                if (!workspace.ConfigureLocalAddress(port.id, administration.addressOptions[admin.addressIndex]))
                    throw new InvalidOperationException("IP o prefijo inválidos. Consulta la bitácora.");
            }));
            Bind(admin.enablePort, () => AdminAction(view, admin, () => ApplySwitchPort(admin, true)));
            Bind(admin.disablePort, () => AdminAction(view, admin, () => ApplySwitchPort(admin, false)));
            Bind(admin.verify, () => AdminAction(view, admin, () => { }, true));
        }

        private static void RequireStage(DiagnosticStage stage)
        {
            var flow = Modules.Module03_Diagnostics.Flow.Module03GuidedFlow.Instance;
            if (flow != null && !flow.Allows(stage))
                throw new InvalidOperationException("Esta acción se habilita al llegar a su objetivo. Revisa tu muñeca y la instrucción del instructor.");
        }

        private void ApplySwitchPort(NetworkAdministrationView admin, bool enabled)
        {
            RequireStage(DiagnosticStage.Ports);
            if (!workspace.ConfigureSwitchPort(AdminPort(admin).id, enabled))
                throw new InvalidOperationException("No se pudo cambiar el puerto. Consulta la bitácora.");
        }

        private PortDefinition AdminPort(NetworkAdministrationView admin)
        {
            var ports = workspace.EditablePorts();
            if (ports.Count == 0) throw new InvalidOperationException("Abre una PC o selecciona el switch desde la laptop.");
            admin.portIndex = Wrap(admin.portIndex, ports.Count);
            return ports[admin.portIndex];
        }

        private void AdminAction(DiagnosticScreenView view, NetworkAdministrationView admin, Action action, bool verify = false)
        {
            if (view != View || !IsOpen || Blocked) return;
            CommandExecuting?.Invoke();
            try
            {
                if (administration == null || administration.targetRules == null || administration.addressOptions == null ||
                    administration.addressOptions.Length == 0)
                    throw new InvalidOperationException("Falta configuración de administración. Ejecuta el configurador del sprint 7.");
                action(); RefreshNodes(); RefreshContext(); RefreshAdministration(admin);
                if (verify) admin.details.text = VerifyConfiguration();
            }
            catch (Exception e) { admin.details.text = e.Message; view.ShowOutput(e.Message); }
        }

        private void RefreshAdministration(NetworkAdministrationView admin)
        {
            if (administration == null) return;
            var ports = workspace.EditablePorts();
            bool pc = workspace.CanEditLocalAddress;
            foreach (var control in new[] { admin.previousAddress, admin.nextAddress, admin.applyAddress })
                if (control != null) { control.gameObject.SetActive(pc); control.interactable = ports.Count > 0; }
            if (admin.addressLabel != null) admin.addressLabel.gameObject.SetActive(pc);
            foreach (var control in new[] { admin.enablePort, admin.disablePort })
                if (control != null) control.gameObject.SetActive(!pc && workspace.CanManageSwitch);
            admin.nextPort.interactable = ports.Count > 1;
            if (admin.previousPort != null) admin.previousPort.interactable = ports.Count > 1;
            if (ports.Count == 0) { admin.details.text = "Selecciona el switch en el minimapa de la laptop o abre una PC presencialmente."; return; }
            var port = AdminPort(admin);
            admin.addressIndex = Wrap(admin.addressIndex, administration.addressOptions.Length);
            if (admin.portLabel != null) admin.portLabel.text = "PUERTO\n" + port.id;
            if (admin.addressLabel != null) admin.addressLabel.text = "IP PROPUESTA\n" + administration.addressOptions[admin.addressIndex] + "/" + workspace.DocumentedPrefix(port.id);
            admin.enablePort.interactable = !port.enabled;
            admin.disablePort.interactable = port.enabled;
            // La consulta del switch verifica acceso antes de mostrar su configuración remota.
            admin.details.text = pc ? workspace.LocalConfiguration() : workspace.SwitchPorts(workspace.SelectedDeviceId);
            admin.details.text += "\n\nPuerto seleccionado: " + port.id + (pc ?
                "\nPropuesta: " + administration.addressOptions[admin.addressIndex] + "/" + workspace.DocumentedPrefix(port.id) :
                "\nHabilitar/deshabilitar requiere acceso de gestión en cada cambio.");
        }

        private static int Wrap(int index, int count) => count > 0 ? (index % count + count) % count : 0;

        private bool IsManagingSwitch => workspace.CanManageSwitch &&
            View.GetComponent<NetworkAdministrationView>() is NetworkAdministrationView admin && admin.panel.activeSelf;

        private void RefreshContext()
        {
            var admin = View.GetComponent<NetworkAdministrationView>();
            bool remote = IsManagingSwitch;
            View.title.text = remote ? $"{selected} · Gestión desde {workspace.LocalDeviceId}" :
                $"{workspace.LocalDeviceId} · Consola local | Origen: {workspace.LocalSourcePortId}";
            View.commands[0].gameObject.SetActive(!remote);
            View.commands[1].gameObject.SetActive(!remote);
            View.commands[3].gameObject.SetActive(workspace.CanManageSwitch && !remote);
            var configLabel = View.commands[2].GetComponentInChildren<TMPro.TMP_Text>();
            if (configLabel != null) configLabel.text = remote ? "Datos de gestión" : "IP / máscara / MAC";
            if (admin == null) return;
            admin.open.gameObject.SetActive(!admin.panel.activeSelf && (workspace.CanEditLocalAddress || workspace.CanManageSwitch));
            var label = admin.open.GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null) label.text = workspace.CanEditLocalAddress ? "Modificar IP" : "Administrar switch";
        }

        private string VerifyConfiguration()
        {
            var rules = administration.targetRules.Evaluate(networkScene.Session);
            var required = administration.requiredRuleIds;
            var devices = administration.verificationDeviceIds;
            if (required == null || required.Length == 0 || devices == null || devices.Length == 0)
                return "Faltan reglas o equipos de verificación en GameData.";
            bool configuration = required.All(id => rules.Any(r => r.RuleId == id && r.Passed));
            bool probes = devices.All(workspace.HasCurrentSuccessfulProbe);
            return "COMPROBAR REPARACIÓN DE IP Y PUERTOS\n\n" +
                "Esta consulta no repara la red. Comprueba la configuración y las pruebas realizadas.\n\n" +
                (configuration && probes ? "RESULTADO: reparación verificada." : "RESULTADO: faltan comprobaciones.") +
                "\n\n1. Configuración de IP y puertos: " + (configuration ? "correcta" : "revisa las direcciones y los puertos habilitados") +
                "\n\n2. Evidencia de conectividad:\n" +
                string.Join("\n", devices.Select(id => id + ": " + (workspace.HasCurrentSuccessfulProbe(id) ? "OK · ping vigente al equipo correcto" : "PENDIENTE · ejecuta ping después del último cambio"))) +
                "\n\nLa continuidad y el etiquetado del cable se comprueban por separado con las herramientas de reparación.";
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
