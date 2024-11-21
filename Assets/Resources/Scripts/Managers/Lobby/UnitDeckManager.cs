using UnityEngine;
using System.Collections.Generic;

public class UnitDeckManager : MonoBehaviour
{
    public UnitDatabase unitDatabase; // ScriptableObject 연결
    public void Initialize()
    {
        // 유저 데이터 로드
        //UserData userdata = UserManager.Instance.currentUser;
        //Debug.Log("Selected Units[0]: " + userdata.selectedUnits[0]);

        // 유닛 정보를 업데이트
        int[] selectedUnits = { 1001, 1002, 1003, 1004, 1005 };
        //UpdateUnitImages(selectedUnits);
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
