using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using Modules.Module03_Diagnostics.Interaction;
using UnityEngine;
using UnityEngine.Rendering;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>Reproduce evidencia ya calculada; nunca decide ni modifica conectividad.</summary>
    public sealed class ProbeTracePresenter : MonoBehaviour
    {
        public Module03NetworkScene network;
        public DiagnosticScreenController screenController;
        public ProbeVisualSettings settings;
        public LineRenderer overlay;
        public Renderer packet;
        [SerializeField, TextArea] private string playbackStatus;
        private Dictionary<string, NetworkPortAnchor> ports;
        private Dictionary<string, FixedNetworkCable> routes;
        private Material material;
        private Vector3[] points;
        private float[] distances;
        private Coroutine playback;
        private bool holdingFailure;
        private DiagnosticScreenView view;
        private ProbeResult activeProbe;
        public bool XRay { get; private set; }

        private void Start()
        {
            if (network == null || network.Diagnostics == null || screenController == null || settings == null ||
                settings.overlayMaterial == null || overlay == null || packet == null)
            { Debug.LogError("M3 animación: faltan sesión o referencias. Ejecuta el configurador del sprint 5.", this); enabled = false; return; }
            ports = network.GetComponentsInChildren<NetworkPortAnchor>(true).ToDictionary(p => p.PortId);
            routes = network.GetComponentsInChildren<FixedNetworkCable>(true).ToDictionary(r => r.CableId);
            points = new Vector3[Mathf.Clamp(settings.samples, 8, 128) + 1]; distances = new float[points.Length];
            // Una sola copia de material y dos renderers reutilizados para todas las pruebas.
            material = new Material(settings.overlayMaterial);
            overlay.sharedMaterial = material; packet.sharedMaterial = material;
            overlay.useWorldSpace = true; overlay.widthMultiplier = settings.lineWidth;
            packet.transform.localScale = Vector3.one * settings.packetSize;
            SetXRay(settings.xRayInitially); Hide();
            Subscribe();
        }
        private void OnEnable() { if (material != null) Subscribe(); }
        private void Subscribe()
        {
            screenController.ProbeExecuted += Play;
            screenController.ScreenClosed += OnScreenClosed;
            screenController.CommandExecuting += Cancel;
            screenController.XRayRequested += ToggleXRay;
        }
        private void OnDisable()
        {
            if (screenController != null)
            {
                screenController.ProbeExecuted -= Play;
                screenController.ScreenClosed -= OnScreenClosed;
                screenController.CommandExecuting -= Cancel;
                screenController.XRayRequested -= ToggleXRay;
            }
            Cancel();
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
        public void ToggleXRay() => SetXRay(!XRay);
        public void SetXRay(bool value)
        {
            XRay = value;
            if (material == null) return;
            material.SetFloat("_ZTest", value ? (float)CompareFunction.Always : (float)CompareFunction.LessEqual);
            material.SetFloat("_ZWrite", 0);
            // Antes de UI transparente: comprobar oclusión y ambos ojos en el visor real.
            material.renderQueue = 2990;
            screenController.SetXRayIndicator(value);
        }
        public void Play(ProbeResult result, DiagnosticScreenView sourceView)
        {
            Cancel();
            if (!isActiveAndEnabled || material == null || !network.Diagnostics.IsCurrent(result)) return;
            view = sourceView; activeProbe = result;
            playback = StartCoroutine(Replay(result));
        }
        private void OnScreenClosed()
        {
            // Cerrar con A permite mirar el tramo rojo; una nueva consulta limpia la señal.
            if (holdingFailure) view = null;
            else Cancel();
        }
        public void Cancel()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null; activeProbe = null; holdingFailure = false; Hide();
            Report("Animación detenida"); view = null;
        }
        private void Update()
        {
            if (activeProbe == null) return;
            if (Time.timeScale <= 0 || !network.Diagnostics.IsCurrent(activeProbe))
            {
                bool stale = !network.Diagnostics.IsCurrent(activeProbe);
                var previousView = view; Cancel();
                if (previousView != null && previousView.animationStatus != null)
                    previousView.animationStatus.text = stale ? "Traza obsoleta: repetir ping" : "Animación cancelada por pausa";
            }
        }
        private void Report(string message)
        {
            playbackStatus = message;
            if (view != null && view.animationStatus != null) view.animationStatus.text = message;
        }
        private void Hide()
        { if (overlay != null) overlay.enabled = false; if (packet != null) packet.enabled = false; }

        private bool Prepare(ProbeTraceStep step)
        {
            if (!ports.TryGetValue(step.FromPort, out var from) || !ports.TryGetValue(step.ToPort, out var to)) return false;
            FixedNetworkCable route = null;
            if (step.Kind == TraceLinkKind.Cable) routes.TryGetValue(step.LinkId, out route);
            // Una ruta fija desalineada no se sustituye por tráfico atravesando la pared en línea recta.
            if (route != null && (route.Length <= 0.001f || route.EndpointA == null || route.EndpointB == null ||
                Vector3.Distance(route.EvaluateDistance(0), route.EndpointA.transform.position) > 0.02f ||
                Vector3.Distance(route.EvaluateDistance(route.Length), route.EndpointB.transform.position) > 0.02f)) return false;
            bool reverse = route != null && route.EndpointA.PortId != step.FromPort;
            distances[0] = 0;
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (float)(points.Length - 1);
                // Los patch cords aún sin spline y conexiones internas usan un tramo abstracto entre anclajes.
                points[i] = route == null ? Vector3.Lerp(from.transform.position, to.transform.position, t) :
                    route.EvaluateDistance((reverse ? 1 - t : t) * route.Length);
                if (i > 0) distances[i] = distances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
            overlay.widthMultiplier = settings.lineWidth;
            overlay.positionCount = points.Length; overlay.SetPositions(points);
            return true;
        }
        private Vector3 Position(float fraction)
        {
            float distance = fraction * distances[distances.Length - 1];
            for (int i = 1; i < distances.Length; i++)
                if (distances[i] >= distance)
                    return Vector3.Lerp(points[i - 1], points[i],
                        Mathf.InverseLerp(distances[i - 1], distances[i], distance));
            return points[points.Length - 1];
        }
        private bool ShowFailureSection(string sourcePort)
        {
            if (!ports.TryGetValue(sourcePort, out var source)) return false;
            var cable = network.Session.Snapshot().cables.FirstOrDefault(c => c.portA == sourcePort || c.portB == sourcePort);
            if (cable == null) return false;
            string otherId = cable.portA == sourcePort ? cable.portB : cable.portA;
            if (string.IsNullOrEmpty(otherId) || !ports.TryGetValue(otherId, out var other)) return false;
            routes.TryGetValue(cable.id, out var route);
            bool reverse = route != null && route.EndpointB != null && route.EndpointB.PortId == sourcePort;
            if (route != null && (route.Length <= 0.001f || route.EndpointA == null || route.EndpointB == null ||
                Vector3.Distance(route.EvaluateDistance(0), route.EndpointA.transform.position) > 0.02f ||
                Vector3.Distance(route.EvaluateDistance(route.Length), route.EndpointB.transform.position) > 0.02f)) return false;
            float length = route != null ? route.Length : Vector3.Distance(source.transform.position, other.transform.position);
            if (length <= 0.001f) return false;
            float section = Mathf.Min(length, Mathf.Max(0.05f, settings.failureSectionLength));
            for (int i = 0; i < points.Length; i++)
            {
                float distance = section * i / (points.Length - 1f);
                points[i] = route != null ? route.EvaluateDistance(reverse ? length - distance : distance) :
                    Vector3.Lerp(source.transform.position, other.transform.position, distance / length);
            }
            // Es una anotación estática de la consulta, no tráfico ni un diagnóstico de cable roto.
            material.SetColor("_BaseColor", settings.failure);
            overlay.widthMultiplier = Mathf.Max(settings.lineWidth, settings.failureLineWidth);
            overlay.positionCount = points.Length; overlay.SetPositions(points); overlay.enabled = true;
            packet.enabled = false;
            return true;
        }
        private IEnumerator Replay(ProbeResult result)
        {
            int skipped = 0;
            foreach (var step in result.Trace)
            {
                if (!network.Diagnostics.IsCurrent(result)) { Hide(); activeProbe = null; Report("Traza obsoleta: repetir ping"); yield break; }
                if (!Prepare(step)) { skipped++; continue; }
                var color = step.Phase == TracePhase.EchoRequest ? settings.request :
                    step.Phase == TracePhase.EchoReply ? settings.reply : settings.arp;
                material.SetColor("_BaseColor", color);
                Report($"{step.Phase} · intento {step.Attempt} · {step.FromPort} → {step.ToPort} · velocidad didáctica");
                overlay.enabled = true; packet.enabled = settings.animateMotion;
                float duration = Mathf.Max(settings.minimumStepSeconds, distances[distances.Length - 1] / Mathf.Max(0.1f, settings.metresPerSecond));
                for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
                { packet.transform.position = Position(elapsed / duration); yield return null; }
            }
            Hide();
            bool failed = result.Status != ProbeStatus.Success;
            bool sectionShown = failed && ShowFailureSection(result.SourcePort);
            Report($"{result.Status} · {result.Received}/{result.Sent}. " +
                (skipped > 0 ? $"{skipped} tramos visuales omitidos: revisar rutas/anclajes. " : "") +
                (failed ? (sectionShown ? "Tramo rojo en origen: prueba fallida, no localiza la avería. Hasta la siguiente consulta." :
                    "Sin respuesta válida. No hay tramo de origen válido para resaltar.") : "Traza terminada."));
            if (failed)
            {
                holdingFailure = true;
                // Mantener activeProbe permite retirar evidencia que quede obsoleta por cambios de red.
                playback = null;
                yield break;
            }
            yield return new WaitForSeconds(settings.resultSeconds);
            Hide(); activeProbe = null; playback = null;
        }
    }
}
