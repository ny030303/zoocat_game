using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardData
{
    public int id;
    public string type;
    public int amount;

    public RewardData(int id, string type, int amount)
    {
        this.id = id;
        this.type = type;
        this.amount = amount;
    }
}

public static class RewardLoader
{
    public static Dictionary<int, RewardData> LoadRewardData(string fileName)
    {
        Dictionary<int, RewardData> rewardDictionary = new Dictionary<int, RewardData>();

        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csvFile == null)
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {fileName}");
                return rewardDictionary;
            }

            string[] lines = csvFile.text.Split('\n');
            for (int i = 1; i < lines.Length; i++) // 첫 줄(헤더) 제외
            {
                string[] values = lines[i].Trim().Split(',');
                if (values.Length < 3) continue;

                int id = int.Parse(values[0]);
                string type = values[1];
                int amount = int.Parse(values[2]);

                rewardDictionary[id] = new RewardData(id, type, amount);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load Reward CSV: {ex.Message}");
        }

        return rewardDictionary;
    }
}
