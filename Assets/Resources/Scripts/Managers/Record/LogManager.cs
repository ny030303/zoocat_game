using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

public class LogManager : MonoBehaviour
{
    private int logFileIndex = 0;
    private int logEventCount = 0;
    private const int MaxEventsPerFile = 1000;
    private string sessionID;
    private string folderPath;

    private void Awake()
    {
        sessionID = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 플랫폼에 따라 저장 경로 설정
        if (Application.isEditor)
        {
            folderPath = Application.dataPath + "/Resources/Logs"; // 에디터에서 저장
        }
        else
        {
            folderPath = Application.persistentDataPath + "/Logs"; // 안드로이드에서는 persistentDataPath 사용
        }

        // 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        Debug.Log("Log Folder Path: " + folderPath);
    }

    public void RecordAction(PlayerAction action)
    {
        if (logEventCount >= MaxEventsPerFile)
        {
            logFileIndex++;
            logEventCount = 0;
        }

        SaveActionToFile(action, logFileIndex);
        logEventCount++;
    }

    private void SaveActionToFile(PlayerAction action, int index)
    {
        string filePath = folderPath + $"/log_{sessionID}_{index}.json";

        // IL2CPP-friendly JsonSerializerSettings
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ContractResolver = new DefaultContractResolver() // AOT 친화적 설정
        };

        string json = JsonConvert.SerializeObject(action, settings);
        File.AppendAllText(filePath, json + "\n");

        Debug.Log($"Saved log to: {filePath}");
    }

    public List<PlayerAction> LoadActionsFromFile(string ymdhms, int index)
    {
        string filePath = folderPath + $"/log_{ymdhms}_{index}.json";
        List<PlayerAction> events = new List<PlayerAction>();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return events; // 빈 리스트 반환
        }

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                reader.SupportMultipleContent = true;

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    ContractResolver = new DefaultContractResolver()
                };

                JsonSerializer serializer = JsonSerializer.Create(settings);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        PlayerAction gameEvent = serializer.Deserialize<PlayerAction>(reader);
                        events.Add(gameEvent);
                    }
                }
            }
        }
        catch (JsonReaderException ex)
        {
            Debug.LogError($"Error reading JSON file: {ex.Message}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error reading file: {ex.Message}");
        }

        return events;
    }
}
