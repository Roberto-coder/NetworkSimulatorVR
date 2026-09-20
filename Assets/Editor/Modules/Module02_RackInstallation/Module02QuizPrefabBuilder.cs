#if UNITY_EDITOR
using System;
using System.Linq;
using Modules.Module02_RackInstallation.Presentation.Quiz;
using Presentacion.Quiz;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>Crea copias independientes del quiz y sus opciones; conserva el diseño del original.</summary>
public static class Module02QuizPrefabBuilder
{
    public const string Folder = "Assets/Prefabs/UI_Components/Quiz/Module02";
    public const string QuizPath = Folder + "/QuizCanvas_Module02_XRI.prefab";
    private const string OptionPath = Folder + "/AnswerOption_Module02_XRI.prefab";
    public static GameObject Build()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(QuizPath);
        if (existing != null) { Validate(existing); return existing; }
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Prefabs/UI_Components/Quiz", "Module02");
        var option = AssetDatabase.LoadAssetAtPath<GameObject>(OptionPath);
        if (option == null)
        {
            var optionRoot = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI_Components/Quiz/AnswerOption.prefab");
            try
            {
                StripIsdk(optionRoot);
                optionRoot.name = "AnswerOption_Module02_XRI";
                option = PrefabUtility.SaveAsPrefabAsset(optionRoot, OptionPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(optionRoot); }
        }
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI_Components/Quiz/QuizCanvas.prefab");
        try
        {
            StripIsdk(root);
            root.name = "QuizCanvas_Module02_XRI";
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                foreach (var raycaster in canvas.GetComponents<GraphicRaycaster>())
                    UnityEngine.Object.DestroyImmediate(raycaster);
                if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                    canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            }
            root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            // Se conecta al cierre del módulo 2, nunca al coordinador específico del módulo 1.
            foreach (var old in root.GetComponentsInChildren<ModuleCompletionCoordinator>(true)) UnityEngine.Object.DestroyImmediate(old);
            var view = root.GetComponentInChildren<QuizView>(true);
            var controller = root.GetComponentInChildren<QuizController>(true);
            Set(view, "optionPrefab", option.GetComponentInChildren<QuizAnswerOptionView>(true));
            Set(controller, "previewQuiz", null);
            var viewData = new SerializedObject(view);
            var retry = (Button)viewData.FindProperty("retryButton").objectReferenceValue;
            var finish = (Button)viewData.FindProperty("finishButton").objectReferenceValue;
            var results = (GameObject)viewData.FindProperty("resultPanel").objectReferenceValue;
            Label(retry, "Repetir quiz"); Label(finish, "Volver al lobby");
            var restart = UnityEngine.Object.Instantiate(retry, retry.transform.parent);
            restart.name = "RestartModuleButton"; Label(restart, "Reiniciar módulo");
            var saveRetry = UnityEngine.Object.Instantiate(retry, retry.transform.parent);
            saveRetry.name = "RetrySaveButton"; Label(saveRetry, "Reintentar guardado");
            // Las copias solo reciben callbacks en Awake, sin eventos persistentes heredados.
            restart.onClick = new Button.ButtonClickedEvent(); saveRetry.onClick = new Button.ButtonClickedEvent();
            var statusObject = new GameObject("SaveStatus", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            statusObject.transform.SetParent(retry.transform.parent.parent, false);
            statusObject.transform.SetSiblingIndex(retry.transform.parent.GetSiblingIndex());
            var status = statusObject.GetComponent<TextMeshProUGUI>();
            status.font = viewData.FindProperty("resultScoreText").objectReferenceValue is TMP_Text score ? score.font : null;
            status.fontSize = 20; status.alignment = TextAlignmentOptions.Center; status.raycastTarget = false;
            statusObject.GetComponent<LayoutElement>().preferredHeight = 65;
            var actions = root.AddComponent<Module02QuizActionsView>();
            Set(actions, "restartModuleButton", restart); Set(actions, "retrySaveButton", saveRetry);
            Set(actions, "finishButton", finish); Set(actions, "saveStatus", status);
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) text.raycastTarget = false;
            results.SetActive(false);
            ((GameObject)viewData.FindProperty("questionPanel").objectReferenceValue).SetActive(true);
            Validate(root);
            return PrefabUtility.SaveAsPrefabAsset(root, QuizPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    private static void StripIsdk(GameObject root)
    {
        // Los adaptadores ISDK de Canvas viven en hijos propios; se eliminan solo en la copia.
        foreach (var child in root.GetComponentsInChildren<Transform>(true).Reverse())
            if (child != null && child != root.transform && child.name.StartsWith("ISDK_", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        foreach (var component in root.GetComponentsInChildren<Component>(true).Reverse())
            if (component != null && (component.GetType().Namespace ?? "").StartsWith("Oculus.Interaction", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(component);
    }
    private static void Validate(GameObject root)
    {
        if (root.GetComponentsInChildren<Component>(true).Any(c => c == null ||
            (c.GetType().Namespace ?? "").StartsWith("Oculus.Interaction", StringComparison.Ordinal)))
            throw new InvalidOperationException("La copia del quiz conserva scripts ausentes o componentes ISDK.");
        if (root.GetComponentInChildren<QuizController>(true) == null || root.GetComponent<Module02QuizActionsView>() == null ||
            root.GetComponentInChildren<TrackedDeviceGraphicRaycaster>(true) == null)
            throw new InvalidOperationException("Faltan controlador, acciones o raycaster XRI en la copia del quiz.");
    }
    private static void Label(Button button, string value)
    {
        var label = button.GetComponentInChildren<TMP_Text>(true);
        label.text = value; label.enableAutoSizing = true; label.fontSizeMin = 16; label.fontSizeMax = 24;
        var layout = button.GetComponent<LayoutElement>();
        if (layout != null) { layout.preferredWidth = 210; layout.flexibleWidth = 1; }
    }
    private static void Set(UnityEngine.Object target, string property, UnityEngine.Object value)
    {
        var data = new SerializedObject(target); data.FindProperty(property).objectReferenceValue = value; data.ApplyModifiedProperties();
    }
}
#endif
