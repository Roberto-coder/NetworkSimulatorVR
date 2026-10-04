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
    public enum SyncState { Local, Pending, Syncing, WaitingForServer, Synced, Offline, Error }
    public SyncState State { get; private set; } = SyncState.Local;
    public string StatusMessage { get; private set; } = "Progreso local";
    public event Action StatusChanged;
    public bool IsSyncInProgress { get; private set; }
    public bool HasPendingChanges => saveFile?.slots?.Exists(slot => slot != null && slot.needsCloudSync) == true;
    private int sessionGeneration;
    private int syncOperation;

    private void SetStatus(SyncState state, string message)
    {
        State = state;
        StatusMessage = message;
        StatusChanged?.Invoke();
    }

    private void RefreshLocalStatus()
    {
        SetStatus(HasPendingChanges ? SyncState.Pending : SyncState.Local,
            HasPendingChanges ? "Guardado en este dispositivo. Pendiente de sincronizar todas las partidas."
                : "Progreso local disponible.");
    }

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
        sessionGeneration++;
        try
        {
            RefreshSavePath();
            string path = savePath;

            Debug.Log("LoadFromLocal path: " + path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

    

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
        RefreshLocalStatus();
    }

    // Crear una partida nueva; no usar para guardar progreso existente.
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
        {
            RefreshLocalStatus();
            return hasPendingLocalChanges;
        }

        ImportRemoteJson(remoteJson);
        return false;
    }

    public void ClearInMemorySession()
    {
        sessionGeneration++;
        saveFile = new SaveFile();
        NormalizeSaveFile();
        RefreshSavePath();
        RefreshLocalStatus();
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
        empty.needsCloudSync = true; // La eliminacion tambien debe llegar a la nube.
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

    /// <summary>
    /// Guarda completitud con recuperación en memoria. Si falla el archivo, reintentar
    /// no vuelve a sumar tiempo ni deja una insignia registrada únicamente en memoria.
    /// </summary>
    public bool TryCompleteModuleLocally(string moduleId, string moduleTitle,
        AchievementDefinition achievement, int totalObjectives, float playTime, out string error)
    {
        EnsureSaveFile();
        string snapshot = JsonUtility.ToJson(saveFile);
        int previousSlot = SessionContext.ActiveSlotId;
        try
        {
            CompleteModuleLocally(moduleId, moduleTitle, achievement, totalObjectives, playTime);
            error = null;
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            saveFile = JsonUtility.FromJson<SaveFile>(snapshot);
            SessionContext.SelectSlot(previousSlot);
            error = "No se pudo guardar el progreso local. Reintenta antes de salir o reiniciar.";
            Debug.LogError($"Guardado de completitud fallido: {exception.Message}", this);
            return false;
        }
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
        if (IsSyncInProgress)
        {
            callback?.Invoke(false, "Ya hay una sincronizacion en curso. Espera la respuesta del servidor.");
            return;
        }
        if (!SessionContext.CanSyncToFirebase)
        {
            SetStatus(SyncState.Local, "Sesion local: progreso conservado en este dispositivo.");
            callback?.Invoke(false, StatusMessage);
            return;
        }
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetStatus(SyncState.Offline, "Sin conexion. Tu progreso local se conserva; conectate y pulsa Sincronizar.");
            callback?.Invoke(false, StatusMessage);
            return;
        }
        if (FirebaseSaveManager.Instance == null)
        {
            SetStatus(SyncState.Error, "Firebase no esta disponible. Tu progreso local se conserva.");
            callback?.Invoke(false, StatusMessage);
            return;
        }

        // Persistir antes de enviar. Nunca borrar pendientes antes de la confirmacion.
        try { SaveLocal(); }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            SetStatus(SyncState.Error, "No se pudo escribir el archivo local. No se inicio la sincronizacion.");
            callback?.Invoke(false, StatusMessage);
            return;
        }
        string uid = SessionContext.UserId;
        int generation = sessionGeneration;
        var originalSlots = new System.Collections.Generic.Dictionary<int, string>();
        foreach (var slot in saveFile.slots) originalSlots[slot.slotID] = JsonUtility.ToJson(slot);
        SaveFile snapshot = JsonUtility.FromJson<SaveFile>(JsonUtility.ToJson(saveFile));
        foreach (var slot in snapshot.slots) slot.needsCloudSync = false;
        IsSyncInProgress = true;
        SetStatus(SyncState.Syncing, "Sincronizando todas las partidas con Firebase...");
        StartCoroutine(ReportSlowSync(generation, ++syncOperation));
        FirebaseSaveManager.Instance.UploadSave(snapshot, (success, message) =>
        {
            IsSyncInProgress = false;
            // Una respuesta anterior nunca modifica la cuenta o sesion actual.
            if (generation != sessionGeneration || uid != SessionContext.UserId)
            {
                RefreshLocalStatus();
                return;
            }
            if (!success)
            {
                SetStatus(SyncState.Error, "No se pudo sincronizar. Tu progreso local sigue pendiente; puedes reintentar.");
                callback?.Invoke(false, StatusMessage);
                return;
            }
            string beforeAcknowledgement = JsonUtility.ToJson(saveFile);
            foreach (var slot in saveFile.slots)
                if (originalSlots.TryGetValue(slot.slotID, out string original) && original == JsonUtility.ToJson(slot))
                    slot.needsCloudSync = false;
            try { SaveLocal(); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                saveFile = JsonUtility.FromJson<SaveFile>(beforeAcknowledgement);
                SetStatus(SyncState.Error, "Firebase recibio la copia, pero fallo la confirmacion local. Se conservan los pendientes.");
                callback?.Invoke(false, StatusMessage);
                return;
            }
            SetStatus(HasPendingChanges ? SyncState.Pending : SyncState.Synced,
                HasPendingChanges ? "Copia sincronizada. Hay cambios posteriores pendientes de subir."
                    : "Todas las partidas sincronizadas con Firebase.");
            callback?.Invoke(true, StatusMessage);
        });
    }

    private System.Collections.IEnumerator ReportSlowSync(int generation, int operation)
    {
        yield return new WaitForSecondsRealtime(20f);
        if (IsSyncInProgress && generation == sessionGeneration && operation == syncOperation)
            SetStatus(SyncState.WaitingForServer,
                "Firebase tarda en responder. Tu progreso local esta guardado. La operacion sigue pendiente de confirmacion.");
    }

    public void SaveLocal()
    {
        EnsureSaveFile();
        string json = JsonUtility.ToJson(saveFile,true);
        Systems.Save.AtomicLocalFile.Write(savePath, json);
        if (!IsSyncInProgress) RefreshLocalStatus();
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
