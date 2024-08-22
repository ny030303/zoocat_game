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
        // �α׸� �ε��մϴ�.
        LogManager logManager = gameObject.AddComponent<LogManager>();
        events = logManager.LoadActionsFromFile(0);

        GameObject manager = GameObject.Find("AIUnitSpawnManager");
        unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        unitSpawnManager.Initialize();
        Debug.Log(unitSpawnManager.owner);
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
            // ���� ���¸� �̺�Ʈ�� ���� ������Ʈ
            ApplyEvent(currentEvent);
            currentIndex++;
            yield return new WaitForSeconds(currentEvent.timestamp - Time.time); // �̺�Ʈ ������ �ð����� ����
        }
    }

    private void ApplyEvent(PlayerAction gameEvent)
    {
        if (gameEvent.actionType == "UnitSpawn" && unitSpawnManager.IsSpawnNext())
        {
            // JObject�� UnitSpawnEvent�� ��ȯ
            UnitSpawnEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitSpawnEvent>();
            if (ev != null)  {  unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID);  }
            else { Debug.LogError("actionData�� UnitSpawnEvent�� ��ȯ�� �� �����ϴ�."); }
        } else if (gameEvent.actionType == "UnitMerge") {
            UnitMergeEvent ev = (gameEvent.actionData as JObject)?.ToObject<UnitMergeEvent>();
            if (ev != null) {
                //unitSpawnManager.SpawnNextAlly(ev.position, ev.unitID); 
            }
            else { Debug.LogError("actionData�� UnitMergeEvent�� ��ȯ�� �� �����ϴ�."); }
        }
    }
}
