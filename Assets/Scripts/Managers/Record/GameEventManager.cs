using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    private List<PlayerAction> actionLog = new List<PlayerAction>();
    private LogManager logManager;

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

    // ==== �̺�Ʈ�� �÷��� ��� ���� ====
    private void RecordMergeEvent(UnitMergeEvent mergeEvent)
    {
        PlayerAction action = new PlayerAction
        {
            timestamp = mergeEvent.timestamp,
            actionType = "UnitMerge",
            actionData = mergeEvent
        };

        actionLog.Add(action);
        logManager.RecordAction(action); // LogManager�� �α� ���
    }

    private void RecordSpawnEvent(UnitSpawnEvent spawnEvent)
    {
        PlayerAction action = new PlayerAction
        {
            timestamp = spawnEvent.timestamp,
            actionType = "UnitSpawn",
            position = spawnEvent.position,
            actionData = spawnEvent
        };

        actionLog.Add(action);
        logManager.RecordAction(action); // LogManager�� �α� ���
    }

    public List<PlayerAction> GetActionLog()
    {
        return actionLog;
    }

    void PrintActionLog()
    {
        foreach (var action in actionLog)
        {
            Debug.Log($"Action Type: {action.actionType}, Timestamp: {action.timestamp}, Position: {action.position}, Rotation: {action.rotation}");

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
    public Vector3 position;
    public Quaternion rotation;
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