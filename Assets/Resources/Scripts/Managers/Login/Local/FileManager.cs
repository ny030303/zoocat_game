using LitJson;
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
            string jsonData = JsonMapper.ToJson(data);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Data saved successfully to {filePath}. Key: {key}, Value: {value}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data. Exception: {ex.Message}");
        }
    }
    public static void SaveUserData(UserData userData)
    {
        GameData data = LoadData() ?? new GameData();
        data.userData = userData; // 유저 정보 저장

        try
        {
            string jsonData = JsonMapper.ToJson(data);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"UserData saved successfully to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save UserData. Exception: {ex.Message}");
        }
    }


    //유닛 데이터 저장하는 SaveUnits() 함수 추가
    public static void SaveUnits(UserUnit[] unitList)
    {
        GameData data = LoadData() ?? new GameData();

        if (unitList != null && unitList.Length > 0)
        {
            data.units = unitList; // 유닛 데이터 업데이트
            Debug.Log($"Saved {unitList.Length} units.");
        }
        else
        {
            Debug.LogWarning("Unit list is empty. Skipping save operation.");
            return;
        }

        try
        {
            string jsonData = JsonMapper.ToJson(data);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Unit data saved successfully to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save unit data. Exception: {ex.Message}");
        }
    }
    private static void SaveDataToFile(GameData data)
    {
        try
        {
            string jsonData = JsonMapper.ToJson(data);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Data saved successfully to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data. Exception: {ex.Message}");
        }
    }

    public static GameData LoadData()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"File not found: {filePath}. Creating default data...");
            GameData defaultData = new GameData();
            SaveDataToFile(defaultData);
            return defaultData;
        }

        try
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonMapper.ToObject<GameData>(jsonData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load data. Exception: {ex.Message}");
            return new GameData();
        }
    }


    public static UserData LoadUserData()
    {
        GameData data = LoadData();

        if (data != null && data.userData != null)
        {
            Debug.Log($"UserData loaded. Username: {data.userData.username}, Level: {data.userData.level}");
            return data.userData;
        }

        Debug.LogWarning("No UserData found in saved data.");
        return new UserData(); // 기본값 반환
    }

    //유닛 데이터 로드하는 LoadUnits() 함수 추가
    public static UserUnit[] LoadUnits()
    {
        GameData data = LoadData();

        if (data != null && data.units != null)
        {
            Debug.Log($"Loaded {data.units.Length} units.");
            return data.units;
        }

        Debug.LogWarning("No units found in saved data.");
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
    public UserUnit[] units = { }; // 유닛 데이터 저장
    public UserData userData = new UserData(); // 유저 데이터
}
