using Modules.Module02_RackInstallation.Interaction;
using Shared.Cabling;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class Module02XrSocketSetup
{
    [MenuItem("Network Simulator/Module 02/Agregar sockets XR a puertos")]
    public static void Configure()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
        {
            Debug.LogWarning("Abre Modulo2 fuera de Play Mode para configurar sus sockets XR.");
            return;
        }

        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        foreach (var port in root.GetComponentsInChildren<NetworkPort>(true))
        {
            // La terminal crea su cable integrado ya conectado, sin agarre en ese extremo.
            if (port.DeviceId == "TERM1" || port.Socket == null ||
                port.GetComponentInChildren<XRSocketInteractor>(true) != null)
                continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Tools/Module02/Cabling/Module02NetworkSocket.prefab");
            if (prefab == null) throw new System.InvalidOperationException("Falta el prefab Module02NetworkSocket.");
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, port.transform);
            Undo.RegisterCreatedObjectUndo(obj, "Agregar socket XR");
            obj.transform.SetPositionAndRotation(port.Socket.ConnectionPosition, port.Socket.ConnectionRotation);
            var socket = obj.GetComponent<Module02NetworkSocket>();
            socket.port = port;
            PrefabUtility.RecordPrefabInstancePropertyModifications(socket);
            PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
            EditorUtility.SetDirty(socket);
            count++;
        }

        if (count > 0) EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"Módulo 2: {count} sockets XR agregados.");
    }
}
