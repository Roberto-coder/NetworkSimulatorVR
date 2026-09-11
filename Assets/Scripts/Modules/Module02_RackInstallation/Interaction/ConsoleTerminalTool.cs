using System;
using System.Collections;
using HPhysic;
using Shared.Cabling;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Modules.Module02_RackInstallation.Interaction
{
    public sealed class ConsoleTerminalTool : MonoBehaviour
    {
        [SerializeField] private GameObject cablePrefab;
        [SerializeField] private NetworkPort integratedSocket;
        [SerializeField] private TMP_Text screen;
        [SerializeField] private string targetDeviceId = "SW1";
        private GameObject spawnedCable;
        private PhysicCable physicsCable;
        private PatchCableLink link;
        private Modules.Module03_Diagnostics.Cable_physics.Scripts.Connector fixedEnd, freeEnd;
        private Module02SwitchPower power;
        private bool initialized;
        private string previousMessage;
        public bool IsConnected { get; private set; }
        public bool IsReady { get; private set; }
        public Module02SwitchPower Power => power;
        public event Action StateChanged;
        private void OnEnable() => StartCoroutine(Initialize());
        private IEnumerator Initialize()
        {
            if (cablePrefab == null || integratedSocket == null || integratedSocket.Socket == null) yield break;
            foreach (var candidate in FindObjectsByType<Module02SwitchPower>(FindObjectsSortMode.None))
                if (candidate.DeviceId == targetDeviceId) { power = candidate; break; }
            // Independent world root: moving/scaling the tablet must not transform all cable points.
            spawnedCable = Instantiate(cablePrefab, integratedSocket.transform.position, integratedSocket.transform.rotation);
            physicsCable = spawnedCable.GetComponent<PhysicCable>();
            link = spawnedCable.GetComponent<PatchCableLink>();
            fixedEnd = spawnedCable.transform.Find("Start")?.GetComponent<Modules.Module03_Diagnostics.Cable_physics.Scripts.Connector>();
            freeEnd = spawnedCable.transform.Find("End")?.GetComponent<Modules.Module03_Diagnostics.Cable_physics.Scripts.Connector>();
            yield return null; // PhysicCable initializes its ordered point list in Start.
            if (physicsCable == null || link == null || physicsCable.StartConnector == null) yield break;
            var start = physicsCable.StartConnector;
            var grab = start.GetComponent<XRGrabInteractable>(); if (grab != null) grab.enabled = false;
            foreach (var detector in start.GetComponentsInChildren<CableEndSocketDetector>()) detector.enabled = false;
            // Begin hanging vertically instead of spawning a long rigid horizontal cable.
            var points = physicsCable.Points;
            if (points != null && points.Count > 1)
            {
                float step = Vector3.Distance(points[0].position, points[1].position);
                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 position = integratedSocket.Socket.ConnectionPosition + Vector3.down * (step * i);
                    points[i].position = position;
                    var body = points[i].GetComponent<Rigidbody>();
                    if (body != null) body.position = position;
                }
            }
            integratedSocket.Socket.Connect(start);
            initialized = start.ConnectedTo == integratedSocket.Socket;
        }
        private void LateUpdate() => RefreshConnection();
        public void RefreshConnection()
        {
            if (link != null) link.RefreshLink();
            if (power != null) power.RefreshSupply();
            bool connected = initialized && link != null && link.Kind == NetworkPortKind.ConsoleRj45 &&
                link.Connects(integratedSocket.Address, targetDeviceId + "/Console");
            bool ready = connected && power != null && power.IsOn;
            string message = !initialized ? "Preparando cable de consola…" : !connected ? "Conecta el cable de consola\na " + targetDeviceId + "/Console" :
                power == null ? "Switch no disponible" : !power.IsOn ? "Enciende el switch" : "Consola lista\nConexión establecida con " + targetDeviceId;
            bool changed = connected != IsConnected || ready != IsReady;
            IsConnected = connected; IsReady = ready;
            if (message != previousMessage) { previousMessage = message; if (screen != null) screen.text = message; }
            if (changed) StateChanged?.Invoke();
        }
        private void OnDisable()
        {
            StopAllCoroutines(); initialized = false;
            if (physicsCable != null)
            {
                // XRI temporarily unparents a held endpoint; explicitly reclaim both before destruction.
                foreach (var end in new[] { fixedEnd, freeEnd })
                {
                    if (end == null) continue;
                    var grab = end.GetComponent<XRGrabInteractable>(); if (grab != null) grab.enabled = false;
                    end.Disconnect(); end.transform.SetParent(spawnedCable.transform, true);
                }
            }
            if (integratedSocket != null && integratedSocket.Socket != null) integratedSocket.Socket.Disconnect();
            if (spawnedCable != null) { spawnedCable.SetActive(false); Destroy(spawnedCable); }
            spawnedCable = null; physicsCable = null; link = null;
            fixedEnd = null; freeEnd = null;
            IsConnected = false; IsReady = false; previousMessage = null; StateChanged?.Invoke();
        }
    }
}
