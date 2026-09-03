#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Wave.csv / WaveGroup.csv / Reward.csv 를 서로, 그리고 Enemy UnitData 와 대조 검증.
/// CSV 컬럼 밀림 · 존재하지 않는 몬스터/보상/그룹 id · 음수 값 등을 잡는다.
/// 메뉴: <b>Tools ▸ Validate ▸ Wave Data</b>
/// </summary>
public static class WaveDataValidator
{
    [MenuItem("Tools/Validate/Wave Data")]
    public static void Validate()
    {
        var waves = CSVLoader.LoadWaveData("Scripts/Data/Sheet/Wave");
        var groups = CSVLoader.LoadWaveGroupData("Scripts/Data/Sheet/WaveGroup");
        var rewards = RewardLoader.LoadRewardData("Scripts/Data/Sheet/Reward");
        var items = MaterialItemLoader.Load();

        var enemies = Resources.LoadAll<UnitData>("Scripts/Data/UnitData/Enemy_UnitData");
        var enemyIds = new HashSet<string>(enemies.Select(e => e.id.Replace("CHA_", "")));

        int errors = 0, warns = 0;
        void Err(string m) { Debug.LogError("[WaveValidate] " + m); errors++; }
        void Warn(string m) { Debug.LogWarning("[WaveValidate] " + m); warns++; }

        if (waves.Count == 0) Warn("Wave.csv 가 비어 있음 (무한 폴백만 동작)");
        if (groups.Count == 0) Err("WaveGroup.csv 가 비어 있음");
        if (enemies.Length == 0) Err("Enemy UnitData 없음 (Resources/Scripts/Data/UnitData/Enemy_UnitData)");

        // 웨이브 id 1부터 연속인지 (무한 연장은 마지막 authored 기준이라 gap 이 있으면 의도치 않은 동작)
        var ids = waves.Keys.OrderBy(k => k).ToList();
        for (int i = 0; i < ids.Count; i++)
            if (ids[i] != i + 1) { Warn($"Wave id 가 1부터 연속이 아님: [{string.Join(",", ids)}]"); break; }

        foreach (var w in waves.Values)
        {
            if (w.timeLimit <= 0) Err($"Wave {w.id}: timeLimit <= 0 ({w.timeLimit})");
            if (!groups.ContainsKey(w.waveGroupId)) Err($"Wave {w.id}: waveGroupId {w.waveGroupId} 가 WaveGroup.csv 에 없음");
            if (w.rewardId != 0 && !rewards.ContainsKey(w.rewardId)) Err($"Wave {w.id}: rewardId {w.rewardId} 가 Reward.csv 에 없음");
        }

        foreach (var g in groups.Values)
        {
            if (g.monsterIds.Count == 0) Err($"WaveGroup {g.id}: 몬스터가 없음");
            if (g.monsterIds.Count != g.monsterCounts.Count)
                Err($"WaveGroup {g.id}: id/count 개수 불일치 ({g.monsterIds.Count}/{g.monsterCounts.Count}) - CSV 컬럼 확인");
            for (int i = 0; i < g.monsterIds.Count; i++)
            {
                if (!enemyIds.Contains(g.monsterIds[i]))
                    Err($"WaveGroup {g.id}: 몬스터 id '{g.monsterIds[i]}' 가 Enemy UnitData 에 없음");
                if (i < g.monsterCounts.Count && g.monsterCounts[i] <= 0)
                    Err($"WaveGroup {g.id}: '{g.monsterIds[i]}' count <= 0 ({g.monsterCounts[i]})");
            }
        }

        foreach (var r in rewards.Values)
        {
            if (r.entries.Count == 0) Warn($"Reward {r.id}: 지급 항목 없음");
            foreach (var e in r.entries)
            {
                if (!items.ContainsKey(e.itemId))
                    Err($"Reward {r.id}: item_id {e.itemId} 가 MaterialItem.csv 에 없음");
                if (e.count <= 0) Warn($"Reward {r.id}: item {e.itemId} count <= 0 ({e.count})");
            }
        }

        foreach (var e in enemies)
            if (e.unitPrefab == null) Warn($"Enemy '{e.id}': unitPrefab 미지정");

        string summary = $"Wave {waves.Count} · Group {groups.Count} · Reward {rewards.Count} · Item {items.Count} · Enemy {enemies.Length}  →  에러 {errors}, 경고 {warns}";
        if (errors > 0) Debug.LogError("[WaveValidate] " + summary);
        else Debug.Log("[WaveValidate] " + summary + (warns == 0 ? "  ✅ 이상 없음" : ""));

        EditorUtility.DisplayDialog("Wave Data 검증",
            summary + (errors > 0 ? "\n\n❌ Console 에서 에러 확인"
                     : warns > 0 ? "\n\n⚠️ 경고 있음 (Console)"
                     : "\n\n✅ 이상 없음"),
            "확인");
    }
}
#endif
