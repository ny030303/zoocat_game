using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class LogManager : MonoBehaviour
{
    private int logFileIndex = 0;
    private int logEventCount = 0;
    private const int MaxEventsPerFile = 1000; // 이벤트 수에 따라 파일을 분할
    private string sessionID;
    private string folderPath = Application.dataPath + "/Resources/Logs"; // Assets/Resources 폴더 내 Logs 폴더를 생성
    private void Start()
    {
        // Generate a unique session ID based on the current date and time
        sessionID = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
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
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // 폴더가 없으면 생성
        }

        string filePath = folderPath + $"/log_{sessionID}_{index}.json";

        //운영체제에 따라 설정된 특정 경로
        //string filePath = Application.persistentDataPath + $"/log_{sessionID}_{index}.json";

        // 자기 참조 루프 무시 설정 추가
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(action, settings);
        File.AppendAllText(filePath, json + "\n");
    }

    public List<PlayerAction> LoadActionsFromFile(string ymdhms, int index)
    {
        // Use the session ID when loading the file
        string filePath = folderPath + $"/log_{ymdhms}_{index}.json";
        List<PlayerAction> events = new List<PlayerAction>();

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                reader.SupportMultipleContent = true;

                JsonSerializer serializer = new JsonSerializer();

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
