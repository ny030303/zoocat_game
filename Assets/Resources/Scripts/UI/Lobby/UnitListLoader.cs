using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitListLoader : MonoBehaviour
{
    // Singleton 인스턴스
    public static UnitListLoader Instance { get; private set; }

    public List<UnitData> unitList = new List<UnitData>();

    private void Awake()
    {
        // Singleton 초기화
        if (Instance == null)
        {
            Instance = this;
            Instance.LoadCharacterData();
            Debug.Log($"Loaded {unitList.Count} characters.");
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

    private void LoadCharacterData()
    {
        UnitData[] loadedData = Resources.LoadAll<UnitData>("Scripts/Data/UnitData/Unit_UnitData");

        if (loadedData != null)
        {
            unitList.AddRange(loadedData);
        }
    }
}
