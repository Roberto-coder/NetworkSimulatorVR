using UnityEngine;
using System.IO;
using System;


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

            savePath = Application.persistentDataPath + "/save.json";
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
            string path = Application.persistentDataPath + "/save.json";

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
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error en LoadFromLocal: " + e);
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

        slot.data = new PlayerProgress();
        slot.data.level = 1;
        slot.data.progress = 0.5f;

        if(saveFile.slots.Count < 4)
        {
            saveFile.slots.Add(slot);
        }
        else
        {
            saveFile.slots[slotID] = slot;
        }

        SaveLocal();

        // Firebase manual
        FirebaseSaveManager.Instance.UploadSave(saveFile);
    }

    public void SaveLocal()
    {
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
}