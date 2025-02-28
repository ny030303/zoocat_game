using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyListLoader : MonoBehaviour
{
    // Singleton 인스턴스
    public static EnemyListLoader Instance { get; private set; }

    public List<UnitData> enemyList = new List<UnitData>();

    private void Awake()
    {
        // Singleton 초기화
        if (Instance == null)
        {
            Instance = this;
            Instance.LoadCharacterData();
            Debug.Log($"Loaded {enemyList.Count} enemys.");
            DontDestroyOnLoad(gameObject); // 다른 씬에서도 유지하려면 추가
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

    }
    public UnitData GetEnemyPrefabToId(string id)
    {
        UnitData data = enemyList.Find(enemy => enemy.id.Replace("CHA_", "").Equals(id));
        return data;
    }
    private void LoadCharacterData()
    {
        UnitData[] loadedData = Resources.LoadAll<UnitData>("Scripts/Data/UnitData/Enemy_UnitData");

        if (loadedData != null)
        {
            enemyList.AddRange(loadedData);
        }
    }
}
