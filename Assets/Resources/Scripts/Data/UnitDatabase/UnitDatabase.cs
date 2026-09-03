using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "scriptable Object/Create Unit Database")]
public class UnitDatabase : ScriptableObject
{
    // 로비 UI 작업용 (LobbyUserManager 등이 Resources 폴더에서 채움). 전투 로직은 아래 static 사용.
    public List<UnitData> unitDeck;
    public List<UnitData> aiUnitDeck;

    private const string PlayerUnitFolder = "Scripts/Data/UnitData/Unit_UnitData";
    private const string CharacterSheet = "Scripts/Data/Sheet/CharacterSheet";

    public static List<UnitData> unitList = new List<UnitData>();
    public static List<UnitData> aiUnitList = new List<UnitData>();

    /// 플레이어 유닛 = Unit_UnitData 폴더의 모든 .asset (프리팹/스프라이트) + CharacterSheet.csv 의 스탯(authoritative).
    /// AI/PvP 미러는 동일 세트를 쓴다.
    public void Initialize()
    {
        unitList.Clear();
        aiUnitList.Clear();

        var sheet = CSVLoader.LoadUnitData(CharacterSheet);           // id -> UnitData(스탯)
        var assets = Resources.LoadAll<UnitData>(PlayerUnitFolder);
        if (assets.Length == 0)
            Debug.LogError($"[UnitDatabase] {PlayerUnitFolder} 에 UnitData 에셋이 없습니다");

        foreach (var asset in assets)
        {
            UnitData u = asset.DeepCopy();       // id / unitPrefab / unitSprite 는 에셋
            MergeStatsFromSheet(u, sheet);       // 나머지 수치는 CSV
            unitList.Add(u);
            aiUnitList.Add(u.DeepCopy());        // 상대 미러용 동일 세트 (id 누락으로 인한 NRE 방지)
        }
    }

    /// CharacterSheet.csv 의 수치를 에셋 위에 덮어씀 (CSV 가 밸런스 소스).
    private static void MergeStatsFromSheet(UnitData u, Dictionary<string, UnitData> sheet)
    {
        string key = u.id.Replace("CHA_", "");
        if (!sheet.TryGetValue(key, out var s))
        {
            Debug.LogWarning($"[UnitDatabase] '{u.id}' 가 CharacterSheet.csv 에 없음 - 에셋 스탯 사용");
            return;
        }
        u.unitName = s.unitName;
        u.grade = s.grade;
        u.atk = s.atk; u.hit = s.hit; u.cri = s.cri;
        u.attackSpeed = s.attackSpeed; u.attackRange = s.attackRange; u.splashRange = s.splashRange;
        u.hp = s.hp; u.def = s.def; u.moveSpeed = s.moveSpeed;
        u.skillId = s.skillId; u.skillValue = s.skillValue;
        u.skillDuration = s.skillDuration; u.skillCooltime = s.skillCooltime;
    }

    public bool IsNull(string owner)  { 
        switch(owner)
        {
            case "player":
                return unitList != null;
            case "ai":
                return aiUnitList != null;
            default:
                return unitList != null;
        }
    }
    public int GetUnitListCount(string owner) {
        switch (owner) {
            case "player":
                return unitList.Count;
            case "ai":
                return aiUnitList.Count;
            default:
                return unitList.Count;
        }
        
    }

    public UnitData GetUnitDataToIdx(string owner, int idx) {
        switch (owner)
        {
            case "player":
                return unitList[idx];
            case "ai":
                return aiUnitList[idx];
            default:
                return unitList[idx];
        }
    }

    public UnitData GetUnitData(string owner, string id)
    {
        List<UnitData> targetList = null;

        switch (owner)
        {
            case "player":
                targetList = unitList;
                break;
            case "ai":
                targetList = aiUnitList;
                break;
            default:
                targetList = unitList;
                break;
        }

        if (targetList == null || targetList.Count == 0)
        {
            Debug.LogWarning("Target list is empty or null.");
            return null;
        }

        UnitData foundUnit = targetList.Find(unit => unit.id == id);

        if (foundUnit == null)
        {
            Debug.LogWarning($"No unit found with id: {id} in {owner}'s unit list.");
        }

        return foundUnit;
    }

    public UnitData GetUnitDataRandom(string owner)
    {
        if (unitList == null || unitList.Count == 0)
        {
            Debug.LogWarning("Unit list is empty or null.");
            return null;
        }

        int randomIndex = Random.Range(0, GetUnitListCount(owner));
        return unitList[randomIndex];
    }
}
