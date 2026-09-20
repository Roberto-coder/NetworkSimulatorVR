#if UNITY_EDITOR
using System;
using System.Linq;
using Framework.Spawning;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module02_RackInstallation;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Flow;
using Modules.Module02_RackInstallation.Interaction;
using Modules.Module02_RackInstallation.Presentation;
using Shared.Cabling;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Integra la escena sin reconstruir el rack, los sockets ni las herramientas existentes.</summary>
public static class Module02SequenceSprint07Setup
{
    private const string Folder = "Assets/GameData/Module02_Sequence";
    [MenuItem("Network Simulator/Module 02/Infraestructura/Sprint 7 - Flujo secuencial")]
    public static void Configure()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Modulo2.unity")
            throw new InvalidOperationException("Abre Modulo2 fuera de Play.");
        var roots = scene.GetRootGameObjects();
        T[] All<T>() where T : Component => roots.SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
        T One<T>() where T : Component
        {
            var found = All<T>(); if (found.Length != 1) throw new InvalidOperationException($"Se requiere un único {typeof(T).Name}."); return found[0];
        }
        if (All<Module02SequenceCoordinator>().Length != 0)
        { Debug.Log("El sprint 7 ya está configurado. Ajusta las referencias o Skip Rack Inspection en Module02_Sequence; no se duplicarán spawners."); return; }
        var manager = One<Module02Manager>(); var slot = One<RackInsertionSlot>();
        var cabling = One<Module02CablingState>(); var labeling = One<Module02LabelingState>();
        var power = One<Module02SwitchPower>(); var configuration = One<Module02SwitchConfiguration>();
        var cards = One<InfoCardController>();
        var grab = power.GetComponentInParent<XRGrabInteractable>();
        var fastening = grab != null ? grab.GetComponentInChildren<RackFasteningAssembly>(true) : null;
        if (grab == null || fastening == null) throw new InvalidOperationException("El switch configurado necesita agarre y fijación.");
        var cables = All<PatchCableLink>().Where(c => c.isActiveAndEnabled).ToArray();
        if (cables.Length != 5 || cables.Any(c => c.GetComponent<CableLabelPair>() == null))
            throw new InvalidOperationException("Se necesitan cinco cables activos con etiquetado configurado.");
        var targets = All<RackInfoTarget>().Where(t => t.gameObject.activeInHierarchy && t.Information != null).ToArray();
        var dynamicTargets = targets.Where(t => t.transform.IsChildOf(grab.transform)).ToArray();
        var staticTargets = targets.Where(t => !t.transform.IsChildOf(grab.transform) && t.GetComponentInParent<XRGrabInteractable>() == null).ToArray();
        if (staticTargets.Length == 0 || dynamicTargets.Length == 0) throw new InvalidOperationException("Faltan fichas estáticas o del switch.");
        if (staticTargets.Concat(dynamicTargets).Any(t => string.IsNullOrWhiteSpace(t.Information.Id)) ||
            staticTargets.Select(t => t.Information.Id).Intersect(dynamicTargets.Select(t => t.Information.Id)).Any())
            throw new InvalidOperationException("Las fichas requieren IDs no vacíos y grupos estático/switch sin IDs compartidos.");
        var definition = new SerializedObject(manager).FindProperty("moduleDefinition").objectReferenceValue as ModuleDefinition;
        if (definition == null) throw new InvalidOperationException("Falta ModuleDefinition.");
        var view = new SerializedObject(cards).FindProperty("view").objectReferenceValue as InfoCardView;
        var cardPanel = view != null ? new SerializedObject(view).FindProperty("cardPanel").objectReferenceValue as GameObject : null;
        if (cardPanel == null) throw new InvalidOperationException("Falta el panel de tarjetas.");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/GameData", "Module02_Sequence");

        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Sprint 7: flujo secuencial");
        var root = new GameObject("Module02_Sequence"); Undo.RegisterCreatedObjectUndo(root, "Crear coordinador");
        var coordinator = Undo.AddComponent<Module02SequenceCoordinator>(root);
        Set(coordinator, "manager", manager); Set(coordinator, "slot", slot); Set(coordinator, "installationZone", slot.gameObject);
        Set(coordinator, "fastening", fastening); Set(coordinator, "cabling", cabling); Set(coordinator, "labeling", labeling);
        Set(coordinator, "power", power); Set(coordinator, "configuration", configuration); Set(coordinator, "cards", cards);
        SetList(coordinator, "rackTargets", staticTargets); SetList(coordinator, "switchTargets", dynamicTargets);
        if (grab.GetComponent<Module02GrabGate>() == null) Undo.AddComponent<Module02GrabGate>(grab.gameObject);
        // Un único switch forma parte de la práctica; la ranura conserva sus poses ajustadas.
        Set(slot, "acceptedGrab", grab);
        var slotData = new SerializedObject(slot); slotData.FindProperty("additionalAcceptedGrabs").arraySize = 0; slotData.ApplyModifiedProperties();
        Undo.RecordObject(slot.gameObject, "Desactivar instalación inicial"); slot.gameObject.SetActive(false);
        foreach (var target in targets)
        {
            Undo.RecordObject(target, "Activar inspección inicial"); target.enabled = staticTargets.Contains(target);
        }

        var spawners = new ObjectSpawner[5];
        for (int i = 0; i < cables.Length; i++)
        {
            // Exportar una copia incluye las diez etiquetas y referencias internas ya ajustadas.
            // No se sobreescribe el prefab base compartido con otros módulos.
            var copy = UnityEngine.Object.Instantiate(cables[i].gameObject);
            GameObject prefab;
            try
            {
                copy.name = $"InstallationCable_{i + 1:00}";
                if (copy.GetComponent<CablePlayerCollisionFilter>() == null)
                    copy.AddComponent<CablePlayerCollisionFilter>();
                prefab = PrefabUtility.SaveAsPrefabAsset(copy, $"{Folder}/InstallationCable_{i + 1:00}.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
            if (prefab == null) throw new InvalidOperationException("No se pudo generar el prefab del cable.");
            var point = new GameObject($"CableSpawner_{i + 1:00}"); Undo.RegisterCreatedObjectUndo(point, "Crear spawner");
            point.transform.SetParent(cables[i].transform.parent, false);
            point.transform.SetPositionAndRotation(cables[i].transform.position, cables[i].transform.rotation);
            var spawner = Undo.AddComponent<ObjectSpawner>(point);
            Set(spawner, "prefab", prefab); Set(spawner, "spawnPoint", point.transform); spawners[i] = spawner;
            Undo.RecordObject(cables[i].gameObject, "Conservar plantilla inactiva"); cables[i].gameObject.SetActive(false);
        }
        SetList(coordinator, "cableSpawners", spawners);

        var visuals = Module02SequenceViewBuilder.Build(root, cardPanel, grab.transform.position);
        Set(coordinator, "confirmReview", visuals.confirm);
        Set(coordinator, "status", visuals.status);

        string[] titles = { "Inspeccionar el rack", "Inspeccionar el switch", "Montar el switch", "Asegurar el switch",
            "Conectar los cinco enlaces", "Etiquetar ambos extremos", "Conectar la consola", "Encender el switch", "Configurar el switch" };
        string[] descriptions = { "Recorre y confirma las fichas estáticas.", "Recorre y confirma las fichas del switch.", "Introduce el switch en la posición indicada.",
            "Fija las dos pestañas con el destornillador.", "Conecta alimentación y cuatro enlaces Ethernet según la tabla.", "Aplica las diez etiquetas en conexiones correctas.",
            "Conecta el cable integrado a SW1/Console.", "Completa cableado y etiquetas antes de encender.", "Ordena y aplica los cuatro bloques de configuración." };
        var definitionData = new SerializedObject(definition); var objectives = definitionData.FindProperty("objectives"); objectives.arraySize = 9;
        for (int i = 0; i < 9; i++)
        {
            string path = $"{Folder}/OBJ_{i + 1:00}_{Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[i]}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ObjectiveData>(path);
            if (data == null) { data = ScriptableObject.CreateInstance<ObjectiveData>(); AssetDatabase.CreateAsset(data, path); }
            var serialized = new SerializedObject(data); serialized.FindProperty("id").stringValue = Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds[i];
            serialized.FindProperty("title").stringValue = titles[i]; serialized.FindProperty("description").stringValue = descriptions[i];
            serialized.ApplyModifiedProperties(); objectives.GetArrayElementAtIndex(i).objectReferenceValue = data;
        }
        // availableTools se conserva completo: ninguna herramienta se oculta por objetivo.
        definitionData.ApplyModifiedProperties(); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(group); Selection.activeGameObject = root;
        Debug.Log("Sprint 7 configurado. Ajusta SequenceStatus, revisa las listas de fichas y guarda la escena. Skip Rack Inspection inicia en el objetivo 2 solo en Editor/Development Build.");
    }
    private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
    { var data = new SerializedObject(target); data.FindProperty(name).objectReferenceValue = value; data.ApplyModifiedProperties(); }
    private static void SetList<T>(UnityEngine.Object target, string name, T[] values) where T : UnityEngine.Object
    {
        var data = new SerializedObject(target); var list = data.FindProperty(name); list.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; data.ApplyModifiedProperties();
    }
}
#endif
