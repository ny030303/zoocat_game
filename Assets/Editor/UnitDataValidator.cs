#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// UnitData 에셋 ↔ CharacterSheet.csv ↔ unitPrefab 교차 검증.
/// 스탯은 CSV 가 소스이므로, 에셋만 있고 CSV 행이 없으면 그 유닛 스탯은 미정의.
/// 메뉴: <b>Tools ▸ Validate ▸ Unit Data</b>
/// </summary>
public static class UnitDataValidator
{
    const string PlayerFolder = "Scripts/Data/UnitData/Unit_UnitData";
    const string EnemyFolder = "Scripts/Data/UnitData/Enemy_UnitData";
    const string Sheet = "Scripts/Data/Sheet/CharacterSheet";

    [MenuItem("Tools/Validate/Unit Data")]
    public static void Validate()
    {
        var sheet = CSVLoader.LoadUnitData(Sheet);                       // key: raw id
        var players = Resources.LoadAll<UnitData>(PlayerFolder);
        var enemies = Resources.LoadAll<UnitData>(EnemyFolder);

        int errors = 0, warns = 0;
        void Err(string m) { Debug.LogError("[UnitValidate] " + m); errors++; }
        void Warn(string m) { Debug.LogWarning("[UnitValidate] " + m); warns++; }

        string Norm(string id) => (id ?? "").Replace("CHA_", "").Trim();
        var sheetIds = new HashSet<string>(sheet.Keys.Select(Norm));

        if (sheet.Count == 0) Err("CharacterSheet.csv 를 못 읽음 (또는 비어 있음)");
        if (players.Length == 0) Err($"{PlayerFolder} 에 UnitData 에셋 없음");

        void CheckAssets(UnitData[] assets, string label)
        {
            var seen = new HashSet<string>();
            foreach (var a in assets)
            {
                string nid = Norm(a.id);
                if (string.IsNullOrEmpty(nid)) { Err($"{label} '{a.name}': id 비어 있음"); continue; }
                if (!seen.Add(nid)) Err($"{label}: id '{nid}' 중복 에셋");

                if (!sheetIds.Contains(nid))
                    Err($"{label} '{nid}': CharacterSheet.csv 에 행이 없음 → 스탯 미정의");

                if (a.unitPrefab == null) Err($"{label} '{nid}': unitPrefab 미지정");
            }
        }

        CheckAssets(players, "Player");
        CheckAssets(enemies, "Enemy");

        // CSV 행이 있는데 에셋이 없는 경우 (스폰 불가)
        var assetIds = new HashSet<string>(players.Concat(enemies).Select(a => Norm(a.id)));
        foreach (var sid in sheetIds)
            if (!assetIds.Contains(sid))
                Warn($"CharacterSheet id '{sid}': 대응 UnitData 에셋 없음 (스폰 불가 - 데이터 전용?)");

        string summary = $"CSV {sheet.Count} · Player 에셋 {players.Length} · Enemy 에셋 {enemies.Length}  →  에러 {errors}, 경고 {warns}";
        if (errors > 0) Debug.LogError("[UnitValidate] " + summary);
        else Debug.Log("[UnitValidate] " + summary + (warns == 0 ? "  ✅ 이상 없음" : ""));

        EditorUtility.DisplayDialog("Unit Data 검증",
            summary + (errors > 0 ? "\n\n❌ Console 에서 에러 확인"
                     : warns > 0 ? "\n\n⚠️ 경고 있음 (Console)"
                     : "\n\n✅ 이상 없음"),
            "확인");
    }
}
#endif
