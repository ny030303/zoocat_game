using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogReplayManager : MonoBehaviour
{
    private UnitSpawnManager unitSpawnManager;
    private GameManager gameManager;
    private UnitDatabase unitDatabase;
    private List<PlayerAction> events;
    private int currentIndex = 0;
    private float replayStartTime;
    private Button[] levelUpgradeButtons;
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        unitDatabase = gameManager.unitDatabase;
        // �α׸� �ε��մϴ�.
        LogManager logManager = gameObject.AddComponent<LogManager>();
        string ymdhms = "20240824_181253";
        events = logManager.LoadActionsFromFile(ymdhms, 0);

        GameObject manager = GameObject.Find("AIUnitSpawnManager");
        unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        unitSpawnManager.Initialize();

        GameObject upgradeBtnsPanel = GameObject.Find("UI_unit_upgrade_tile_AI");
        levelUpgradeButtons = upgradeBtnsPanel.GetComponentsInChildren<Button>();

        // ������ ������ �ð��� ����մϴ�.
        replayStartTime = Time.time;

        // ����� �����մϴ�.
        StartReplay();
    }
    private void StartReplay()
    {
        StartCoroutine(Replay());
    }

    private IEnumerator Replay()
    {
        while (currentIndex < events.Count)
        {
            PlayerAction currentEvent = events[currentIndex];

            // ù �̺�Ʈ�� Ÿ�ӽ������� ������� ��� �ð� ���
            float timeToWait = currentEvent.timestamp - (Time.time - replayStartTime);

            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            // ���� ���¸� �̺�Ʈ�� ���� ������Ʈ
            ApplyEvent(currentEvent);
            currentIndex++;
        }
    }
    Vector2 cpyLocalPos(GameObject obj, Transform parentTransform)
    {
        Vector2 pos = new Vector2(obj.transform.position.x, obj.transform.position.y);
        pos = parentTransform.InverseTransformPoint(pos);
        return pos;
    }
    private void ApplyEvent(PlayerAction gameEvent)
    {
        if (gameEvent.actionType == "UnitSpawn" && unitSpawnManager.IsSpawnNext())
        {
            // JObject�� UnitSpawnEvent�� ��ȯ
            UnitSpawnEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitSpawnEvent>();
            if (ev != null) { Debug.Log("spawn pos:" + ev.position); unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID); }
            else { Debug.LogError("actionData�� UnitSpawnEvent�� ��ȯ�� �� �����ϴ�."); }
        }
        else if (gameEvent.actionType == "UnitMerge")
        {
            UnitMergeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitMergeEvent>();
            if (ev != null)
            {
                Transform parentTransform = unitSpawnManager.GetParentTransform();
                int childCount = parentTransform.childCount;
                // �ڽ� ������Ʈ���� GameObject �迭�� ��������
                GameObject[] childObjects = new GameObject[childCount];
                for (int i = 0; i < childCount; i++) { childObjects[i] = parentTransform.GetChild(i).gameObject; }

                foreach (GameObject obj in childObjects)
                {
                    Unit unitset = obj.GetComponent<Unit>();
                    if (unitset.unitData.id == ev.unitID1)
                    {
                        if (cpyLocalPos(obj, parentTransform).ToString() == ((Vector2)ev.startPosition).ToString()) { unitset.Kill(); }
                        else if (cpyLocalPos(obj, parentTransform).ToString() == ((Vector2)ev.endPosition).ToString())
                        {
                            GameObject unit = unitSpawnManager.SpawnNextAlly(ev.endPosition, ev.resultUnitID);
                            // �� ���� ���׷��̵�
                            Unit newunitset = unit.GetComponent<Unit>();
                            newunitset.UpgradeUnitMerged(unitset.unitData);
                            // ���� ���� �����
                            unitSpawnManager.KillUnit(obj);
                        }
                    }
                }
            }
            else { Debug.LogError("actionData�� UnitMergeEvent�� ��ȯ�� �� �����ϴ�."); }
        }
        else if (gameEvent.actionType == "UnitLevelUpgrade") {
            UnitLevelUpgradeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitLevelUpgradeEvent>();

            UnitData unitData = unitDatabase.GetUnitDataToIdx("ai", ev.unitNumber);
            GameObject unitsSpawnLocationObj = unitSpawnManager.GetParentTransform().gameObject;
            if (unitData != null && unitData.level < unitData.maxUpgradeLevel)
            {
                unitData.LevelUp(); // �ش� ���� ������ ���׷��̵�
                Unit[] allUnits = unitsSpawnLocationObj.GetComponentsInChildren<Unit>();
                foreach (Unit unit in allUnits) { if (unit.unitData.id == unitData.id) { unit.unitData.LevelUp(); } }// ���� ����� ���� �����Ϳ��� ���� 

                // UI �ؽ�Ʈ ������Ʈ
                GameObject btnObj = levelUpgradeButtons[ev.unitNumber].gameObject;
                TextMeshProUGUI[] TextMeshes = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
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
                if (unitData.level == unitData.maxUpgradeLevel) levelUpgradeButtons[ev.unitNumber].interactable = false;
            }
            else
            {
                Debug.Log("���׷��̵� �Ұ�");
            }
        }
    }
}
