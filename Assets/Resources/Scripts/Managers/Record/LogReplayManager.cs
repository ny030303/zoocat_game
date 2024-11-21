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
        // 로그를 로드합니다.
        LogManager logManager = gameObject.AddComponent<LogManager>();
        string ymdhms = "20240824_181253";
        events = logManager.LoadActionsFromFile(ymdhms, 0);

        GameObject manager = GameObject.Find("AIUnitSpawnManager");
        unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        unitSpawnManager.Initialize();

        GameObject upgradeBtnsPanel = GameObject.Find("UI_unit_upgrade_tile_AI");
        levelUpgradeButtons = upgradeBtnsPanel.GetComponentsInChildren<Button>();

        // 게임이 시작한 시간을 기록합니다.
        replayStartTime = Time.time;

        // 재생을 시작합니다.
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

            // 첫 이벤트의 타임스탬프를 기반으로 대기 시간 계산
            float timeToWait = currentEvent.timestamp - (Time.time - replayStartTime);

            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            // 게임 상태를 이벤트에 따라 업데이트
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
            // JObject를 UnitSpawnEvent로 변환
            UnitSpawnEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitSpawnEvent>();
            if (ev != null) { Debug.Log("spawn pos:" + ev.position); unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID); }
            else { Debug.LogError("actionData를 UnitSpawnEvent로 변환할 수 없습니다."); }
        }
        else if (gameEvent.actionType == "UnitMerge")
        {
            UnitMergeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitMergeEvent>();
            if (ev != null)
            {
                Transform parentTransform = unitSpawnManager.GetParentTransform();
                int childCount = parentTransform.childCount;
                // 자식 오브젝트들을 GameObject 배열로 가져오기
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
                            // 새 유닛 업그레이드
                            Unit newunitset = unit.GetComponent<Unit>();
                            newunitset.UpgradeUnitMerged(unitset.unitData);
                            // 기존 유닛 지우기
                            unitSpawnManager.KillUnit(obj);
                        }
                    }
                }
            }
            else { Debug.LogError("actionData를 UnitMergeEvent로 변환할 수 없습니다."); }
        }
        else if (gameEvent.actionType == "UnitLevelUpgrade") {
            UnitLevelUpgradeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitLevelUpgradeEvent>();

            UnitData unitData = unitDatabase.GetUnitDataToIdx("ai", ev.unitNumber);
            GameObject unitsSpawnLocationObj = unitSpawnManager.GetParentTransform().gameObject;
            if (unitData != null && unitData.level < unitData.maxUpgradeLevel)
            {
                unitData.LevelUp(); // 해당 유닛 데이터 업그레이드
                Unit[] allUnits = unitsSpawnLocationObj.GetComponentsInChildren<Unit>();
                foreach (Unit unit in allUnits) { if (unit.unitData.id == unitData.id) { unit.unitData.LevelUp(); } }// 개인 복사된 유닛 데이터에도 적용 

                // UI 텍스트 업데이트
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
                Debug.Log("업그레이드 불가");
            }
        }
    }
}
