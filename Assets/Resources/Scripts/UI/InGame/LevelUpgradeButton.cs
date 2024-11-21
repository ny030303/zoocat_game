using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpgradeButton : MonoBehaviour
{
    private Button spawnButton;
    private GameManager gameManager;
    private GameEventManager eventManager;
    public UnitDatabase unitDatabase;
    public int unitNumber;
    private UnitData unitData;
    public GameObject unitsSpawnLocationObj;

    void Start()
    {
        // 버튼 컴포넌트를 가져와 클릭 이벤트에 메서드 연결
        spawnButton = GetComponent<Button>();
        gameManager = FindAnyObjectByType<GameManager>();
        eventManager = FindObjectOfType<GameEventManager>();

        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(ClickEvent);
        }
        else
        {
            Debug.LogError("Spawn Button이 null입니다. 버튼 컴포넌트를 확인하세요.");
        }

        // 이벤트 구독
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged += changeState;
        }
    }

    private void ClickEvent()
    {
        if (isInUnitData()) unitData = unitDatabase.GetUnitDataToIdx("player", unitNumber);

        if (unitData != null && unitData.level < unitData.maxUpgradeLevel && gameManager.CheckLevelUpgradeState(unitData.upgradeCost))
        {
            gameManager.UpgradeUnit(unitData.upgradeCost); // 게임 재화 사용
            // 업그레이드 로직 추가
            unitData.LevelUp(); // 해당 유닛 데이터 업그레이드
            Unit[] allUnits = unitsSpawnLocationObj.GetComponentsInChildren<Unit>();
            foreach (Unit unit in allUnits)
            {
                if (unit.unitData.id == unitData.id)
                {
                    unit.unitData.LevelUp(); // 개인 복사된 유닛 데이터에도 적용
                }
            }
            // UI 텍스트 업데이트
            TextMeshProUGUI[] TextMeshes = this.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            string currencyText = unitData.level == unitData.maxUpgradeLevel ? "---" : unitData.upgradeCost.ToString();
            string levelText = unitData.level == unitData.maxUpgradeLevel ? "Lv. MAX" : "Lv. " + unitData.level.ToString();

            foreach (TextMeshProUGUI txtMesh in TextMeshes)
            {
                switch (txtMesh.gameObject.name)
                {
                    case "CurrencyText":
                        txtMesh.text = currencyText;
                        break;
                    case "LevelText":
                        txtMesh.text = levelText;
                        break;
                }
            }
            if(unitData.level == unitData.maxUpgradeLevel) spawnButton.interactable = false;
            eventManager.OnUnitLevelUpgraded(unitData.id, unitNumber);
        }
        else
        {
            Debug.Log("업그레이드 불가");
        }
    }

    private void changeState()
    {
        if (isInUnitData())
        {
            unitData = unitDatabase.GetUnitDataToIdx("player", unitNumber);
            if (gameManager.CheckLevelUpgradeState(unitData.upgradeCost)) 
            {
                spawnButton.interactable = unitData.level == unitData.maxUpgradeLevel ? false : true;
            }
            else
            {
                spawnButton.interactable = false;
            }
        }
        else
        {
            spawnButton.interactable = false;
            Debug.LogWarning("유닛 데이터가 유효하지 않습니다.");
        }
    }

    private bool isInUnitData()
    {
        // unitDatabase가 null이 아닌지, 그리고 unitNumber가 유효한 범위 내에 있는지 확인
        return unitDatabase != null && unitNumber >= 0 && unitNumber < unitDatabase.GetUnitListCount("player");
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged -= changeState;
        }
    }

    // Update 메서드가 필요 없으므로 삭제
}
