using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class LogManager : MonoBehaviour
{
    private int logFileIndex = 0;
    private int logEventCount = 0;
    private const int MaxEventsPerFile = 1000; // �̺�Ʈ ���� ���� ������ ����

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

        // �ڱ� ���� ���� ���� ���� �߰�
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(action, settings);
        File.AppendAllText(filePath, json + "\n");
    }
}
