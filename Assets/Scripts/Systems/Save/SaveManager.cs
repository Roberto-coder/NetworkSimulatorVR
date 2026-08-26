using UnityEngine;
using System.IO;
using System;
using GameData.Achievements;
using Systems.Auth;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    string savePath;
    public SaveFile saveFile;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            RefreshSavePath();
            LoadFromLocal();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Cargar archivo al iniciar sesión
    public void LoadFromLocal()
    {
        try
        {
            RefreshSavePath();
            string path = savePath;

            Debug.Log("LoadFromLocal path: " + path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                Debug.Log("JSON cargado: " + json);

                saveFile = JsonUtility.FromJson<SaveFile>(json);
            }
            else
            {
                Debug.Log("No existe save local, creando nuevo");

                saveFile = new SaveFile();
            }

            NormalizeSaveFile();
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error en LoadFromLocal: " + e);
            saveFile = new SaveFile();
            NormalizeSaveFile();
        }
    }

    // Guardado manual
    public void SaveGame(int slotID, string module, float playtime)
    {
        SaveSlot slot = new SaveSlot();

        slot.slotID = slotID;
        slot.moduleTitle = module;
        slot.lastSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        slot.playTime = playtime;
        slot.screenshot = "";
        slot.needsCloudSync = true;

        slot.data = new PlayerProgress();
        slot.data.level = 1;
        slot.data.progress = 0.5f;

        int existingIndex = saveFile.slots.FindIndex(item => item.slotID == slotID);
        if (existingIndex >= 0)
            saveFile.slots[existingIndex] = slot;
        else
            saveFile.slots.Add(slot);

        saveFile.activeSlotId = slotID;
        SessionContext.SelectSlot(slotID);

        SaveLocal();
    }

    public void ImportRemoteJson(string json)
    {
        RefreshSavePath();
        saveFile = string.IsNullOrWhiteSpace(json)
            ? new SaveFile()
            : JsonUtility.FromJson<SaveFile>(json);
        NormalizeSaveFile();
        SaveLocal();
    }

    public bool RestoreSessionData(string remoteJson)
    {
        // El UID puede cambiar después de que Firebase restaura su sesión.
        LoadFromLocal();
        bool hasPendingLocalChanges = saveFile?.slots?.Exists(slot => slot.needsCloudSync) == true;

        if (string.IsNullOrWhiteSpace(remoteJson) || hasPendingLocalChanges)
            return hasPendingLocalChanges;

        ImportRemoteJson(remoteJson);
        return false;
    }

    public void ClearInMemorySession()
    {
        saveFile = new SaveFile();
        NormalizeSaveFile();
        RefreshSavePath();
    }

    public bool SelectSlot(int slotId)
    {
        EnsureSaveFile();
        SaveSlot slot = saveFile.slots.Find(item => item.slotID == slotId);
        if (slot == null)
            return false;

        saveFile.activeSlotId = slotId;
        SessionContext.SelectSlot(slotId);
        SaveLocal();
        return true;
    }

    public bool IsSlotEmpty(int slotId)
    {
        EnsureSaveFile();
        SaveSlot slot = saveFile.slots.Find(item => item.slotID == slotId);
        if (slot == null || slot.data == null)
            return true;

        // Compatibilidad con archivos antiguos que serializaron `data: {}`
        // aun cuando el slot nunca tuvo una partida real.
        bool hasSaveMetadata = !string.IsNullOrWhiteSpace(slot.moduleTitle)
            || !string.IsNullOrWhiteSpace(slot.lastSave)
            || slot.playTime > 0f;
        PlayerProgress progress = slot.data;
        bool hasProgress = progress.level > 0
            || progress.progress > 0f
            || progress.completedModuleIds?.Count > 0
            || progress.achievements?.Count > 0
            || progress.completedTutorialIds?.Count > 0
            || progress.modules?.Count > 0;

        return !hasSaveMetadata && !hasProgress;
    }

    public void DeleteSlot(int slotId)
    {
        EnsureSaveFile();
        int index = saveFile.slots.FindIndex(item => item.slotID == slotId);
        SaveSlot empty = CreateEmptySlot(slotId);
        if (index >= 0)
            saveFile.slots[index] = empty;
        else
            saveFile.slots.Add(empty);

        if (saveFile.activeSlotId == slotId)
        {
            saveFile.activeSlotId = -1;
            SessionContext.SelectSlot(-1);
        }

        SaveLocal();
    }

    public void CompleteModuleLocally(
        string moduleId,
        string moduleTitle,
        AchievementDefinition achievement,
        int totalObjectives = 0,
        float modulePlayTime = -1f)
    {
        EnsureSaveFile();
        SaveSlot slot = GetOrCreateActiveSlot(moduleTitle);
        PlayerProgress progress = slot.data ??= new PlayerProgress();
        progress.completedModuleIds ??= new System.Collections.Generic.List<string>();
        progress.achievements ??= new System.Collections.Generic.List<AchievementProgress>();
        progress.modules ??= new System.Collections.Generic.List<ModuleProgress>();

        if (!progress.completedModuleIds.Contains(moduleId))
            progress.completedModuleIds.Add(moduleId);

        if (achievement != null &&
            !progress.achievements.Exists(item => item.achievementId == achievement.AchievementId))
        {
            progress.achievements.Add(new AchievementProgress
            {
                achievementId = achievement.AchievementId,
                unlockedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        ModuleProgress moduleProgress = progress.modules.Find(item => item.moduleId == moduleId);
        if (moduleProgress == null)
        {
            moduleProgress = new ModuleProgress { moduleId = moduleId };
            progress.modules.Add(moduleProgress);
        }

        moduleProgress.moduleName = moduleTitle;
        moduleProgress.completed = true;
        moduleProgress.totalObjectives = Mathf.Max(moduleProgress.totalObjectives, totalObjectives);
        moduleProgress.completedObjectives = moduleProgress.totalObjectives;
        moduleProgress.playTime = modulePlayTime >= 0f ? modulePlayTime : Mathf.Max(moduleProgress.playTime, slot.playTime);
        moduleProgress.completedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (modulePlayTime >= 0f)
            slot.playTime += modulePlayTime;

        slot.moduleTitle = moduleTitle;
        slot.lastSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        slot.needsCloudSync = true;
        SaveLocal();
    }

    public bool HasCompletedTutorial(string tutorialId)
    {
        EnsureSaveFile();
        SaveSlot slot = saveFile.slots.Find(item => item.slotID == saveFile.activeSlotId);
        return slot?.data?.completedTutorialIds?.Contains(tutorialId) == true;
    }

    public void CompleteTutorialLocally(string tutorialId, string locationTitle)
    {
        EnsureSaveFile();
        SaveSlot slot = GetOrCreateActiveSlot(locationTitle);
        PlayerProgress progress = slot.data ??= new PlayerProgress();
        progress.completedTutorialIds ??= new System.Collections.Generic.List<string>();

        if (!progress.completedTutorialIds.Contains(tutorialId))
            progress.completedTutorialIds.Add(tutorialId);

        slot.moduleTitle = locationTitle;
        slot.lastSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        slot.needsCloudSync = true;
        SaveLocal();
    }

    public void SyncLocalToFirebase(Action<bool, string> callback = null)
    {
        EnsureSaveFile();
        if (!SessionContext.CanSyncToFirebase)
        {
            Debug.LogWarning("La sesión actual es local; no se sincronizará con Firebase.");
            callback?.Invoke(false, "Sesión local: guardado conservado en este dispositivo");
            return;
        }

        if (FirebaseSaveManager.Instance == null)
        {
            Debug.LogWarning("No hay FirebaseSaveManager activo; el guardado local se conserva.");
            callback?.Invoke(false, "Firebase no está disponible");
            return;
        }

        System.Collections.Generic.List<int> pendingSlots = saveFile.slots
            .FindAll(slot => slot.needsCloudSync)
            .ConvertAll(slot => slot.slotID);

        foreach (SaveSlot slot in saveFile.slots)
            slot.needsCloudSync = false;

        SaveLocal();
        FirebaseSaveManager.Instance.UploadSave(saveFile, (success, message) =>
        {
            if (!success)
            {
                foreach (SaveSlot slot in saveFile.slots)
                    slot.needsCloudSync = pendingSlots.Contains(slot.slotID);
                SaveLocal();
            }

            callback?.Invoke(success, message);
        });
    }

    public void SaveLocal()
    {
        EnsureSaveFile();
        string json = JsonUtility.ToJson(saveFile,true);
        File.WriteAllText(savePath,json);
    }

    // Autosave (no sube a firebase)
    public void AutoSave(int slotID)
    {
        if(slotID < saveFile.slots.Count)
        {
            saveFile.slots[slotID].lastSave = DateTime.Now.ToString();
            SaveLocal();
        }
    }

    private void EnsureSaveFile()
    {
        saveFile ??= new SaveFile();
        saveFile.slots ??= new System.Collections.Generic.List<SaveSlot>();
    }

    private void NormalizeSaveFile()
    {
        EnsureSaveFile();
        for (int slotId = 0; slotId < 4; slotId++)
        {
            if (!saveFile.slots.Exists(item => item.slotID == slotId))
                saveFile.slots.Add(CreateEmptySlot(slotId));
        }

        foreach (SaveSlot slot in saveFile.slots)
            NormalizePlayerProgress(slot);

        if (SessionContext.ActiveSlotId >= 0)
            saveFile.activeSlotId = SessionContext.ActiveSlotId;
        else if (saveFile.activeSlotId >= 0)
            SessionContext.SelectSlot(saveFile.activeSlotId);
    }

    private static void NormalizePlayerProgress(SaveSlot slot)
    {
        if (slot?.data == null)
            return;

        PlayerProgress progress = slot.data;
        progress.completedModuleIds ??= new System.Collections.Generic.List<string>();
        progress.achievements ??= new System.Collections.Generic.List<AchievementProgress>();
        progress.completedTutorialIds ??= new System.Collections.Generic.List<string>();
        progress.modules ??= new System.Collections.Generic.List<ModuleProgress>();

        foreach (string moduleId in progress.completedModuleIds)
        {
            if (string.IsNullOrWhiteSpace(moduleId) || progress.modules.Exists(item => item.moduleId == moduleId))
                continue;

            progress.modules.Add(new ModuleProgress
            {
                moduleId = moduleId,
                moduleName = slot.moduleTitle,
                completed = true,
                completedObjectives = 0,
                totalObjectives = 0,
                playTime = slot.playTime,
                completedAt = slot.lastSave
            });
        }
    }

    private void RefreshSavePath()
    {
        string key = SessionContext.LocalStorageKey;
        savePath = Path.Combine(Application.persistentDataPath, $"save_{key}.json");
    }

    private static SaveSlot CreateEmptySlot(int slotId) => new SaveSlot
    {
        slotID = slotId,
        moduleTitle = string.Empty,
        lastSave = string.Empty,
        playTime = 0f,
        screenshot = string.Empty,
        needsCloudSync = false,
        data = null
    };

    private SaveSlot GetOrCreateActiveSlot(string moduleTitle)
    {
        if (saveFile.activeSlotId < 0)
        {
            Debug.LogWarning("No había un slot activo; se usará el slot 0 para esta prueba local.");
            saveFile.activeSlotId = 0;
            SessionContext.SelectSlot(0);
        }

        SaveSlot slot = saveFile.slots.Find(item => item.slotID == saveFile.activeSlotId);
        if (slot != null)
            return slot;

        slot = new SaveSlot
        {
            slotID = saveFile.activeSlotId,
            moduleTitle = moduleTitle,
            screenshot = string.Empty,
            data = new PlayerProgress()
        };
        saveFile.slots.Add(slot);
        return slot;
    }
}
