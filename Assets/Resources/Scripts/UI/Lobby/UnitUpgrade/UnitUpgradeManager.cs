using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUpgradeManager : MonoBehaviour
{ // 로비에서 유닛을 업그레이드 하는 부분

    public Dictionary<(int, int), UnitUpgradeData> upgradeData; // 업그레이드 데이터 저장
    public UserData userData;  // 유저 데이터
    public Dictionary<string, UnitData> baseUnitData;  // 기본값 유닛 데이터
    public string costCsvFilePath = "Scripts/Data/Sheet/CharacterCost"; // CSV 파일 경로
    public string sheetCsvFilePath = "Scripts/Data/Sheet/CharacterSheet"; // CSV 파일 경로

    public List<UnitData> unitList; // 게임의 모든 유닛 데이터
    private UserUnit[] userUnitList; // 유저가 보유한 유닛 목록
    private void Start()
    {
        StartCoroutine(InitializeInventory());
        upgradeData = CSVLoader.LoadUpgradeData(costCsvFilePath);
        baseUnitData = CSVLoader.LoadUnitData(sheetCsvFilePath);
    }
    public Action<UnitData, UserUnit, UnitUpgradeData> OnUnitUpgraded;
    IEnumerator InitializeInventory()
    {
        // 데이터 로드가 완료될 때까지 대기
        while (UserManager.Instance == null || UserManager.Instance.units == null || UserManager.Instance.units.Length == 0 ||
               UnitListLoader.Instance == null || UnitListLoader.Instance.unitList == null)
        {
            yield return null;
        }
        // 데이터 할당
        userData = UserManager.Instance.currentUser;
        unitList = UnitListLoader.Instance.unitList;
        userUnitList = UserManager.Instance.units;

        Debug.Log("유닛 데이터 및 사용자 유닛 로드 완료");
        GenerateCharacterList();
    }

    private void GenerateCharacterList()
    {
    }

    public bool CanUpgradeUnit(string unitId)
    {
        UserUnit userUnit = GetUserUnit(unitId);
        UnitUpgradeData nextUpgrade = GetUnitUpgradeData(unitId);
        // 업그레이드 조건 확인 (골드 및 조각 개수)

        return userUnit.piece >= nextUpgrade.cost;
        //return userUnit.piece >= nextUpgrade.cost && userData.gold >= nextUpgrade.cost;
    }

    public UnitUpgradeData GetUnitUpgradeData(string unitId)
    {
        UnitData unit = unitList.Find(i => i.id.Replace("CHA_", "").Equals(unitId));
        UserUnit userUnit = GetUserUnit(unitId);

        if (userUnit == null || userUnit.unlock == 0) return null; // 유닛이 없거나 잠겨있으면 업그레이드 불가

        int currentGrade = unit.grade;
        int currentLevel = userUnit.lv;

        // 현재 레벨이 데이터에 있는지 확인
        if (!upgradeData.ContainsKey((currentGrade, currentLevel)))
        {
            Debug.Log($"업그레이드 데이터 없음: grade {currentGrade}, level {currentLevel}");
            return null;
        }
        UnitUpgradeData nextUpgrade = upgradeData[(currentGrade, currentLevel)];
        Debug.Log($"업그레이드 데이터: grade {currentGrade}, level {currentLevel}");
        Debug.Log($"데이터: piece {userUnit.piece}, cost {nextUpgrade.cost}, gold {userData.gold}");
        return nextUpgrade;
    }

    public void UpgradeUnit(string unitId)
    {
        if (!CanUpgradeUnit(unitId)) return;

        UserUnit userUnit = GetUserUnit(unitId);
        if (userUnit == null) return;

        UnitData unitData = unitList.Find(i => i.id.Replace("CHA_", "").Equals(unitId));
        int currentGrade = unitData.grade;
        int currentLevel = userUnit.lv;

        //UnitUpgradeData baseUpgrade = upgradeData[(currentGrade, 1)];
        UnitUpgradeData nextUpgrade = upgradeData[(currentGrade, currentLevel)];

        // 비용 차감
        userUnit.piece -= nextUpgrade.cost;
        //userData.gold -= nextUpgrade.cost;

        // 레벨 업
        userUnit.lv++;

        baseUnitData.TryGetValue(unitId, out UnitData foundUnit);
        // 기존 baseAtk 값을 유지하고 10% 증가 방식으로 atk 재계산
        float baseAtk = foundUnit.atk; // 처음 설정된 기본 공격력
        unitData.atk = (int) Mathf.Round(baseAtk * Mathf.Pow(1.1f, userUnit.lv - 1)); // 레벨이 올라갈 때마다 10% 증가

        Debug.Log($"Unit {unitId} upgraded to Level {userUnit.lv}. New ATK: {unitData.atk}");

        if (UserManager.Instance.isGuest == 0)
        {

            // 파일 저장
            FileManager.SaveUserData(userData); // 유저의 정보를 저장
            FileManager.SaveUnit(userUnit); // 유저의 유닛 상태를 저장(Lv, piece 등)
            UserManager.Instance.currentUser = FileManager.LoadUserData(); // 변경된 데이터 다시 로드
            Debug.Log("유닛업그레이드 업데이트 완료.");
        }
        else
        {
            SendUpgradeUnitEventMessageToServer(userData, userUnit);
        }


        for (int i = 0; i < userUnitList.Length; i++)
        {
            if (userUnitList[i].id.Equals(userUnit.id)) // 같은 ID의 유닛 찾기
            {
                userUnitList[i] = userUnit; // 기존 유닛 데이터 업데이트
            }
        }
        UserManager.Instance.OnUserUnitListChanged?.Invoke(userUnitList); // 이벤트 호출
        if (userUnit.lv == 10) { OnUnitUpgraded?.Invoke(unitData, userUnit, upgradeData[(currentGrade, 9)]); }
        else OnUnitUpgraded?.Invoke(unitData, userUnit, upgradeData[(currentGrade, userUnit.lv)]); // 디테일 창 값 전달호출 }
        }




    private void SendUpgradeUnitEventMessageToServer(UserData currentUser, UserUnit userUnit)
    {
        var messageToSend = new
        {
            @event = "upgradeUnit",  // @ 기호를 사용하여 예약어 사용
            data = new
            {
                userId = currentUser.id,
                userUnit = userUnit,
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

    private UserUnit GetUserUnit(string unitId)
    {
        foreach (var unit in userUnitList)
        {
            if (unit.id.Replace("CHA_", "").Equals(unitId)) return unit;
        }
        return null;
    }
}
