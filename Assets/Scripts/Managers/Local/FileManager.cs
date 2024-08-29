using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileManager
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "GuestGameData.json");

    public static void SaveData(string key, string value)
    {
        GameData data = LoadData() ?? new GameData();

        if (!string.IsNullOrEmpty(value))
        {
            data.dataDictionary[key] = value;
            Debug.Log($"Key: {key} added with Value: {value} to dataDictionary.");
        }
        else
        {
            Debug.LogWarning($"Value for Key: {key} is null or empty. Skipping save operation.");
            return;
        }

        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Data saved successfully to {filePath}. Key: {key}, Value: {value}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data. Exception: {ex.Message}");
        }

        if (File.Exists(filePath))
        {
            Debug.Log($"File exists at {filePath}.");
        }
        else
        {
            Debug.LogError($"File not found at {filePath} after saving attempt.");
        }
    }

    public static GameData LoadData()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<GameData>(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data. Exception: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"File not found: {filePath}");
        }
        return null;
    }

    public static void DeleteDataFile()
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"File deleted successfully: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete file. Exception: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"No file to delete at: {filePath}");
        }
    }
}

[Serializable]
public class GameData
{
    public Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
}
