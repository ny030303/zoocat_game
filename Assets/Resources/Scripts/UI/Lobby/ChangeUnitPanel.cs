using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class ChangeUnitPanel : MonoBehaviour
{
    public LobbyUserManager lobbyUserManager;

    private UnitData changedUnitData;
    private UserUnit changedUserUnit;
    public GameObject unitContainer; // 유닛 정보를 업데이트할 UI 컨테이너
    public GameObject unitTemplate;  // 유닛 UI 템플릿 (Prefab)
    public Sprite defaultSprite;    // 기본 이미지 (null일 때 대체)
    
    public void OnShowPanel(UnitData unit, UserUnit userUnit)
    {
        // 유닛 컨테이너 초기화 (기존 UI 클리어)
        foreach (Transform child in unitContainer.transform) { Destroy(child.gameObject); }
        changedUnitData = unit;
        changedUserUnit = userUnit;
        // 유닛 UI 생성
        UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

    }

    public void ChangeUnit(UnitData unit)
    {
        if (lobbyUserManager == null || lobbyUserManager.unitDatabase == null)
        {
            Debug.LogError("LobbyUserManager 또는 UnitDatabase가 설정되지 않았습니다.");
            return;
        }

        List<UnitData> unitDeck = lobbyUserManager.unitDatabase.unitDeck;

        if (unitDeck.Contains(changedUnitData)) // 기존 유닛이 덱에 있을 경우
        {
            int index = unitDeck.IndexOf(changedUnitData);
            if (unitDeck.Contains(unit)) // 클릭한 유닛이 덱에 있는 경우
            {
                int unitIndex = unitDeck.IndexOf(unit);
                unitDeck[unitIndex] = changedUnitData; // 기존 유닛을 클릭한 유닛이 있던 위치로 이동
            }
            unitDeck[index] = unit; // 클릭한 유닛으로 변경
            Debug.Log($"{changedUnitData.unitName}이(가) {unit.unitName}으로 교체됨.");
        }
        else if (unitDeck.Contains(unit)) // 클릭한 유닛이 덱에 있을 경우
        {
            int index = unitDeck.IndexOf(unit);
            unitDeck[index] = changedUnitData; // 클릭한 유닛 위치에 기존 changedUnitData를 삽입
            Debug.Log($"{unit.unitName}이(가) {changedUnitData.unitName}으로 교체됨.");
        }

        lobbyUserManager.OnUnitDeckChanged?.Invoke(unitDeck); // UI 갱신 이벤트 호출

        // 변경된 유닛 데이터를 반영
        changedUnitData = unit;

        if (UserManager.Instance == null)
        {
            Debug.LogWarning("UserManager.Instance is null!");
            return;
        }

        UserData userdata = UserManager.Instance.currentUser;
        userdata.selectedUnits = unitDeck.ConvertAll(u => u.id.Replace("CHA_", "")).ToArray(); // UserData의 selectedUnits에 ID에서 "CHA_" 제거 후 저장

        if (UserManager.Instance.isGuest == 0)
        {
            FileManager.SaveUserData(userdata); // 변경 사항 저장
            UserManager.Instance.currentUser = FileManager.LoadUserData(); // 변경된 데이터 다시 로드
            Debug.Log("덱 변경 사항 저장 UserManager 업데이트 완료.");
        } else
        {
            SendUpdateDeckEventMessageToServer(userdata, userdata.selectedUnits);
        }
        
    }


    private void SendUpdateDeckEventMessageToServer(UserData currentUser, string[] newDeck)
    {
        var messageToSend = new
        {
            @event = "updateDeck",  // @ 기호를 사용하여 예약어 사용
            data = new
            {
                userId = currentUser.id,
                newDeck = newDeck,
            }
        };
        // JSON 문자열로 변환
        string jsonMessage = JsonMapper.ToJson(messageToSend);
        try
        {
            // 서버에 메시지 전송
            SocketBinder.Instance.GetWs().Send(jsonMessage);
            Console.WriteLine("서버로 메시지 전송: " + jsonMessage);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle the error
            Debug.LogError("WebSocket is not open: " + ex.Message);
        }
    }
}
