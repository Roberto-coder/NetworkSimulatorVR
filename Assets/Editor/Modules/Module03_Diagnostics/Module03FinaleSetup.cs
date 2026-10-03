using System;
using System.Linq;
using GameData.Achievements;
using GameData.Quiz;
using Modules.Module03_Diagnostics.Flow;
using Modules.Module03_Diagnostics.Presentation;
using Modules.Module02_RackInstallation.Presentation.Quiz;
using Presentacion.Quiz;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03FinaleSetup
    {
        [MenuItem("Network Simulator/Module 03/Sprint 9 - Quiz y cierre")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = EditorSceneManager.GetActiveScene();
            var flow = Module03ProbeVisualSetup.InScene<Module03GuidedFlow>(scene).Single();
            var data = AssetDatabase.LoadAssetAtPath<QuizData>("Assets/GameData/Quiz/Module03Quiz.asset");
            var achievement = AssetDatabase.LoadAssetAtPath<AchievementDefinition>("Assets/GameData/Achievements/Module03Completion.asset");
            var camera = Module03ProbeVisualSetup.InScene<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
            if (data == null || !data.IsValid || achievement == null || camera == null || flow.settings == null || flow.settings.module == null)
                throw new InvalidOperationException("Falta quiz válido, insignia, cámara o configuración del sprint 8.");
            var existing = Module03ProbeVisualSetup.InScene<Module03FinaleController>(scene).SingleOrDefault();
            if (existing != null)
            {
                Undo.RecordObject(existing, "Reparar cierre M3");
                existing.flow = flow;
                existing.guidance = flow.GetComponent<Module03GuidancePresenter>();
                if (existing.quizRoot == null && existing.quiz != null) existing.quizRoot = existing.quiz.gameObject;
                if (existing.quizRoot == null) throw new InvalidOperationException("Asigna Quiz Root al Canvas existente antes de reaplicar.");
                existing.quiz = existing.quizRoot.GetComponentInChildren<QuizController>(true);
                existing.actions = existing.quizRoot.GetComponentInChildren<Module02QuizActionsView>(true);
                if (existing.quiz == null || existing.actions == null) throw new InvalidOperationException("El Canvas existente no contiene QuizController o QuizActionsView.");
                var moduleData = new SerializedObject(flow.settings.module);
                moduleData.FindProperty("finalQuiz").objectReferenceValue = data;
                moduleData.FindProperty("completionAchievement").objectReferenceValue = achievement;
                moduleData.ApplyModifiedProperties();
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("Referencias del cierre actualizadas. Se conserva el Canvas y su colocación. Closure Status muestra qué espera durante Play.", existing);
                return;
            }
            const string folder = "Assets/Prefabs/UI_Components/Quiz/Module03";
            const string path = folder + "/QuizCanvas_Module03_XRI.prefab";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Prefabs/UI_Components/Quiz", "Module03");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                // Copia de presentación: conserva controles XR, sin modificar el prefab del módulo 2.
                const string source = "Assets/Prefabs/UI_Components/Quiz/Module02/QuizCanvas_Module02_XRI.prefab";
                if (!AssetDatabase.CopyAsset(source, path)) throw new InvalidOperationException("Falta el prefab XR del quiz del módulo 2.");
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            if (prefab.GetComponentInChildren<QuizController>(true) == null || prefab.GetComponentInChildren<Module02QuizActionsView>(true) == null)
                throw new InvalidOperationException("Prefab de quiz incompleto.");
            Undo.RecordObject(flow.settings.module, "Datos cierre M3");
            var definition = new SerializedObject(flow.settings.module);
            definition.FindProperty("finalQuiz").objectReferenceValue = data;
            definition.FindProperty("completionAchievement").objectReferenceValue = achievement;
            definition.ApplyModifiedProperties();
            var root = new GameObject("Module03 finale");
            Undo.RegisterCreatedObjectUndo(root, "Cierre M3");
            root.transform.SetParent(flow.transform.parent, false);
            var finale = Undo.AddComponent<Module03FinaleController>(root);
            var panel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(panel, "Canvas fijo de quiz M3");
            panel.name = "QuizCanvas_Module03";
            var ui = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "_UI");
            if (ui != null) panel.transform.SetParent(ui.transform, false);
            panel.transform.position = camera.transform.position + camera.transform.forward * 2;
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - camera.transform.position);
            foreach (var canvas in panel.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera = camera;
            // Guardado en escena: no crear ni reposicionar el Canvas durante Play.
            finale.flow = flow; finale.quizRoot = panel;
            finale.quiz = panel.GetComponentInChildren<QuizController>(true);
            finale.actions = panel.GetComponentInChildren<Module02QuizActionsView>(true);
            finale.guidance = flow.GetComponent<Module03GuidancePresenter>();
            panel.SetActive(false);
            foreach (var component in panel.GetComponentsInChildren<Component>(true))
                if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(panel);
            EditorUtility.SetDirty(finale);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = panel;
            Debug.Log("Sprint 9 conectado. Ajusta el Canvas fijo, guarda y habilita Modulo3/Lobby en Build Settings. Se mostrará al verificar la práctica.", finale);
        }
    }
}
