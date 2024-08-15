using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "scriptable Object/Create Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public List<UnitData> unitDeck;

    public static List<UnitData> unitList = new List<UnitData>();

    public void Initialize()
    {

        foreach (var unit in unitDeck)
        {
            unitList.Add(unit.DeepCopy());
            //unit.Initialize();
        }
    }

    public bool IsNull()
    {
        return unitList != null;
    }
    public int GetUnitListCount()
    {
        return unitList.Count;
    }

    public UnitData GetUnitDataToIdx(int idx)
    {
        return unitList[idx];
    }

    public UnitData GetUnitData(string unitName)
    {
        return unitList.Find(unit => unit.unitName == unitName);
    }

    public UnitData GetUnitDataRandom()
    {
        if (unitList == null || unitList.Count == 0)
        {
            Debug.LogWarning("Unit list is empty or null.");
            return null;
        }

        int randomIndex = Random.Range(0, GetUnitListCount());
        return unitList[randomIndex];
    }

    public GameObject[] GetUnitPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();

        foreach (var unit in unitList)
        {
            if (unit.unitPrefab != null)
            {
                prefabs.Add(unit.unitPrefab);
            }
        }

        return prefabs.ToArray();
    }
}
