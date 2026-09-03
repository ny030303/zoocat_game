using System;
using System.Collections.Generic;
using UnityEngine;

/// 보상 = (아이템 id, 개수) 쌍들. Reward.csv: reward_id, item_id, count, item2, item2_count …
public struct RewardEntry
{
    public int itemId;
    public int count;
    public RewardEntry(int itemId, int count) { this.itemId = itemId; this.count = count; }
}

public class RewardData
{
    public int id;
    public List<RewardEntry> entries;

    public RewardData(int id, List<RewardEntry> entries)
    {
        this.id = id;
        this.entries = entries ?? new List<RewardEntry>();
    }
}

public static class RewardLoader
{
    public static Dictionary<int, RewardData> LoadRewardData(string fileName)
    {
        var dict = new Dictionary<int, RewardData>();
        try
        {
            TextAsset csv = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csv == null) { Debug.LogError($"CSV 없음: {fileName}"); return dict; }

            string[] lines = csv.text.Split('\n');
            for (int i = 1; i < lines.Length; i++)               // 0행 = 헤더
            {
                string[] v = lines[i].Trim().Split(',');
                if (v.Length < 3 || string.IsNullOrEmpty(v[0])) continue;

                int id = int.Parse(v[0]);
                var entries = new List<RewardEntry>();
                // (item_id, count) 쌍을 끝까지
                for (int c = 1; c + 1 < v.Length; c += 2)
                {
                    if (string.IsNullOrEmpty(v[c]) || string.IsNullOrEmpty(v[c + 1])) continue;
                    if (int.TryParse(v[c], out int itemId) && int.TryParse(v[c + 1], out int cnt) && cnt != 0)
                        entries.Add(new RewardEntry(itemId, cnt));
                }
                dict[id] = new RewardData(id, entries);
            }
        }
        catch (Exception ex) { Debug.LogError($"Failed to load Reward CSV: {ex.Message}"); }
        return dict;
    }
}
