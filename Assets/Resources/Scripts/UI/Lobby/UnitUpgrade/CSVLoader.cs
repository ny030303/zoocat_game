using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnitUpgradeData
{
    public string num;
    public int grade;
    public int level;
    public int cost;
    public int atkStatGrowth;

    public UnitUpgradeData(string num, int grade, int level, int cost, int atkStatGrowth)
    {
        this.num = num;
        this.grade = grade;
        this.level = level;
        this.cost = cost;
        this.atkStatGrowth = atkStatGrowth;
    }
}
public static class CSVLoader
{
    public static Dictionary<(int, int), UnitUpgradeData> LoadUpgradeData(string fileName) // 경로 대신 파일명 사용
    {
        Dictionary<(int, int), UnitUpgradeData> upgradeData = new Dictionary<(int, int), UnitUpgradeData>();

        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName.Replace(".csv", "")); // 🔄 확장자 제거 후 로드
            if (csvFile == null)
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {fileName}");
                return upgradeData;
            }

            string[] lines = csvFile.text.Split('\n'); // 🔄 파일을 읽는 대신 text 속성 사용

            for (int i = 1; i < lines.Length; i++)  // 첫 줄은 헤더라서 제외
            {
                string[] values = lines[i].Trim().Split(',');

                if (values.Length < 5) // 🔄 5개 컬럼 확인 (id, grade, level, cost, atkStatGrowth)
                {
                    Debug.LogWarning($"Invalid data at line {i + 1}");
                    continue;
                }

                int grade = int.Parse(values[1]);
                int level = int.Parse(values[2]);
                int cost = int.Parse(values[3]);
                int atkStatGrowth = int.Parse(values[4]);

                var key = (grade, level); // 🔄 튜플 키 생성

                if (!upgradeData.ContainsKey(key))
                {
                    upgradeData[key] = new UnitUpgradeData(values[0], grade, level, cost, atkStatGrowth);
                }
                else
                {
                    Debug.LogWarning($"Duplicate entry for grade {grade}, level {level} at line {i + 1}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load CSV file: {ex.Message}");
        }

        return upgradeData;
    }

    public static Dictionary<string, UnitData> LoadUnitData(string fileName)
    {
        Dictionary<string, UnitData> unitDataDictionary = new Dictionary<string, UnitData>();

        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csvFile == null)
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {fileName}");
                return unitDataDictionary;
            }

            string[] lines = csvFile.text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // 첫 줄(헤더) 제외
            {
                string[] values = lines[i].Trim().Split(',');

                if (values.Length < 16 || string.IsNullOrEmpty(values[0])) // 데이터 개수 확인
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                        Debug.LogWarning($"[CharacterSheet] line {i + 1} 열 부족({values.Length}/16) - 건너뜀");
                    continue;
                }

                try
                {
                    UnitData unitData = ScriptableObject.CreateInstance<UnitData>();

                    unitData.id = values[0];
                    unitData.unitName = values[1];
                    unitData.grade = int.Parse(values[2]);
                    unitData.atk = int.Parse(values[3]);
                    unitData.hit = (int)(int.Parse(values[4].Replace("%", "")) / 100f);
                    unitData.cri = float.Parse(values[5].Replace("%", "")) / 100f; // 100% -> 1.0 변환
                    unitData.attackSpeed = float.Parse(values[6]);
                    unitData.attackRange = float.Parse(values[7]);
                    unitData.splashRange = float.Parse(values[8]);
                    unitData.hp = int.Parse(values[9]);
                    unitData.def = int.Parse(values[10]);
                    unitData.moveSpeed = float.Parse(values[11]);
                    unitData.skillId = values[12];
                    unitData.skillValue = float.Parse(values[13]);
                    unitData.skillDuration = float.Parse(values[14]);
                    unitData.skillCooltime = float.Parse(values[15]);

                    if (!unitDataDictionary.ContainsKey(unitData.id))
                    {
                        unitDataDictionary[unitData.id] = unitData;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate entry found for unit ID {unitData.id} at line {i + 1}");
                    }
                } catch (Exception ex)
                {
                    Debug.LogError($"[CharacterSheet] line {i + 1} 파싱 실패: {ex.Message} | {lines[i]}");
                }

            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"1Failed to load CSV file: {ex.Message}");
        }

        return unitDataDictionary;
    }


    // 웨이브 로더
    public static Dictionary<int, WaveData> LoadWaveData(string fileName)
    {
        Dictionary<int, WaveData> waveDictionary = new Dictionary<int, WaveData>();

        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csvFile == null)
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {fileName}");
                return waveDictionary;
            }

            string[] lines = csvFile.text.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Trim().Split(',');
                if (values.Length < 6) continue;

                int id = int.Parse(values[0]);
                float timeLimit = float.Parse(values[1]);
                int hpAdditional = int.Parse(values[2]);
                int spdAdditional = int.Parse(values[3]);
                int rewardId = int.Parse(values[4]);
                int waveGroupId = int.Parse(values[5]);

                WaveData waveData = new WaveData(id, timeLimit, hpAdditional, spdAdditional, rewardId, waveGroupId);
                waveDictionary[id] = waveData;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load Wave CSV: {ex.Message}");
        }

        return waveDictionary;
    }

    public static Dictionary<int, WaveGroupData> LoadWaveGroupData(string fileName)
    {
        Dictionary<int, WaveGroupData> waveGroupDictionary = new Dictionary<int, WaveGroupData>();

        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName.Replace(".csv", ""));
            if (csvFile == null)
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {fileName}");
                return waveGroupDictionary;
            }

            string[] lines = csvFile.text.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Trim().Split(',');
                if (values.Length < 4) continue;

                int id = int.Parse(values[0]);
                bool isBoss = values[1] == "1";
                List<string> monsterIds = new List<string>();
                List<int> monsterCounts = new List<int>();

                for (int j = 2; j < values.Length; j += 2)
                {
                    if (!string.IsNullOrEmpty(values[j]) && !string.IsNullOrEmpty(values[j + 1]))
                    {
                        monsterIds.Add(values[j]);
                        monsterCounts.Add(int.Parse(values[j + 1]));
                    }
                }

                WaveGroupData waveGroupData = new WaveGroupData(id, isBoss, monsterIds, monsterCounts);
                waveGroupDictionary[id] = waveGroupData;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load WaveGroup CSV: {ex.Message}");
        }

        return waveGroupDictionary;
    }
}