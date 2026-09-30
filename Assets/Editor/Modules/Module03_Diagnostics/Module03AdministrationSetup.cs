using System;
using System.Linq;
using GameData.Module03;
using Modules.Module03_Diagnostics.Interaction;
using Modules.Module03_Diagnostics.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03AdministrationSetup
    {
        [MenuItem("Network Simulator/Module 03/Sprint 7 - Puertos e IP")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Salir de Play.");
            var scene = SceneManager.GetActiveScene();
            var networks = Module03ProbeVisualSetup.InScene<Module03NetworkScene>(scene);
            if (networks.Length != 1) throw new InvalidOperationException("La escena debe contener una única red M3.");
            var network = networks[0];
            var controller = Module03ProbeVisualSetup.ControllerFor(network);
            const string path = "Assets/GameData/Module03/Presentation/NetworkAdministrationSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<NetworkAdministrationSettings>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<NetworkAdministrationSettings>();
                AssetDatabase.CreateAsset(settings, path);
            }
            if (settings.targetRules == null)
                settings.targetRules = AssetDatabase.LoadAssetAtPath<NetworkTargetRulesAsset>("Assets/GameData/Module03/Validation/OfficeTargetRules.asset");
            if (settings.targetRules == null) throw new InvalidOperationException("Faltan reglas finales.");
            var errors = settings.targetRules.ValidateAgainst(network.initialState);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            EditorUtility.SetDirty(settings);

            // Modificar el prefab común preserva sprites, posiciones y overrides de cada Canvas.
            var content = PrefabUtility.LoadPrefabContents(DiagnosticScreenPrefabSetup.PrefabPath);
            try
            {
                DiagnosticCanvasBuilder.AddAdministrationControls(content.GetComponent<DiagnosticScreenView>());
                PrefabUtility.SaveAsPrefabAsset(content, DiagnosticScreenPrefabSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(content); }
            foreach (var view in network.GetComponentsInChildren<DiagnosticInteractionTarget>(true)
                .Select(t => t.screen).Where(v => v != null).Distinct())
            {
                if (view.GetComponent<NetworkAdministrationView>() != null) continue;
                Undo.RegisterFullObjectHierarchyUndo(view.gameObject, "Administración M3");
                DiagnosticCanvasBuilder.AddAdministrationControls(view);
                var admin = view.GetComponent<NetworkAdministrationView>();
                Undo.RegisterCreatedObjectUndo(admin.open.gameObject, "Abrir administración");
                Undo.RegisterCreatedObjectUndo(admin.panel, "Panel administración");
                EditorUtility.SetDirty(view);
            }
            Undo.RecordObject(controller, "Configuración administración M3");
            controller.administration = settings;
            EditorUtility.SetDirty(controller);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Sprint 7 preparado. Guarda la escena. Desde la laptop selecciona el switch y abre Administrar; configura IP presencialmente en cada PC. Verifica con nuevos pings.", controller);
        }
    }
}
