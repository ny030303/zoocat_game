using System.Collections.Generic;
using UnityEngine;



//public class PlayerRecorder : MonoBehaviour
//{
//    private List<PlayerAction> actionLog = new List<PlayerAction>();
//    private float startTime;

//    void Start()
//    {
//        startTime = Time.time;
//    }

//    void Update()
//    {
//        // ����: �̵� ���
//        if (Input.GetKeyDown(KeyCode.W))
//        {
//            RecordAction("Move", transform.position, transform.rotation, null);
//        }

//        // ��Ÿ �ൿ ��� �߰�...
//    }

//    void RecordAction(string actionType, Vector3 position, Quaternion rotation, object actionData)
//    {
//        PlayerAction action = new PlayerAction
//        {
//            timestamp = Time.time - startTime,
//            position = position,
//            rotation = rotation,
//            actionType = actionType,
//            actionData = actionData
//        };

//        actionLog.Add(action);
//    }
//}
