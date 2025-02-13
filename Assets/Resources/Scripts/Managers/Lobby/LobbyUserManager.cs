using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUserManager : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    // 덱 변경 이벤트 추가
    public Action<List<UnitData>> OnUnitDeckChanged;

    void Awake()
    {
        if (UserManager.Instance == null)
        {
            Debug.LogWarning("UserManager.Instance is null!");
            return;
        }

        // 유저 데이터 로드
        UserData userdata = UserManager.Instance.currentUser;
        if (userdata != null)
        {
            // selectedUnits 데이터를 기반으로 덱 초기화
            InitializeUnitDeck(userdata.selectedUnits);
        }
        else
        {
            Debug.LogWarning("userdata is null!");
        }
    }

    public void AddUnitToDeck(UnitData unit)
    {
        if (!unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Add(unit);
            OnUnitDeckChanged?.Invoke(unitDatabase.unitDeck); // 이벤트 호출
        }
    }

    public void RemoveUnitFromDeck(UnitData unit)
    {
        if (unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Remove(unit);
            OnUnitDeckChanged?.Invoke(unitDatabase.unitDeck); // 이벤트 호출
        }
    }

    // 유닛 덱 초기화
    private void InitializeUnitDeck(string[] selectedUnitIds)
    {
        // 덱 초기화
        unitDatabase.unitDeck.Clear();

        foreach (string unitId in selectedUnitIds)
        {
            // ScriptableObject 경로 설정
            string unitPath = $"Scripts/Data/UnitData/Unit_UnitData/CHA_{unitId:D4}";
            UnitData unit = Resources.Load<UnitData>(unitPath);

            if (unit != null)
            {
                unitDatabase.unitDeck.Add(unit);
                Debug.Log($"{unit.unitName}이(가) 덱에 추가되었습니다.");
            }
            else
            {
                Debug.LogWarning($"유닛 {unitId}을(를) 로드할 수 없습니다. 경로: {unitPath}");
            }
        }

        Debug.Log("유닛 덱 초기화 완료");
        PrintUnitDeck(); // 초기화 후 결과 출력
    }

    // 덱 초기화
    public void ClearUnitDeck()
    {
        unitDatabase.unitDeck.Clear();
        Debug.Log("덱이 초기화되었습니다.");
    }

    // 덱 교체
    public void ReplaceUnitDeck(List<UnitData> newDeck)
    {
        unitDatabase.unitDeck = newDeck;
        Debug.Log("새로운 덱으로 교체되었습니다.");
    }

    // 덱 출력 (디버깅용)
    public void PrintUnitDeck()
    {
        Debug.Log("현재 유닛 덱:");
        foreach (var unit in unitDatabase.unitDeck)
        {
            Debug.Log($"- {unit.unitName}");
        }
    }
}
