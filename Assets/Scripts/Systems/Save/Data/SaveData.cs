using System;
using System.Collections.Generic;

[Serializable]
public class SaveSlot
{
    public int slotID;
    public string moduleTitle;
    public string lastSave;
    public float playTime;
    public string screenshot;
    public bool needsCloudSync;

    public PlayerProgress data;
}

[Serializable]
public class PlayerProgress
{
    public int level;
    public float progress;
    public List<string> completedModuleIds = new List<string>();
    public List<AchievementProgress> achievements = new List<AchievementProgress>();
    public List<string> completedTutorialIds = new List<string>();
    public List<ModuleProgress> modules = new List<ModuleProgress>();
}

[Serializable]
public class ModuleProgress
{
    public string moduleId;
    public string moduleName;
    public bool completed;
    public int completedObjectives;
    public int totalObjectives;
    public float playTime;
    public string completedAt;
}

[Serializable]
public class AchievementProgress
{
    public string achievementId;
    public string unlockedAt;
}

[System.Serializable]
public class SaveFile
{
    public int activeSlotId = -1;
    public System.Collections.Generic.List<SaveSlot> slots = new System.Collections.Generic.List<SaveSlot>();
}
