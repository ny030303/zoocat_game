using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogReplayManager : MonoBehaviour
{
    private UnitSpawnManager unitSpawnManager;
    private List<PlayerAction> events;
    private int currentIndex = 0;
    void Start()
    {
        // 로그를 로드합니다.
        LogManager logManager = gameObject.AddComponent<LogManager>();
        events = logManager.LoadActionsFromFile(0);

        GameObject manager = GameObject.Find("AIUnitSpawnManager");
        unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        unitSpawnManager.Initialize();
        Debug.Log(unitSpawnManager.owner);
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
            // 게임 상태를 이벤트에 따라 업데이트
            ApplyEvent(currentEvent);
            currentIndex++;
            yield return new WaitForSeconds(currentEvent.timestamp - Time.time); // 이벤트 사이의 시간차를 유지
        }
    }

    private void ApplyEvent(PlayerAction gameEvent)
    {
        if (gameEvent.actionType == "UnitSpawn" && unitSpawnManager.IsSpawnNext())
        {
            // JObject를 UnitSpawnEvent로 변환
            UnitSpawnEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitSpawnEvent>();
            if (ev != null)  {  unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID);  }
            else { Debug.LogError("actionData를 UnitSpawnEvent로 변환할 수 없습니다."); }
        } else if (gameEvent.actionType == "UnitMerge") {
            UnitMergeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitMergeEvent>();
            if (ev != null) {
                //unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID); 
            }
            else { Debug.LogError("actionData를 UnitMergeEvent로 변환할 수 없습니다."); }
        }
    }
}
