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
        // ��ư ������Ʈ�� ������ Ŭ�� �̺�Ʈ�� �޼��� ����
        spawnButton = GetComponent<Button>();
        gameManager = FindAnyObjectByType<GameManager>();
        eventManager = FindObjectOfType<GameEventManager>();

        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(ClickEvent);
        }
        else
        {
            Debug.LogError("Spawn Button�� null�Դϴ�. ��ư ������Ʈ�� Ȯ���ϼ���.");
        }

        // �̺�Ʈ ����
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
            gameManager.UpgradeUnit(unitData.upgradeCost); // ���� ��ȭ ���
            // ���׷��̵� ���� �߰�
            unitData.LevelUp(); // �ش� ���� ������ ���׷��̵�
            Unit[] allUnits = unitsSpawnLocationObj.GetComponentsInChildren<Unit>();
            foreach (Unit unit in allUnits)
            {
                if (unit.unitData.id == unitData.id)
                {
                    unit.unitData.LevelUp(); // ���� ����� ���� �����Ϳ��� ����
                }
            }
            // UI �ؽ�Ʈ ������Ʈ
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
            Debug.Log("���׷��̵� �Ұ�");
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
            Debug.LogWarning("���� �����Ͱ� ��ȿ���� �ʽ��ϴ�.");
        }
    }

    private bool isInUnitData()
    {
        // unitDatabase�� null�� �ƴ���, �׸��� unitNumber�� ��ȿ�� ���� ���� �ִ��� Ȯ��
        return unitDatabase != null && unitNumber >= 0 && unitNumber < unitDatabase.GetUnitListCount("player");
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged -= changeState;
        }
    }

    // Update �޼��尡 �ʿ� �����Ƿ� ����
}
