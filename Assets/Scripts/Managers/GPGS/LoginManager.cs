using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader ��ũ��Ʈ�� ����

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
        // Ŭ���� ������ Guestform ������ �ʱ�ȭ�մϴ�.
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");

        // ���������� ã�������� Ȯ���ϴ� ���� �����ϴ�.
        if (GuestformPanel != null && LobbyEntryPanel != null)
        {
            GuestformPanel.SetActive(false);
            LobbyEntryPanel.SetActive(false);
        }
        else { Debug.LogError("Panel�� ã�� �� �����ϴ�. �̸��� Ȯ���ϼ���."); }
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
            Debug.Log(log);  // ������� ���� �α��� ����� ����մϴ�.
            LoginPanel.SetActive(!LoginPanel.activeSelf);
            LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        });
    }

    public void GuestLogin()
    {
        // �� UUID ���� �� ����
        string newUUID = Guid.NewGuid().ToString();

        try
        {
            FileManager.SaveData(UUID_KEY, newUUID);
            Debug.Log("New Guest UUID created and saved: " + newUUID);
        }
        catch (Exception e)
        {
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
            LoginPanel.SetActive(!LoginPanel.activeSelf);
            LobbyEntryPanel.SetActive(!LobbyEntryPanel.activeSelf);
        }
    }

    public void OnLobbyEnterButtonClicked()
    {
        // �κ� ������ ��ȯ
        sceneLoader.LoadScene("LobbyScene");
    }
}
