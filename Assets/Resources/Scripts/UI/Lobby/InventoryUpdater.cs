using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    public UnitUpgradeManager unitUpgradeManager;
    void Start()
    {
        StartCoroutine(InitializeInventory());
        UserManager.Instance.OnUserUnitListChanged += updateCharacterList;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        UserManager.Instance.OnUserUnitListChanged -= updateCharacterList;
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
        // 유닛 덱 변경 이벤트 구독
        Debug.Log("유닛 데이터 및 사용자 유닛 로드 완료");
        GenerateCharacterList();
    }
    // 리스트 갱신 구독이벤트
    private void updateCharacterList(UserUnit[] userUnits)
    {
        Debug.Log("리스트 갱신");
        unitList = UnitListLoader.Instance.unitList;
        userUnitList = userUnits;
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

            UserUnit matchedUserUnit = userUnitList.FirstOrDefault(u => u.id == unitIdWithoutPrefix); // 유저의 유닛 레벨, 경험치 등
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

                //Debug.Log($" {unitIdWithoutPrefix} CanUpgradeUnit: {unitUpgradeManager.CanUpgradeUnit(unitIdWithoutPrefix)}");
                // 유닛 UI 생성
                GameObject unitObject = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

                UnitUpgradeData unitUpgradeData = null;

                // 레벨 표시
                Transform levelchild = unitObject.transform.Find("Level");
                if (levelchild != null)
                {
                    TMP_Text levelText = levelchild.GetComponent<TMP_Text>();
                    levelText.text = $"Lv. {matchedUserUnit.lv}";
                }
                // 슬라이더에 값 넣기
                Transform sliderchild = unitObject.transform.Find("Slider");
                if (sliderchild != null && unitUpgradeManager != null)
                {
                    Slider slider = sliderchild.GetComponent<Slider>();
                    unitUpgradeData = unitUpgradeManager.GetUnitUpgradeData(unitIdWithoutPrefix); // 업그레이드 값 가져옴
                                                                                                  
                    Transform handleTransform = slider.transform.Find("Fill Area/Fill");// Handle 색상 변경을 위한 Fill 이미지 찾기
                    Image handleImage = handleTransform != null ? handleTransform.GetComponent<Image>() : null;
                    if (unitUpgradeData == null)
                    {
                        slider.maxValue = 1;
                        slider.value = 1;
                        handleImage.color = Color.yellow;
                        Transform valChild = sliderchild.transform.Find("ValueText");
                        TMP_Text valText = valChild.GetComponent<TMP_Text>();
                        valText.text = $"MAX";
                    } else
                    {
                        slider.maxValue = unitUpgradeData.cost;
                        slider.value = matchedUserUnit.piece;
                        Transform valChild = sliderchild.transform.Find("ValueText");
                        TMP_Text valText = valChild.GetComponent<TMP_Text>();
                        valText.text = $"{matchedUserUnit.piece} / {unitUpgradeData.cost}";
                    }
                }

                // 버튼 이벤트 추가
                Button button = unitObject.GetComponent<Button>();
                if (button != null)
                {
                    //Debug.Log($"Button found for unit: {unit.name}");
                    button.onClick.AddListener(() => OnUnitClicked(unit, matchedUserUnit, unitUpgradeData));
                }
                else
                {
                    Debug.LogWarning($"Button component missing on unit template for {unit.name}");
                }
            }
        }
    }

    void OnUnitClicked(UnitData unit, UserUnit userUnit, UnitUpgradeData unitUpgradeData)
    {
        if (UnitDetails.Instance != null)
        {
            UnitDetails.Instance.ShowDetails(unit, userUnit, unitUpgradeData);
        }
        else
        {
            Debug.LogWarning("UnitDetails instance is not initialized.");
        }
    }
}
