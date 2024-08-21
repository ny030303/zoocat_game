using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class LogManager : MonoBehaviour
{
    private int logFileIndex = 0;
    private int logEventCount = 0;
    private const int MaxEventsPerFile = 1000; // 이벤트 수에 따라 파일을 분할

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
        string filePath = Application.persistentDataPath + $"/log_{index}.json";

        // 자기 참조 루프 무시 설정 추가
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(action, settings);
        File.AppendAllText(filePath, json + "\n");
    }
}
