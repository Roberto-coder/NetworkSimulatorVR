using System.Collections;
using System.Linq;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using TMPro;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Dos snaps XR y secuencia visual. La evidencia sólo se registra al completar una prueba estable.</summary>
    public sealed class XRCableTester : MonoBehaviour
    {
        public RepairTesterSocket master, remote;
        public Renderer[] masterLeds, remoteLeds;
        public TMP_Text feedback;
        private CableRepairController controller;
        private Coroutine routine;
        private MaterialPropertyBlock properties;
        private void Start()
        {
            controller = FindObjectsByType<CableRepairController>(FindObjectsSortMode.None)
                .SingleOrDefault(c => c.gameObject.scene == gameObject.scene);
            properties = new MaterialPropertyBlock();
            LightsOff();
        }
        public bool Accepts(Connector plug) => controller != null && controller.Ready &&
            controller.cables.Any(c => c.isActiveAndEnabled && (c.endA == plug || c.endB == plug));

        private RepairPatchCord DockedCable()
        {
            var a = master != null ? master.SelectedConnector : null;
            var b = remote != null ? remote.SelectedConnector : null;
            if (a == null || b == null || a == b || a.IsConnected || b.IsConnected) return null;
            return controller.cables.FirstOrDefault(c => c.isActiveAndEnabled &&
                ((c.endA == a && c.endB == b) || (c.endA == b && c.endB == a)));
        }
        public void BeginTest()
        {
            var flow = Modules.Module03_Diagnostics.Flow.Module03GuidedFlow.Instance;
            if (flow != null && !flow.Allows(Modules.Module03_Diagnostics.Domain.DiagnosticStage.Cable)) return;
            if (routine != null || controller == null || !controller.Ready) return;
            if (masterLeds == null || remoteLeds == null || masterLeds.Length != 8 || remoteLeds.Length != 8 ||
                masterLeds.Any(r => r == null) || remoteLeds.Any(r => r == null))
            { Say("Faltan los 8 LEDs de cada unidad."); return; }
            controller.SyncConnections();
            var cable = DockedCable();
            if (cable == null) { Say("Encaja ambos extremos del MISMO cable en Master y Remote."); return; }
            routine = StartCoroutine(Test(cable));
        }
        private bool StillValid(RepairPatchCord cable, Connector first, Connector second, long revision) =>
            controller != null && controller.Ready && Time.timeScale > 0 &&
            controller.network.Session.Revision == revision && DockedCable() == cable &&
            master.SelectedConnector == first && remote.SelectedConnector == second;
        private IEnumerator Test(RepairPatchCord cable)
        {
            long revision = controller.network.Session.Revision;
            var first = master.SelectedConnector;
            var second = remote.SelectedConnector;
            bool intact = controller.network.Session.Snapshot().cables.Single(c => c.id == cable.cableId).intact;
            var settings = controller.settings;
            LightsOff();
            Say("Probando continuidad…");
            for (int pin = 0; pin < 8; pin++)
            {
                SetLed(masterLeds[pin], settings.testerLedOn);
                // El dominio sólo modela continuidad global: no inventar cuál conductor está roto.
                SetLed(remoteLeds[pin], intact ? settings.testerLedOn : settings.testerLedOff);
                for (float elapsed = 0; elapsed < Mathf.Max(0.05f, settings.testerLedSeconds); elapsed += Time.deltaTime)
                {
                    if (!StillValid(cable, first, second, revision))
                    { LightsOff(); Say("Prueba cancelada: cambió la conexión, la red o la pausa."); routine = null; yield break; }
                    yield return null;
                }
                SetLed(masterLeds[pin], settings.testerLedOff);
                SetLed(remoteLeds[pin], settings.testerLedOff);
            }
            // Revisar también después del último frame, antes de conceder evidencia.
            if (StillValid(cable, first, second, revision))
            {
                controller.SyncConnections();
                controller.Service.TestCable(cable.cableId, out var message);
                Say(message); controller.RefreshStatus();
            }
            LightsOff(); routine = null;
        }
        private void SetLed(Renderer led, Color color)
        {
            if (led == null) return;
            properties ??= new MaterialPropertyBlock();
            led.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color); properties.SetColor("_Color", color);
            led.SetPropertyBlock(properties);
        }
        private void LightsOff()
        {
            Color off = controller != null ? controller.settings.testerLedOff : Color.black;
            if (masterLeds != null) foreach (var led in masterLeds) SetLed(led, off);
            if (remoteLeds != null) foreach (var led in remoteLeds) SetLed(led, off);
        }
        private void Say(string text) { if (feedback != null) feedback.text = text; }
        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null; LightsOff();
            // Salir de selección antes de destruir la herramienta protege los plugs que XRI puede reparentar.
            Release(master); Release(remote);
        }
        private static void Release(RepairTesterSocket socket)
        {
            if (socket == null || socket.interactionManager == null || !socket.hasSelection) return;
            socket.interactionManager.SelectExit(socket, socket.firstInteractableSelected);
        }
    }
}
