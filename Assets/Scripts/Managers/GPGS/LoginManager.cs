using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader 스크립트를 참조

    private GameObject LoginPanel;
    private GameObject GuestformPanel;
    private GameObject LobbyEntryPanel;

    private const string UUID_KEY = "GuestUUID";

    void Start()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        // 클래스 수준의 Guestform 변수를 초기화합니다.
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");

        // 정상적으로 찾아졌는지 확인하는 것이 좋습니다.
        if (GuestformPanel != null && LobbyEntryPanel != null)
        {
            GuestformPanel.SetActive(false);
            LobbyEntryPanel.SetActive(false);
        }
        else { Debug.LogError("Panel을 찾을 수 없습니다. 이름을 확인하세요."); }
        if(GPGSBinder.Inst.IsLoggedIn())
        {
            LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        }
        GuestUUIDInit();
    }
    public void Logout()
    {
        GPGSBinder.Inst.Logout();
        FileManager.DeleteDataFile();
        LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        LoginPanel.SetActive(!LoginPanel.activeSelf);
    }
    public void GooglePlayLogin()
    {
        GPGSBinder.Inst.Login((success, localUser) =>
        {
            string log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}";
            Debug.Log(log);  // 디버깅을 위해 로그인 결과를 출력합니다.
            LoginPanel.SetActive(!LoginPanel.activeSelf);
            LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        });
    }

    public void GuestLogin()
    {
        // 새 UUID 생성 및 저장
        string newUUID = Guid.NewGuid().ToString();

        try
        {
            FileManager.SaveData(UUID_KEY, newUUID);
            Debug.Log("New Guest UUID created and saved: " + newUUID);
        }
        catch (Exception e)
        {
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
            LoginPanel.SetActive(!LoginPanel.activeSelf);
            LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        }
    }

    public void OnLobbyEnterButtonClicked()
    {
        // 로비 씬으로 전환
        sceneLoader.LoadScene("LobbyScene");
    }
}
