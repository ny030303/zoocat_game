using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUpdater : MonoBehaviour
{
    public GameObject unitTemplate; // 캐릭터 Prefab
    public GameObject unitContainer; // 캐릭터 컨테이너(부모)
    public Sprite defaultSprite; // 기본 프로필 이미지
    public int ishave; // 보유 여부 (1: 보유, 0: 미보유)

    public List<UnitData> unitList; // 게임의 모든 유닛 데이터
    private UserUnit[] userUnitList; // 유저가 보유한 유닛 목록

    void Start()
    {
        StartCoroutine(InitializeInventory());
    }

    IEnumerator InitializeInventory()
    {
        // 데이터 로드가 완료될 때까지 대기
        while (UserManager.Instance == null || UserManager.Instance.units == null || UserManager.Instance.units.Length == 0 ||
               UnitListLoader.Instance == null || UnitListLoader.Instance.unitList == null)
        {
            yield return null;
        }

        // 데이터 할당
        unitList = UnitListLoader.Instance.unitList;
        userUnitList = UserManager.Instance.units;

        Debug.Log("유닛 데이터 및 사용자 유닛 로드 완료");
        GenerateCharacterList();
    }

    void GenerateCharacterList()
    {
        if (unitList == null || userUnitList == null)
        {
            Debug.LogWarning("유닛 데이터가 아직 로드되지 않았습니다.");
            return;
        }

        // 기존 유닛 UI 제거
        foreach (Transform child in unitContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (UnitData unit in unitList)
        {
            string unitIdWithoutPrefix = unit.id.Replace("CHA_", ""); // "CHA_" 제거

            UserUnit matchedUserUnit = userUnitList.FirstOrDefault(u => u.id == unitIdWithoutPrefix);

            if (matchedUserUnit != null)
            {
                Debug.Log($"매칭된 유닛: {matchedUserUnit.id}");
            }
            else
            {
                Debug.Log($"유저가 해당 유닛({unit.id})을 보유하지 않음");
                continue;
            }

            //UserUnit matchedUserUnit = null;
            //// 유저가 보유한 유닛 찾기
            //foreach (UserUnit usrUnit in userUnitList)
            //{

            //    Debug.Log($"유닛 {unit.id}: {usrUnit.id}: {usrUnit.id.Equals(unit.id)}");
            //    if (usrUnit.id.Equals(unit.id)) {
            //        matchedUserUnit = usrUnit;
            //    }
            //}
            ////UserUnit matchedUserUnit = userUnitList.FirstOrDefault(u => u.id == unit.id);

            //if (matchedUserUnit == null) {
            //    Debug.Log($"유저가 해당 유닛({unit.id})을 보유하지 않음");
            //    continue;
            //}


            Debug.Log($"유닛 {unit.id} 보유 여부: {matchedUserUnit.unlock} (필요: {ishave})");
            if (matchedUserUnit.unlock == ishave)
            {
                // 유닛 UI 생성
                GameObject unitObject = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

                // 버튼 이벤트 추가
                Button button = unitObject.GetComponent<Button>();
                if (button != null)
                {
                    //Debug.Log($"Button found for unit: {unit.name}");
                    button.onClick.AddListener(() => OnUnitClicked(unit, matchedUserUnit));
                }
                else
                {
                    Debug.LogWarning($"Button component missing on unit template for {unit.name}");
                }
            }
        }
    }

    void OnUnitClicked(UnitData unit, UserUnit userUnit)
    {
        if (UnitDetails.Instance != null)
        {
            UnitDetails.Instance.ShowDetails(unit, userUnit);
        }
        else
        {
            Debug.LogWarning("UnitDetails instance is not initialized.");
        }
    }
}
