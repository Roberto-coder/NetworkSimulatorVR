using System;
using System.Linq;
using GameData.Module03;
using HPhysic;
using Modules.Module03_Diagnostics.Cable_physics.Scripts;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using Shared.Cabling;
using Framework.Interaction.Tools;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Modules.Module03_Diagnostics.Editor
{
    /// <summary>Prepara variantes del módulo 2 sin modificar sus prefabs ni la escena del módulo 2.</summary>
    public sealed class Module03CableRepairSetup : EditorWindow
    {
        private Module03NetworkScene network;
        private CableRepairSettings settings;
        private string status;
        [MenuItem("Network Simulator/Module 03/Sprint 6 - Reparación y etiquetado")]
        public static void Open() => GetWindow<Module03CableRepairSetup>("M3 reparación");
        private void OnEnable()
        {
            network = Module03ProbeVisualSetup.InScene<Module03NetworkScene>(EditorSceneManager.GetActiveScene()).FirstOrDefault();
            settings = AssetDatabase.LoadAssetAtPath<CableRepairSettings>("Assets/GameData/Module03/Presentation/CableRepairSettings.asset");
        }
        private void OnGUI()
        {
            network = (Module03NetworkScene)EditorGUILayout.ObjectField("Red", network, typeof(Module03NetworkScene), true);
            settings = (CableRepairSettings)EditorGUILayout.ObjectField("Configuración", settings, typeof(CableRepairSettings), false);
            EditorGUILayout.HelpBox("Crea sockets sólo para patch cords, reutiliza cables RJ45 del módulo 2 y añade tester/etiquetadora a la rueda. Revisar colocación física antes de Play.", MessageType.Info);
            if (GUILayout.Button("Crear / completar sprint 6 (Undo)"))
            {
                try { Configure(); status = "Preparado. Revisa conectores, soporte de repuesto y panel; guarda la escena."; }
                catch (Exception e) { status = e.Message; Debug.LogException(e); }
            }
            EditorGUILayout.HelpBox(status ?? "Listo", MessageType.None);
        }
        private void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || network == null || network.gameObject.scene != EditorSceneManager.GetActiveScene())
                throw new InvalidOperationException("Seleccionar red de la escena activa y salir de Play.");
            var scene = network.gameObject.scene;
            var screen = Module03ProbeVisualSetup.ControllerFor(network);
            var tools = Module03ProbeVisualSetup.InScene<ToolManager>(scene);
            if (tools.Length != 1) throw new InvalidOperationException("Se necesita un único ToolManager en la escena.");
            var managers = Module03ProbeVisualSetup.InScene<CableRepairController>(scene).Where(c => c.network == network).ToArray();
            if (managers.Length > 1) throw new InvalidOperationException("Hay varios gestores de reparación para esta red.");
            var n = network.initialState.CopyDefinition();
            var anchors = network.GetComponentsInChildren<NetworkPortAnchor>(true).ToDictionary(p => p.PortId);
            foreach (var cable in n.cables.Where(c => c.interactable))
                foreach (var id in new[] { cable.portA, cable.portB }.Where(id => !string.IsNullOrEmpty(id)))
                    if (!anchors.ContainsKey(id)) throw new InvalidOperationException("Falta anclaje: " + id);
            EnsureAssets();
            // Validar reglas antes de generar objetos, sin alterar el estado inicial.
            var validationSession = new Domain.NetworkSession(n);
            _ = new Domain.CableRepairService(validationSession, new Domain.NetworkSimulationService(validationSession), settings.incident);
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("M3 reparación física");
            try
            {
                foreach (var id in n.cables.Where(c => c.interactable).SelectMany(c => new[] { c.portA, c.portB }).Where(id => !string.IsNullOrEmpty(id)).Distinct())
                {
                    var anchor = anchors[id];
                    if (anchor.GetComponentInChildren<NetworkPort>(true) != null) continue;
                    var socket = (GameObject)PrefabUtility.InstantiatePrefab(settings.socketPrefab, anchor.transform);
                    Undo.RegisterCreatedObjectUndo(socket, "Socket patch cord");
                    socket.transform.localPosition = Vector3.zero; socket.transform.localRotation = Quaternion.identity;
                    var port = socket.GetComponent<NetworkPort>();
                    var connector = socket.GetComponentInChildren<Connector>(true);
                    int slash = id.LastIndexOf('/');
                    port.Configure(id.Substring(0, slash), id.Substring(slash + 1), NetworkPortKind.EthernetRj45, connector);
                    var body = connector.GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
                    SetBool(connector, "makeConnectionKinematic", true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(port);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(body);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(socket.transform);
                }
                var existing = network.GetComponentsInChildren<RepairPatchCord>(true).ToList();
                if (existing.GroupBy(c => c.cableId).Any(g => g.Count() > 1)) throw new InvalidOperationException("IDs de patch cord repetidos.");
                // Convertir en el mismo objeto conserva referencias del gestor, plugs y colocación manual.
                foreach (var old in existing)
                {
                    if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(old.gameObject) == SharedCablePath) continue;
                    if (PrefabUtility.IsPartOfPrefabInstance(old.gameObject))
                    {
                        if (PrefabUtility.GetOutermostPrefabInstanceRoot(old.gameObject) != old.gameObject)
                            throw new InvalidOperationException("El cable " + old.name + " pertenece a un prefab contenedor. Separarlo antes de migrar.");
                        PrefabUtility.UnpackPrefabInstance(old.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    }
                    PrefabUtility.ConvertToPrefabInstance(old.gameObject, settings.patchCordPrefab,
                        new ConvertToPrefabInstanceSettings
                        {
                            objectMatchMode = ObjectMatchMode.ByHierarchy,
                            recordPropertyOverridesOfMatches = true,
                            componentsNotMatchedBecomesOverride = true,
                            gameObjectsNotMatchedBecomesOverride = true,
                            changeRootNameToAssetName = false
                        }, InteractionMode.UserAction);
                }
                foreach (var cable in n.cables.Where(c => c.interactable))
                {
                    if (existing.Any(c => c.cableId == cable.id)) continue;
                    var obj = (GameObject)PrefabUtility.InstantiatePrefab(settings.patchCordPrefab, network.transform);
                    Undo.RegisterCreatedObjectUndo(obj, "Patch cord M3");
                    // La instancia permanece vinculada: geometría y componentes se editan en el prefab común.
                    obj.name = cable.id;
                    var binding = obj.GetComponent<RepairPatchCord>(); binding.cableId = cable.id;
                    var physics = obj.GetComponent<PhysicCable>();
                    Vector3 a = !string.IsNullOrEmpty(cable.portA) ? anchors[cable.portA].transform.position :
                        anchors[settings.incident.portB].transform.position + Vector3.up * 0.4f + Vector3.right * 0.5f;
                    Vector3 b = !string.IsNullOrEmpty(cable.portB) ? anchors[cable.portB].transform.position : a + Vector3.right * 0.7f;
                    PlaceCable(physics, binding, a, b, settings.patchCordLength);
                    if (string.IsNullOrEmpty(cable.portA) && string.IsNullOrEmpty(cable.portB))
                    {
                        // Soporte editable para que el repuesto no caiga al iniciar Play.
                        var tray = GameObject.CreatePrimitive(PrimitiveType.Cube); tray.name = "Soporte " + cable.id;
                        tray.transform.SetParent(network.transform, false); tray.transform.position = (a + b) * 0.5f + Vector3.down * 0.12f;
                        tray.transform.localScale = new Vector3(1.1f, 0.06f, 0.5f); Undo.RegisterCreatedObjectUndo(tray, "Soporte repuesto");
                    }
                    binding.labelA.text = cable.labelA; binding.labelB.text = cable.labelB;
                    // Guardar sólo los valores distintos de esta instancia, sin aplicarlos a todos los cables.
                    foreach (var component in obj.GetComponentsInChildren<Component>(true))
                        if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                    existing.Add(binding);
                }
                var manager = managers.SingleOrDefault();
                if (manager == null)
                {
                    var obj = new GameObject("Cable repair manager");
                    var parent = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "_Managers");
                    if (parent != null) obj.transform.SetParent(parent.transform, false);
                    Undo.RegisterCreatedObjectUndo(obj, "Gestor reparación"); manager = Undo.AddComponent<CableRepairController>(obj);
                }
                Undo.RecordObject(manager, "Referencias reparación"); manager.network = network; manager.settings = settings; manager.diagnostics = screen; manager.cables = existing.ToArray();
                if (manager.statusText == null)
                {
                    manager.statusText = Text(screen.transform, "Repair status", Vector3.zero, new Vector2(650, 90), 22);
                    Undo.RegisterCreatedObjectUndo(manager.statusText.transform.parent.gameObject, "Estado reparación");
                    manager.statusText.transform.parent.position = anchors[settings.incident.portB].transform.position + Vector3.up * 0.6f;
                    manager.statusText.text = "Diagnosticar → sustituir → etiquetar → verificar ping";
                }
                var toolData = new SerializedObject(tools[0]); var list = toolData.FindProperty("availableToolsOverride");
                if (list.arraySize == 0) AddTool(list, "Mano vacía", null);
                foreach (var entry in new[] { ("Tester M3", settings.testerPrefab), ("Etiquetadora M3", settings.labelMakerPrefab) })
                {
                    bool found = false;
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        var item = list.GetArrayElementAtIndex(i);
                        var oldPrefab = item.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
                        if (oldPrefab == entry.Item2) found = true;
                        // Actualizar el slot del tester de proximidad sin duplicar entradas en la rueda.
                        var oldTool = oldPrefab != null ? oldPrefab.GetComponent<CableRepairTool>() : null;
                        if (entry.Item2 == settings.testerPrefab && oldTool != null && oldTool.mode == CableRepairTool.Mode.Tester)
                        {
                            item.FindPropertyRelative("prefab").objectReferenceValue = settings.testerPrefab;
                            item.FindPropertyRelative("name").stringValue = "Tester XR M3";
                            found = true;
                        }
                    }
                    if (!found) AddTool(list, entry.Item1, entry.Item2);
                }
                toolData.ApplyModifiedProperties(); PrefabUtility.RecordPrefabInstancePropertyModifications(tools[0]);
                EditorUtility.SetDirty(manager); EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets(); Undo.CollapseUndoOperations(group);
                Selection.activeGameObject = manager.gameObject;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        private static void AddTool(SerializedProperty list, string name, GameObject prefab)
        {
            int i = list.arraySize++; var item = list.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("name").stringValue = name; item.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            string iconPath = name == "Tester M3" ? "Assets/Recursos/ToolWheel/Sprites/TesterRJ45.png" :
                name == "Etiquetadora M3" ? "Assets/Recursos/ToolWheel/Sprites/etiquetadora.png" : null;
            item.FindPropertyRelative("icon").objectReferenceValue = iconPath == null ? null : AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }
        private void EnsureAssets()
        {
            const string folder = "Assets/GameData/Module03/Presentation";
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<CableRepairSettings>(folder + "/CableRepairSettings.asset");
                if (settings == null) { settings = CreateInstance<CableRepairSettings>(); AssetDatabase.CreateAsset(settings, folder + "/CableRepairSettings.asset"); }
            }
            if (settings.patchCordPrefab == null) settings.patchCordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Module02/Module02_Sequence/InstallationCable_02.prefab");
            if (settings.socketPrefab == null) settings.socketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tools/Module02/Cabling/NetworkPortSocket_RJ45.prefab");
            if (settings.patchCordPrefab == null || settings.socketPrefab == null ||
                settings.patchCordPrefab.GetComponent<PatchCableLink>() == null || settings.patchCordPrefab.GetComponent<PhysicCable>() == null ||
                settings.patchCordPrefab.GetComponent<PatchCableLink>().Kind != NetworkPortKind.EthernetRj45 || settings.socketPrefab.GetComponent<NetworkPort>() == null)
                throw new InvalidOperationException("Asignar prefabs RJ45 válidos del módulo 2 en CableRepairSettings.");
            settings.patchCordPrefab = EnsureSharedCable(settings.patchCordPrefab);
            if (settings.testerPrefab == null || settings.testerPrefab.GetComponent<XRCableTester>() == null)
                settings.testerPrefab = Module03XrTesterBuilder.EnsurePrefab();
            if (settings.labelMakerPrefab == null) settings.labelMakerPrefab = BuildTool(CableRepairTool.Mode.LabelMaker);
            EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        }
        private const string SharedCablePath = "Assets/Prefabs/Dispositivos/Modulo3/PatchCord_M03.prefab";
        private static GameObject EnsureSharedCable(GameObject source)
        {
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(SharedCablePath);
            if (saved != null)
            {
                var binding = saved.GetComponent<RepairPatchCord>();
                if (binding == null || binding.endA == null || binding.endB == null || binding.labelA == null || binding.labelB == null)
                    throw new InvalidOperationException("El prefab PatchCord_M03 requiere binding, extremos y etiquetas completos.");
                return saved; // No sobrescribir cambios del usuario en Prefab Mode.
            }
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                // Sólo se desempaqueta la plantilla temporal. Las instancias de escena permanecen vinculadas.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                obj.name = "PatchCord_M03";
                obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                foreach (var label in obj.GetComponentsInChildren<CableEndpointLabel>(true))
                {
                    var data = new SerializedObject(label);
                    var visual = data.FindProperty("visual").objectReferenceValue as GameObject;
                    if (visual != null) visual.SetActive(false);
                    DestroyImmediate(label);
                }
                foreach (var pair in obj.GetComponentsInChildren<CableLabelPair>(true)) DestroyImmediate(pair);
                foreach (var traffic in obj.GetComponentsInChildren<CableTrafficVisualizer>(true)) traffic.enabled = false;
                var binding = obj.GetComponent<RepairPatchCord>() ?? obj.AddComponent<RepairPatchCord>();
                binding.cableId = ""; // ID e integridad pertenecen al inventario, no al prefab común.
                var physical = new SerializedObject(obj.GetComponent<PhysicCable>());
                binding.endA = ((GameObject)physical.FindProperty("start").objectReferenceValue).GetComponent<Connector>();
                binding.endB = ((GameObject)physical.FindProperty("end").objectReferenceValue).GetComponent<Connector>();
                foreach (var end in new[] { binding.endA, binding.endB })
                {
                    var data = new SerializedObject(end);
                    data.FindProperty("<ConnectedTo>k__BackingField").objectReferenceValue = null;
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
                if (binding.labelA == null) binding.labelA = Text(binding.endA.transform, "Label A", new Vector3(0, 0.025f, 0), new Vector2(100, 24), 13);
                if (binding.labelB == null) binding.labelB = Text(binding.endB.transform, "Label B", new Vector3(0, 0.025f, 0), new Vector2(100, 24), 13);
                binding.labelA.text = ""; binding.labelB.text = "";
                var result = PrefabUtility.SaveAsPrefabAsset(obj, SharedCablePath);
                if (result == null) throw new InvalidOperationException("No se pudo guardar PatchCord_M03.");
                return result;
            }
            finally { DestroyImmediate(obj); }
        }
        [MenuItem("Network Simulator/Module 03/Agregar sockets XR a patch cords")]
        private static void AddNetworkXrSockets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var manager in Module03ProbeVisualSetup.InScene<CableRepairController>(scene))
            foreach (var port in manager.network.GetComponentsInChildren<NetworkPort>(true))
            {
                if (port.Socket == null || port.GetComponentInChildren<RepairNetworkSocket>(true) != null) continue;
                var obj = new GameObject("XR network socket");
                Undo.RegisterCreatedObjectUndo(obj, "Socket XR de red");
                obj.transform.SetParent(port.transform, false);
                obj.transform.SetPositionAndRotation(port.Socket.ConnectionPosition, port.Socket.ConnectionRotation);
                var trigger = obj.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 0.06f;
                var socket = obj.AddComponent<RepairNetworkSocket>(); socket.port = port;
                socket.attachTransform = obj.transform;
                EditorUtility.SetDirty(socket);
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Network Simulator/Module 03/Recolocar patch cords desde sockets")]
        private static void RepositionPatchCords()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var manager in Module03ProbeVisualSetup.InScene<CableRepairController>(scene))
            {
                var ports = manager.network.GetComponentsInChildren<NetworkPort>(true).ToDictionary(p => p.Address);
                foreach (var cable in manager.network.GetComponentsInChildren<RepairPatchCord>(true))
                {
                    var state = manager.network.initialState.CopyDefinition().cables.Find(c => c.id == cable.cableId);
                    if (state == null) continue;
                    // El repuesto conserva los extremos colocados manualmente en su soporte.
                    Vector3 a = cable.endA.transform.position, b = cable.endB.transform.position;
                    if (!string.IsNullOrEmpty(state.portA)) a = ports[state.portA].Socket.ConnectionPosition - (cable.endA.ConnectionPosition - a);
                    if (!string.IsNullOrEmpty(state.portB)) b = ports[state.portB].Socket.ConnectionPosition - (cable.endB.ConnectionPosition - b);
                    if (Vector3.Distance(a, b) > manager.settings.patchCordLength)
                    { Debug.LogWarning(cable.name + ": sockets separados más que la longitud configurada. Acércalos antes de recolocar.", cable); continue; }
                    Undo.RegisterFullObjectHierarchyUndo(cable.gameObject, "Recolocar patch cord");
                    PlaceCable(cable.GetComponent<PhysicCable>(), cable, a, b, manager.settings.patchCordLength);
                    foreach (var component in cable.GetComponentsInChildren<Component>(true))
                        if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void PlaceCable(PhysicCable physical, RepairPatchCord cable, Vector3 a, Vector3 b, float length)
        {
            if (!physical.PlaceBetween(a, b, length, cable.transform.TransformDirection(cable.slackDirection)))
                throw new InvalidOperationException(cable.name + ": extremos demasiado separados o puntos físicos incompletos.");
        }
        private static GameObject BuildTool(CableRepairTool.Mode mode)
        {
            if (!AssetDatabase.IsValidFolder(Module03XrTesterBuilder.Folder))
                AssetDatabase.CreateFolder("Assets/Prefabs/Tools", "Module03");
            string path = Module03XrTesterBuilder.Folder + "/" + mode + "_M03.prefab";
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (saved != null) return saved;
            var root = new GameObject(mode + " M3");
            try
            {
                var identity = root.AddComponent<Framework.Interaction.Tools.Tool>(); var type = new SerializedObject(identity);
                type.FindProperty("type").intValue = (int)(mode == CableRepairTool.Mode.Tester ? ToolType.Tester : ToolType.LabelMaker); type.ApplyModifiedPropertiesWithoutUndo();
                var tool = root.AddComponent<CableRepairTool>(); tool.mode = mode;
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube); body.name = "Tool body"; body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0, 0, 0.07f); body.transform.localScale = new Vector3(0.09f, 0.04f, 0.14f);
                DestroyImmediate(body.GetComponent<Collider>());
                var tip = new GameObject("Tip"); tip.transform.SetParent(root.transform, false); tip.transform.localPosition = Vector3.forward * 0.16f; tool.tip = tip.transform;
                tool.feedback = Text(root.transform, "Display", new Vector3(0, 0.06f, 0.06f), new Vector2(250, 90), 16);
                tool.feedback.text = mode.ToString();
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { DestroyImmediate(root); }
        }
        private static void AddLabelButton(Transform parent, string caption, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var obj = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform; rect.sizeDelta = new Vector2(120, 35); rect.anchoredPosition = position;
            var button = obj.GetComponent<Button>(); button.targetGraphic = obj.GetComponent<Image>(); UnityEventTools.AddPersistentListener(button.onClick, action);
            var label = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); label.transform.SetParent(obj.transform, false);
            ((RectTransform)label.transform).sizeDelta = rect.sizeDelta; var text = label.GetComponent<TextMeshProUGUI>(); text.text = caption; text.color = Color.black; text.fontSize = 13; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        }
        private static TMP_Text Text(Transform parent, string name, Vector3 position, Vector2 size, float font)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Canvas)); panel.transform.SetParent(parent, false);
            panel.transform.localPosition = position; panel.transform.localScale = Vector3.one * 0.001f;
            panel.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace; ((RectTransform)panel.transform).sizeDelta = size;
            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); textObj.transform.SetParent(panel.transform, false);
            ((RectTransform)textObj.transform).sizeDelta = size; var text = textObj.GetComponent<TextMeshProUGUI>(); text.fontSize = font;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
        }
        private static void SetBool(UnityEngine.Object target, string property, bool value)
        { var data = new SerializedObject(target); data.FindProperty(property).boolValue = value; data.ApplyModifiedProperties(); }
    }
}
