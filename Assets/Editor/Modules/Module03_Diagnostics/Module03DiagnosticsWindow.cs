using System;
using GameData.Module03;
using Modules.Module03_Diagnostics.Domain;
using UnityEditor;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    /// <summary>Banco de pruebas del dominio; no escribe assets ni modifica la escena.</summary>
    public sealed class Module03DiagnosticsWindow : EditorWindow
    {
        private NetworkInitialStateAsset initial;
        private NetworkSession session;
        private NetworkSimulationService service;
        private ProbeResult last;
        private string source = "Laptop/eth0", destination = "192.168.10.11", message;
        private Vector2 scroll;
        [MenuItem("Network Simulator/Module 03/Sprint 3 - Diagnóstico")]
        public static void Open() => GetWindow<Module03DiagnosticsWindow>("M3 - Diagnóstico");
        private void OnEnable() => initial = AssetDatabase.LoadAssetAtPath<NetworkInitialStateAsset>("Assets/GameData/Module03/Topologies/OfficeInitialState.asset");
        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox("Sesión de prueba independiente. No cambia tu escena ni los assets. Los botones de incidente usan los IDs de la oficina de ejemplo.", MessageType.Info);
            initial = (NetworkInitialStateAsset)EditorGUILayout.ObjectField("Inicial", initial, typeof(NetworkInitialStateAsset), false);
            if (GUILayout.Button("Crear / reiniciar sesión desde asset")) Run(() => { session = initial.CreateSession(); service = new NetworkSimulationService(session); last = null; });
            source = EditorGUILayout.TextField("Puerto origen", source);
            destination = EditorGUILayout.TextField("IPv4 destino", destination);
            using (new EditorGUI.DisabledScope(service == null))
            {
                if (GUILayout.Button("Ping (4 intentos)")) Run(() => last = service.Ping(source, destination));
                if (GUILayout.Button("Sustituir Patch-01 por repuesto")) Run(() =>
                {
                    if (!session.ConnectCable("Patch-01", "", "") || !session.ConnectCable("Replacement-01", "R01/front", "PC-01/eth0"))
                        throw new InvalidOperationException("No se pudo sustituir el cable; revisar IDs y bitácora.");
                    session.LabelCable("Replacement-01", "R01", "PC-01");
                });
                if (GUILayout.Button("Inhabilitar P02")) Run(() => session.SetPortEnabled("SW-01/P02", false));
                if (GUILayout.Button("Habilitar P02")) Run(() => session.SetPortEnabled("SW-01/P02", true));
                if (GUILayout.Button("Duplicar IP de PC-03 (.12)")) Run(() => session.SetAddress("PC-03/eth0", "192.168.10.12", 24));
                if (GUILayout.Button("Restaurar IP de PC-03 (.13)")) Run(() => session.SetAddress("PC-03/eth0", "192.168.10.13", 24));
            }
            if (last != null)
            {
                EditorGUILayout.LabelField($"{last.Status}: {last.Received}/{last.Sent} respuestas; revisión {last.Revision}");
                EditorGUILayout.HelpBox(last.Message + (service.IsCurrent(last) ? "" : " RESULTADO OBSOLETO: repetir prueba."), MessageType.Info);
                foreach (var step in last.Trace) EditorGUILayout.LabelField($"{step.Phase}: {step.FromPort} → {step.ToPort} [{step.LinkId}]");
            }
            if (service != null)
            {
                EditorGUILayout.LabelField("Vecinos aprendidos del origen");
                foreach (var n in service.GetNeighbours(source)) EditorGUILayout.LabelField($"{n.Ip} → {n.Mac} ({n.PortId})");
            }
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Warning);
            EditorGUILayout.EndScrollView();
        }
        private void Run(Action action)
        {
            try { action(); message = null; }
            catch (Exception e) { message = e.Message; }
        }
    }
}
