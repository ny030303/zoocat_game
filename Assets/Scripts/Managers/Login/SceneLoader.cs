using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;  // 로딩 화면 오브젝트
    public Slider progressBar;        // 로딩 진행 바 (Slider UI 요소)
    private bool isUserDataLoaded = false; // 유저 데이터가 로드되었는지 확인하는 변수
    private string userData;          // 서버에서 받은 유저 데이터

    // 씬을 비동기적으로 로드하는 코루틴
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadUserDataAndScene(sceneName));
    }

    // 유저 데이터를 소켓을 통해 로드하고, 씬을 비동기적으로 로드하는 코루틴
    private IEnumerator LoadUserDataAndScene(string sceneName)
    {
        // 1. 소켓을 통해 서버에서 유저 데이터를 요청
        yield return StartCoroutine(LoadUserDataFromServer());

        // 2. 유저 데이터를 로드한 후 씬을 로드
        yield return StartCoroutine(LoadSceneAsync(sceneName));
    }

    // 소켓을 통해 유저 데이터를 비동기적으로 로드하는 메서드
    private IEnumerator LoadUserDataFromServer()
    {
        // 유저 데이터를 서버에 요청
        var messageToSend = new
        {
            @event = "joinLobby",  // 서버에 보낼 이벤트 이름
            data = new
            {
                userId = "some_user_id"  // 필요에 따라 유저 ID 등을 포함
            }
        };

        // JSON 문자열로 변환하여 서버로 전송
        string jsonMessage = LitJson.JsonMapper.ToJson(messageToSend);
        try
        {
            SocketBinder.Instance.GetWs().Send(jsonMessage);  // 서버에 메시지 전송
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError("WebSocket is not open: " + ex.Message);
            yield break;
        }

        // 서버로부터 응답을 기다림 (유저 데이터가 로드될 때까지 대기)
        while (!isUserDataLoaded)
        {
            yield return null;
        }

        // 응답받은 유저 데이터를 사용 (userData에 저장된 데이터)
        Debug.Log("User data received: " + userData);

        // TODO: userData를 파싱하고 게임 내에서 사용할 수 있도록 처리
        // 예시: var user = JsonUtility.FromJson<UserData>(userData);
    }

    // 서버로부터 유저 데이터를 받았을 때 콜백으로 처리하는 함수
    public void OnUserDataReceived(string data)
    {
        userData = data; // 서버에서 받은 데이터를 저장
        isUserDataLoaded = true; // 유저 데이터가 로드되었음을 표시
    }

    // 비동기 씬 로드 및 로딩 화면 표시
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 화면을 활성화
        loadingScreen.SetActive(true);

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
