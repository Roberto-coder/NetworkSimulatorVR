using GameData.Module03;
using UnityEditor;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Editor
{
    public static class Module03DataValidation
    {
        [MenuItem("Network Simulator/Module 03/Sprint 1 - Validar datos")]
        public static void ValidateOffice()
        {
            // Esta herramienta solo inspecciona los assets; no modifica escenas ni configuración.
            var initial = AssetDatabase.LoadAssetAtPath<NetworkInitialStateAsset>("Assets/GameData/Module03/Topologies/OfficeInitialState.asset");
            var target = AssetDatabase.LoadAssetAtPath<NetworkTargetRulesAsset>("Assets/GameData/Module03/Validation/OfficeTargetRules.asset");
            if (initial == null || target == null) { Debug.LogError("Faltan los assets de red del sprint 1."); return; }
            try
            {
                var errors = target.ValidateAgainst(initial);
                if (errors.Count > 0) { foreach (string error in errors) Debug.LogError(error); return; }
                var session = initial.CreateSession();
                foreach (var result in target.Evaluate(session))
                    Debug.Log($"{result.RuleId}: {(result.Passed ? "CUMPLE" : "PENDIENTE")} | esperado: {result.Expected} | observado: {result.Observed}");
                Debug.Log("Sprint 1: configuración válida. Las condiciones pendientes son fallas de práctica, no errores de carga.");
            }
            catch (System.ArgumentException exception) { Debug.LogError(exception.Message); }
        }
    }
}
