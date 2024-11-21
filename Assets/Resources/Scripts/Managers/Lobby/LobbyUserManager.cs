using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUserManager : MonoBehaviour
{
    public UnitDatabase unitDatabase;

    void Awake()
    {
        // 유저 데이터 로드
        UserData userdata = UserManager.Instance.currentUser;

        // selectedUnits 데이터를 기반으로 덱 초기화
        InitializeUnitDeck(userdata.selectedUnits);
    }

    // 유닛 덱 초기화
    private void InitializeUnitDeck(int[] selectedUnitIds)
    {
        // 덱 초기화
        unitDatabase.unitDeck.Clear();

        foreach (int unitId in selectedUnitIds)
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

    // 유닛 추가
    public void AddUnitToDeck(UnitData unit)
    {
        if (!unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Add(unit);
            Debug.Log($"{unit.unitName}이(가) 덱에 추가되었습니다.");
        }
        else
        {
            Debug.LogWarning($"{unit.unitName}은(는) 이미 덱에 존재합니다.");
        }
    }

    // 유닛 제거
    public void RemoveUnitFromDeck(UnitData unit)
    {
        if (unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Remove(unit);
            Debug.Log($"{unit.unitName}이(가) 덱에서 제거되었습니다.");
        }
        else
        {
            Debug.LogWarning($"{unit.unitName}은(는) 덱에 존재하지 않습니다.");
        }
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
