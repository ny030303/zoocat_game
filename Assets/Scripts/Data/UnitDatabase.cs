using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "scriptable Object/Create Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public List<UnitData> unitList;

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

        int randomIndex = Random.Range(0, unitList.Count);
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
