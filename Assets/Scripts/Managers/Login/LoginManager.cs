using LitJson;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader ��ũ��Ʈ�� ����

    private GameObject LoginPanel;
    private GameObject GuestformPanel;
    private GameObject LobbyEntryPanel;

    private const string UUID_KEY = "GuestUUID";

    void Start()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        // Ŭ���� ������ Guestform ������ �ʱ�ȭ�մϴ�.
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");

        // ���������� ã�������� Ȯ���ϴ� ���� �����ϴ�.
        if (GuestformPanel != null && LobbyEntryPanel != null && LobbyEntryPanel != null) { GuestformPanel.SetActive(false); }
        else { Debug.LogError("Panel�� ã�� �� �����ϴ�. �̸��� Ȯ���ϼ���."); }

        GPGSBinder.Inst.Init((isLoggedIn, localUser) => {
            if (isLoggedIn)
            {
                Debug.Log("User is logged in." + localUser);
                SendGoogleLoginEventMessageToServer(localUser);
                // ������ �α��� �� ���� ó��
                LoginPanel.SetActive(false);
                LobbyEntryPanel.SetActive(true);
            }
            else {
                Debug.Log("User failed to log in.");
                // �α��� ���� �� ó���� ����
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
            @event = "login",  // @ ��ȣ�� ����Ͽ� ����� ���
            data = new
            {
                id = localUser.id,
                userName = localUser.userName,
                underage = localUser.underage,
            }
        };
        // JSON ���ڿ��� ��ȯ
        string jsonMessage = JsonMapper.ToJson(messageToSend);
        try {
            // ������ �޽��� ����
            SocketBinder.Instance.GetWs().Send(jsonMessage);
            Console.WriteLine("������ �޽��� ����: " + jsonMessage);
        }
        catch (InvalidOperationException ex) {
            // Log and handle the error
            Debug.LogError("WebSocket is not open: " + ex.Message);
        }
    }

    public void GuestLogin()
    {
        // �� UUID ���� �� ����
        string newUUID = Guid.NewGuid().ToString();

        try {
            FileManager.SaveData(UUID_KEY, newUUID);
            Debug.Log("New Guest UUID created and saved: " + newUUID);
        }
        catch (Exception e) {
            Debug.LogError("Failed to save UUID: " + e.Message);
            return; // ���忡 �����ϸ� �޼��带 �����մϴ�.
        }

        TMP_InputField input = GuestformPanel.GetComponentInChildren<TMP_InputField>();

        if (input != null)
        {
            string playerName = input.text;
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("Player name is empty, please enter a name.");
                return; // �̸��� ��� ���� ��� �޼��带 �����մϴ�.
            }

            try
            {
                FileManager.SaveData("GuestPlayerName", playerName);
                Debug.Log($"Guest Player Name saved: {playerName}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save player name: " + e.Message);
                return; // ���忡 �����ϸ� �޼��带 �����մϴ�.
            }
        }
        else
        {
            Debug.LogError("TMP_InputField not found in GuestformPanel.");
            return; // �Է� �ʵ带 ã�� ���� ��� �޼��带 �����մϴ�.
        }

        ShowGuestLoginPanel();
        LoginPanel.SetActive(!LoginPanel.activeSelf);
        LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);

        input.text = ""; // �Է� �ʵ� �ʱ�ȭ
    }



    public void ShowGuestLoginPanel()
    {
        if (GuestformPanel != null)
        {  GuestformPanel.SetActive(!GuestformPanel.activeSelf); }
        else {  Debug.LogError("Guestform�� null�Դϴ�. �ʱ�ȭ�� ������ ���� �� �ֽ��ϴ�."); }
    }

    public void GuestUUIDInit()
    {
        // GameData�� �ε�
        GameData gamedata = FileManager.LoadData();

        if (gamedata != null && gamedata.dataDictionary.ContainsKey(UUID_KEY))
        {
            // UUID�� �����ϸ� �ε�� UUID�� ���
            string existingUUID = gamedata.dataDictionary[UUID_KEY];
            Debug.Log("Existing Guest UUID: " + existingUUID);
            LoginPanel.SetActive(false);
            LobbyEntryPanel.SetActive(true);
        }
    }

    public void OnLobbyEnterButtonClicked()
    {
        // �κ� ������ ��ȯ
        sceneLoader.LoadScene("LobbyScene");
    }
}
