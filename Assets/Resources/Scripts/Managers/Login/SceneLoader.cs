using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;  // 로딩 화면 오브젝트
    public Slider progressBar;        // 로딩 진행 바 (Slider UI 요소)
    private bool isUserDataLoaded = false; // 유저 데이터가 로드되었는지 확인하는 변수
    private JsonData units;          // 서버에서 받은 유저 데이터
    void OnEnable()
    {
        SocketDispatcher.Instance.On(SocketEvents.UserJoined, OnUserJoined);
    }

    void OnDisable()
    {
        if (SocketDispatcher.HasInstance)
            SocketDispatcher.Instance.Off(SocketEvents.UserJoined, OnUserJoined);
    }

    // userJoined: data = { units: { userId, units: Unit[] } | null }
    private void OnUserJoined(JsonData data)
    {
        JsonData serverUnits = null;
        if (data != null && data.Has("units") && data["units"] != null && data["units"].Has("units"))
            serverUnits = data["units"]["units"];

        if (serverUnits != null && serverUnits.IsArray && serverUnits.Count > 0)
        {
            units = serverUnits;
            UserManager.Instance.LoadUserUnitsFromJson(units);
        }
        else
        {
            // 서버 로스터 없음(신규 게스트 등) → 로컬 로스터 유지
            Debug.LogWarning("[SceneLoader] userJoined without units - keeping local roster");
        }
        isUserDataLoaded = true;
    }

    // 씬을 비동기적으로 로드하는 코루틴
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadUserDataAndScene(sceneName));
    }

    // 유저 데이터를 소켓을 통해 로드하고, 씬을 비동기적으로 로드하는 코루틴
    private IEnumerator LoadUserDataAndScene(string sceneName)
    {
        if (UserManager.Instance.isGuest != 0)
        {
            // 서버를 통해 데이터를 가져오는 경우.(google play games)
            // 1. 소켓을 통해 서버에서 유저 데이터를 요청
            yield return StartCoroutine(LoadUserDataFromServer());
            Debug.Log("Logged-in user detected. Fetching user data from server...");
        }
        else
        {
            Debug.Log("Guest login detected. Skipping server data load.");
        }
        // 2. 유저가 가진 유닛 데이터를 레벨에 맞게 수치를 계산한 후
        yield return StartCoroutine(LoadUnitLevel());

        // 3. 유저 데이터를 로드한 후 씬을 로드
        yield return StartCoroutine(LoadSceneAsync(sceneName));
    }


    private IEnumerator LoadUnitLevel()
    {
        UserUnit[] userUnits = UserManager.Instance.units;
        List<UnitData> unitList = UnitListLoader.Instance.unitList; // 게임 내 유닛 데이터
        Dictionary<string, UnitData> baseUnitData;  // 기본값 유닛 데이터
        string sheetCsvFilePath = "Scripts/Data/Sheet/CharacterSheet"; // CSV 파일 (확장자 제거)

        // CSV 데이터 로드
        baseUnitData = CSVLoader.LoadUnitData(sheetCsvFilePath);

        // 모든 유닛의 스탯 업데이트 진행
        for (int i = 0; i < unitList.Count; i++)
        {
            if (unitList[i] == null) continue;
            if (userUnits == null || i >= userUnits.Length) continue; // 유저 유닛 데이터 없으면 기본 스탯 유지

            // ID 변환 및 기본 공격력 가져오기
            if (baseUnitData.TryGetValue(unitList[i].id.Replace("CHA_", ""), out UnitData foundUnit))
            {
                float baseAtk = foundUnit.atk; // 처음 설정된 기본 공격력
                unitList[i].atk = (int)Mathf.Round(baseAtk * Mathf.Pow(1.1f, userUnits[i].lv - 1)); // 레벨이 올라갈 때마다 10% 증가
            }
            else
            {
                Debug.LogWarning($"Unit ID {unitList[i].id} not found in baseUnitData.");
            }
        }

        // 모든 계산이 끝난 후 대기 (완료 플래그 추가)
        bool isCompleted = false;

        while (!isCompleted)
        {
            yield return null; // 한 프레임 대기 (CPU 부하 방지)
            isCompleted = true;
        }

        Debug.Log("✅ LoadUnitLevel() 완료: 모든 유닛 레벨 스탯 계산 완료.");
    }

    // 소켓을 통해 유저 데이터를 비동기적으로 로드하는 메서드
    private IEnumerator LoadUserDataFromServer()
    {
        // 로딩 화면을 활성화
        loadingScreen.SetActive(true);

        // 서버에 로비 참가 요청 (연결의 userId 사용 - payload 불필요).
        // 아직 인증 전이면 SocketBinder 가 보류했다가 loginSuccess 후 자동 전송.
        SocketBinder.Instance.SendWhenAuthed(SocketEvents.JoinLobby);

        // 서버 응답(userJoined) 대기 — 소켓이 안 붙어도 타임아웃 후 로컬 캐시로 진행
        const float timeout = 10f;
        float elapsed = 0f;
        while (!isUserDataLoaded && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!isUserDataLoaded)
            Debug.LogWarning("[SceneLoader] server data timeout - continuing with local cache");
    }

    // 비동기 씬 로드 및 로딩 화면 표시
    private IEnumerator LoadSceneAsync(string sceneName)
    {

        // 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 로딩 완료 후 씬 전환 대기

        // 씬이 완전히 로드될 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상태에 따라 로딩 바 업데이트
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            progressBar.value = progress;

            // 로딩이 완료되면 씬 전환
            if (asyncLoad.progress >= 0.9f)
            {
                // 필요 시 로딩 완료 후 잠시 대기
                yield return new WaitForSeconds(1f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
