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
//        // 예시: 이동 기록
//        if (Input.GetKeyDown(KeyCode.W))
//        {
//            RecordAction("Move", transform.position, transform.rotation, null);
//        }

//        // 기타 행동 기록 추가...
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
