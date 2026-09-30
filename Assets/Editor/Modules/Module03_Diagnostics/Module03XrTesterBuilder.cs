using System;
using System.Linq;
using Framework.Interaction.Tools;
using Modules.Module03_Diagnostics.Interaction;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    /// <summary>Copia visual del tester M1: conserva mallas y LEDs, sustituye el acoplamiento por XRI.</summary>
    public static class Module03XrTesterBuilder
    {
        public const string Folder = "Assets/Prefabs/Tools/Module03";
        public const string Path = Folder + "/Tester_XR_M03.prefab";
        public static GameObject EnsurePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (existing != null) return existing;
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Prefabs/Tools", "Module03");
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tools/Module01/Tester.prefab");
            if (source == null) throw new InvalidOperationException("Falta el prefab Tester del módulo 1.");
            var root = new GameObject("Tester_XR_M03");
            try
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
                PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                visual.name = "Tester model (M1)";
                var oldAnimation = visual.GetComponentInChildren<TesterAnimationController>(true);
                if (oldAnimation == null) throw new InvalidOperationException("El tester M1 no tiene su controlador de LEDs.");
                var data = new SerializedObject(oldAnimation);
                var mainLeds = ReadLeds(data.FindProperty("ledRenderers"));
                var remoteLeds = ReadLeds(data.FindProperty("remoteLedRenderers"));
                var masterPose = visual.GetComponentsInChildren<Transform>(true).First(t => t.name == "MasterSocket");
                var remotePose = visual.GetComponentsInChildren<Transform>(true).First(t => t.name == "RemoteSocket");
                Vector3 masterPosition = masterPose.position, remotePosition = remotePose.position;
                Quaternion masterRotation = masterPose.rotation, remoteRotation = remotePose.rotation;
                // La copia no conserva validadores, sockets Meta ni eventos del módulo 1.
                // Desactivar antes de retirar dependencias evita inicialización accidental en Editor.
                visual.SetActive(false);
                var legacyTester = visual.GetComponent<Modules.Module01_CableMaking.Interaction.TesterTool>();
                if (legacyTester != null) UnityEngine.Object.DestroyImmediate(legacyTester);
                foreach (var behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true).Reverse())
                    if (behaviour != null && !(behaviour is TMP_Text)) UnityEngine.Object.DestroyImmediate(behaviour);
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                foreach (var body in visual.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.DestroyImmediate(body);
                visual.SetActive(true);

                var identity = root.AddComponent<Framework.Interaction.Tools.Tool>();
                var identityData = new SerializedObject(identity); identityData.FindProperty("type").intValue = (int)ToolType.Tester; identityData.ApplyModifiedPropertiesWithoutUndo();
                var tester = root.AddComponent<XRCableTester>();
                tester.masterLeds = mainLeds; tester.remoteLeds = remoteLeds;
                tester.master = Socket(root.transform, "Master XR snap", masterPosition, masterRotation, tester);
                tester.remote = Socket(root.transform, "Remote XR snap", remotePosition, remoteRotation, tester);
                tester.feedback = Display(root.transform);
                var input = root.AddComponent<CableRepairTool>(); input.mode = CableRepairTool.Mode.Tester;
                input.tip = tester.master.transform; input.feedback = tester.feedback;
                var result = PrefabUtility.SaveAsPrefabAsset(root, Path);
                if (result == null) throw new InvalidOperationException("No se pudo guardar el tester XR.");
                return result;
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static Renderer[] ReadLeds(SerializedProperty array)
        {
            if (array == null || array.arraySize != 8) throw new InvalidOperationException("Asignar 8 LEDs por unidad en el tester M1.");
            var leds = new Renderer[8];
            for (int i = 0; i < leds.Length; i++)
            {
                leds[i] = array.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                if (leds[i] == null) throw new InvalidOperationException("Referencia LED vacía en tester M1.");
            }
            return leds;
        }
        private static RepairTesterSocket Socket(Transform parent, string name, Vector3 position, Quaternion rotation, XRCableTester tester)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            obj.transform.SetPositionAndRotation(position, rotation);
            var trigger = obj.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 0.035f;
            var body = obj.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            var socket = obj.AddComponent<RepairTesterSocket>(); socket.tester = tester;
            var attach = new GameObject("Attach plug here"); attach.transform.SetParent(obj.transform, false);
            socket.attachTransform = attach.transform;
            return socket;
        }
        private static TMP_Text Display(Transform parent)
        {
            var canvas = new GameObject("XR tester display", typeof(RectTransform), typeof(Canvas));
            canvas.transform.SetParent(parent, false); canvas.transform.localPosition = new Vector3(0, 0.12f, 0.06f);
            canvas.transform.localScale = Vector3.one * 0.001f; canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var label = new GameObject("Result", typeof(RectTransform), typeof(TextMeshProUGUI)); label.transform.SetParent(canvas.transform, false);
            ((RectTransform)label.transform).sizeDelta = new Vector2(320, 70);
            var text = label.GetComponent<TextMeshProUGUI>(); text.fontSize = 16; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.text = "Encaja ambos extremos y pulsa gatillo"; return text;
        }
    }
}
