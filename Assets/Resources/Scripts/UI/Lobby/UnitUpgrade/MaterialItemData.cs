using System;
using System.Collections.Generic;
using UnityEngine;

/// MaterialItem.csv: id, _name, item_sub_type, grade
///  item_sub_type: "MoneyMaterial"(재화) / "NormalMaterial"(일반 재료) …
public class MaterialItemData
{
    public int id;
    public string name;
    public string subType;
    public int grade;

    public MaterialItemData(int id, string name, string subType, int grade)
    {
        this.id = id;
        this.name = name;
        this.subType = subType;
        this.grade = grade;
    }

    public bool IsCurrency => subType == "MoneyMaterial";
}

public static class MaterialItemLoader
{
    // MaterialItem.csv 의 재화 아이템 id (편의 상수)
    public const int GoldItemId = 100001;  // 무료 재화
    public const int GemItemId = 100002;   // 유료 재화

    public static Dictionary<int, MaterialItemData> Load(string fileName = "Scripts/Data/Sheet/MaterialItem")
    {
        var dict = new Dictionary<int, MaterialItemData>();
        try
        {
            TextAsset csv = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csv == null) { Debug.LogError($"CSV 없음: {fileName}"); return dict; }

            string[] lines = csv.text.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] v = lines[i].Trim().Split(',');
                if (v.Length < 4 || string.IsNullOrEmpty(v[0])) continue;
                int id = int.Parse(v[0]);
                int.TryParse(v[3], out int grade);
                dict[id] = new MaterialItemData(id, v[1], v[2], grade);
            }
        }
        catch (Exception ex) { Debug.LogError($"Failed to load MaterialItem CSV: {ex.Message}"); }
        return dict;
    }
}
