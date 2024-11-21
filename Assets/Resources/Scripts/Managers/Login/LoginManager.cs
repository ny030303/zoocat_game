using LitJson;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader 스크립트를 참조

    private GameObject LoginPanel;
    private GameObject GuestformPanel;
    private GameObject LobbyEntryPanel;

    private const string UUID_KEY = "GuestUUID";

    void Start()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        // 클래스 수준의 Guestform 변수를 초기화합니다.
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");

        // 정상적으로 찾아졌는지 확인하는 것이 좋습니다.
        if (GuestformPanel != null && LobbyEntryPanel != null && LobbyEntryPanel != null) { GuestformPanel.SetActive(false); }
        else { Debug.LogError("Panel을 찾을 수 없습니다. 이름을 확인하세요."); }

        GPGSBinder.Inst.Init((isLoggedIn, localUser) => {
            if (isLoggedIn)
            {
                Debug.Log("User is logged in." + localUser);
                SendGoogleLoginEventMessageToServer(localUser);
                // 게임의 로그인 후 로직 처리
                LoginPanel.SetActive(false);
                LobbyEntryPanel.SetActive(true);
            }
            else {
                Debug.Log("User failed to log in.");
                // 로그인 실패 시 처리할 로직
                LoginPanel.SetActive(true);
                LobbyEntryPanel.SetActive(false);
            }
        });
        GuestUUIDInit();
    }
    public void Logout()
    {
        GPGSBinder.Inst.Logout();
        FileManager.DeleteDataFile();
        LoginPanel.SetActive(true);
        LobbyEntryPanel.SetActive(false);
    }
    public void GooglePlayLogin() {
        GPGSBinder.Inst.Login((success, localUser) => {
            if (success) { SendGoogleLoginEventMessageToServer(localUser); }

            LoginPanel.SetActive(false);
            LobbyEntryPanel.SetActive(true);
        });
    }

    public void SendGoogleLoginEventMessageToServer(ILocalUser localUser)
    {
        var messageToSend = new
        {
            @event = "login",  // @ 기호를 사용하여 예약어 사용
            data = new
            {
                id = localUser.id,
                userName = localUser.userName,
                underage = localUser.underage,
            }
        };
        // JSON 문자열로 변환
        string jsonMessage = JsonMapper.ToJson(messageToSend);
        try {
            // 서버에 메시지 전송
            SocketBinder.Instance.GetWs().Send(jsonMessage);
            Console.WriteLine("서버로 메시지 전송: " + jsonMessage);
        }
        catch (InvalidOperationException ex) {
            // Log and handle the error
            Debug.LogError("WebSocket is not open: " + ex.Message);
        }
    }

    public void GuestLogin()
    {
        // 새 UUID 생성 및 저장
        string newUUID = Guid.NewGuid().ToString();

        try {
            FileManager.SaveData(UUID_KEY, newUUID);
            Debug.Log("New Guest UUID created and saved: " + newUUID);
        }
        catch (Exception e) {
            Debug.LogError("Failed to save UUID: " + e.Message);
            return; // 저장에 실패하면 메서드를 종료합니다.
        }

        TMP_InputField input = GuestformPanel.GetComponentInChildren<TMP_InputField>();

        if (input != null)
        {
            string playerName = input.text;
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("Player name is empty, please enter a name.");
                return; // 이름이 비어 있을 경우 메서드를 종료합니다.
            }

            try
            {
                FileManager.SaveData("GuestPlayerName", playerName);
                Debug.Log($"Guest Player Name saved: {playerName}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save player name: " + e.Message);
                return; // 저장에 실패하면 메서드를 종료합니다.
            }
        }
        else
        {
            Debug.LogError("TMP_InputField not found in GuestformPanel.");
            return; // 입력 필드를 찾지 못한 경우 메서드를 종료합니다.
        }

        ShowGuestLoginPanel();
        LoginPanel.SetActive(!LoginPanel.activeSelf);
        LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);

        input.text = ""; // 입력 필드 초기화
    }



    public void ShowGuestLoginPanel()
    {
        if (GuestformPanel != null)
        {  GuestformPanel.SetActive(!GuestformPanel.activeSelf); }
        else {  Debug.LogError("Guestform이 null입니다. 초기화에 문제가 있을 수 있습니다."); }
    }

    public void GuestUUIDInit()
    {
        // GameData를 로드
        GameData gamedata = FileManager.LoadData();

        if (gamedata != null && gamedata.dataDictionary.ContainsKey(UUID_KEY))
        {
            // UUID가 존재하면 로드된 UUID를 출력
            string existingUUID = gamedata.dataDictionary[UUID_KEY];
            Debug.Log("Existing Guest UUID: " + existingUUID);
            LoginPanel.SetActive(false);
            LobbyEntryPanel.SetActive(true);
        }
    }

    public void OnLobbyEnterButtonClicked()
    {
        // 로비 씬으로 전환
        sceneLoader.LoadScene("LobbyScene");
    }
}
