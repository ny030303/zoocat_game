using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "scriptable Object/Create Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public List<UnitData> unitDeck;
    public List<UnitData> aiUnitDeck;

    public static List<UnitData> unitList = new List<UnitData>();
    public static List<UnitData> aiUnitList = new List<UnitData>();

    public void Initialize()
    {
        foreach (var unit in unitDeck)  { unitList.Add(unit.DeepCopy()); }
        foreach (var aiunit in aiUnitDeck) {
            UnitData tmp = aiunit.DeepCopy();
            //Dictionary<string, UnitData> baseUnitData;  // 기본값 유닛 데이터
            //string sheetCsvFilePath = "Scripts/Data/Sheet/CharacterSheet"; // CSV 파일 경로
            //baseUnitData = CSVLoader.LoadUnitData(sheetCsvFilePath);
            //Debug.Log(tmp.id);
            //baseUnitData.TryGetValue(tmp.id.Replace("CHA_", ""), out UnitData foundUnit);

            //// 기존 baseAtk 값을 유지하고 10% 증가 방식으로 atk 재계산
            //int baseAtk = foundUnit.atk; // 처음 설정된 기본 공격력
            //tmp.atk = baseAtk;
            aiUnitList.Add(tmp); 
        }
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
