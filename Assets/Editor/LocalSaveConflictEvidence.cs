#if UNITY_EDITOR
using System;
using System.IO;
using Systems.Auth;
using Systems.Save;
using UnityEditor;
using UnityEngine;

/// <summary>Prepara una diferencia local real sin contactar ni escribir Firebase.</summary>
public static class LocalSaveConflictEvidence
{
    private const string MenuPath = "Network Simulator/Pruebas/Preparar diferencia local con respaldo";

    [MenuItem(MenuPath)]
    private static void Prepare()
    {
        SaveManager manager = SaveManager.Instance;
        if (!Application.isPlaying || manager == null || !SessionContext.CanSyncToFirebase || manager.IsSyncInProgress)
        {
            Debug.LogWarning("Inicia Play con una cuenta Firebase y espera a que termine cualquier sincronización.");
            return;
        }

        int slotId = manager.saveFile?.activeSlotId ?? -1;
        SaveSlot slot = manager.saveFile?.slots?.Find(item => item.slotID == slotId);
        if (slot == null || manager.IsSlotEmpty(slotId))
        {
            Debug.LogWarning("Selecciona primero una partida existente de la cuenta de prueba.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, $"save_{SessionContext.LocalStorageKey}.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("No existe el archivo local que se debe respaldar.", path);

        string backup = path + ".evidencia-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".bak";
        File.Copy(path, backup, false);
        SaveFile changed = JsonUtility.FromJson<SaveFile>(File.ReadAllText(path));
        SaveSlot changedSlot = changed.slots.Find(item => item.slotID == slotId);
        if (changedSlot == null || changedSlot.data == null)
            throw new InvalidOperationException("La partida seleccionada no coincide con la copia en disco.");

        changedSlot.moduleTitle = "PRUEBA LOCAL - " + changedSlot.moduleTitle;
        changedSlot.needsCloudSync = true;
        AtomicLocalFile.Write(path, JsonUtility.ToJson(changed, true));
        manager.LoadFromLocal();
        Debug.Log($"[Evidencia] Diferencia local preparada en slot {slotId}. Firebase no fue modificado.\nArchivo: {path}\nRespaldo: {backup}\nDetén Play y vuelve a iniciar sesión para comprobar que se conserva PRUEBA LOCAL. No pulses Sincronizar durante la prueba.");
        EditorUtility.RevealInFinder(path);
    }
}
#endif
