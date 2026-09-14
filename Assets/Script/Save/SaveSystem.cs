using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SavePath =
        Path.Combine(Application.persistentDataPath, "savegame.json");

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void Save(SaveData data)
    {
        data.savedAtUtc = DateTime.UtcNow.ToString("o");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData Load()
    {
        if (!HasSave())
            return null;

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave()
    {
        if (HasSave())
            File.Delete(SavePath);
    }
}
