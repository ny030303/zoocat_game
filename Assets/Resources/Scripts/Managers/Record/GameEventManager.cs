using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    private List<PlayerAction> actionLog = new List<PlayerAction>();
    private LogManager logManager;

    /// Phase C: 기록된 액션을 실시간 구독(대전 릴레이). 파일 로깅과 별개.
    public event Action<PlayerAction> OnActionRecorded;

    void Start()
    {
        logManager = gameObject.AddComponent<LogManager>();
    }

    // ���� ��ġ�� �̺�Ʈ
    public void OnUnitMerged(string unitID1, Vector3 startPosition, string unitID2, Vector3 endPosition, string resultUnitID, Vector3 resultPosition)
    {
        UnitMergeEvent mergeEvent = new UnitMergeEvent
        {
            unitID1 = unitID1,
            startPosition = startPosition,
            unitID2 = unitID2,
            endPosition = endPosition,
            resultUnitID = resultUnitID,
            resultposition = resultPosition,
            timestamp = Time.time
        };

        RecordMergeEvent(mergeEvent);
    }

    // ���� ���� �̺�Ʈ
    public void OnUnitSpawned(string unitID, Vector3 position)
    {
        UnitSpawnEvent spawnEvent = new UnitSpawnEvent
        {
            unitID = unitID,
            position = position,
            timestamp = Time.time
        };
        RecordSpawnEvent(spawnEvent);
    }
    // ���� ���� ���׷��̵� �̺�Ʈ
    public void OnUnitLevelUpgraded(string unitID, int unitNumber)
    {
        UnitLevelUpgradeEvent unitLevelUpgradeEvent = new UnitLevelUpgradeEvent
        {
            unitID = unitID,
            unitNumber = unitNumber,
            timestamp = Time.time
        };
        RecordLevelUpgradedEvent(unitLevelUpgradeEvent);
    }

    // 구독자(대전 릴레이) 먼저 통지 → 그 다음 파일 로깅(IL2CPP 에서 Newtonsoft AOT 로 실패해도
    // 릴레이는 영향 없도록 try/catch).
    private void Commit(PlayerAction action)
    {
        actionLog.Add(action);
        OnActionRecorded?.Invoke(action);
        try { logManager.RecordAction(action); }
        catch (Exception ex) { Debug.LogWarning("[GameEventManager] log write failed: " + ex.Message); }
    }

    // ==== �̺�Ʈ�� �÷��� ��� ���� ====
    private void RecordMergeEvent(UnitMergeEvent mergeEvent)
    {
        Commit(new PlayerAction
        {
            timestamp = mergeEvent.timestamp,
            actionType = "UnitMerge",
            actionData = mergeEvent
        });
    }

    private void RecordSpawnEvent(UnitSpawnEvent spawnEvent)
    {
        Commit(new PlayerAction
        {
            timestamp = spawnEvent.timestamp,
            actionType = "UnitSpawn",
            actionData = spawnEvent
        });
    }
    private void RecordLevelUpgradedEvent(UnitLevelUpgradeEvent unitLevelUpgradeEvent)
    {
        Commit(new PlayerAction
        {
            timestamp = unitLevelUpgradeEvent.timestamp,
            actionType = "UnitLevelUpgrade",
            actionData = unitLevelUpgradeEvent
        });
    }

    public List<PlayerAction> GetActionLog()
    {
        return actionLog;
    }

    void PrintActionLog()
    {
        foreach (var action in actionLog)
        {
            Debug.Log($"Action Type: {action.actionType}, Timestamp: {action.timestamp}");

            // actionData�� �߰� ������ ���� ���, ���
            if (action.actionData != null)
            {
                Debug.Log($"Action Data: {action.actionData.ToString()}");
            }
        }
    }
}


[System.Serializable]
public class PlayerAction
{
    public float timestamp;
    public string actionType; // ��: "Move", "Attack" ��
    public object actionData; // �ʿ��� �߰� ����
}

// ���� ��ġ�� �̺�Ʈ ����
public class UnitMergeEvent
{
    public string unitID1;
    public Vector3 startPosition;
    public string unitID2;
    public Vector3 endPosition;
    public string resultUnitID;
    public Vector3 resultposition;
    public float timestamp;
}

// ���� ���� �̺�Ʈ ����
public class UnitSpawnEvent
{
    public string unitID;
    public Vector3 position;
    public float timestamp;
}

// ���� ���� �̺�Ʈ ����
public class UnitLevelUpgradeEvent
{
    public string unitID;
    public int unitNumber;
    public float timestamp;
}