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

    public PlayerProgress data;
}

[Serializable]
public class PlayerProgress
{
    public int level;
    public float progress;
}

[System.Serializable]
public class SaveFile
{
    public System.Collections.Generic.List<SaveSlot> slots = new System.Collections.Generic.List<SaveSlot>();
}