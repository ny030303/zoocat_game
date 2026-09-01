using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class ChangeUnitPanel : MonoBehaviour
{
    public LobbyUserManager lobbyUserManager;

    private UnitData changedUnitData;
    private UserUnit changedUserUnit;
    public GameObject unitContainer; // ���� ������ ������Ʈ�� UI �����̳�
    public GameObject unitTemplate;  // ���� UI ���ø� (Prefab)
    public Sprite defaultSprite;    // �⺻ �̹��� (null�� �� ��ü)

    // 서버가 인정한 마지막 덱 (updateDeck 무변경 가드 + deckUpdateError 롤백용)
    private static string[] lastServerDeck;

    private void OnEnable()
    {
        SocketDispatcher.Instance.On(SocketEvents.DeckUpdated, OnDeckUpdated);
        SocketDispatcher.Instance.On(SocketEvents.DeckUpdateError, OnDeckUpdateError);
        if (lastServerDeck == null && UserManager.Instance != null && UserManager.Instance.currentUser != null)
            lastServerDeck = UserManager.Instance.currentUser.selectedUnits;
    }

    private void OnDisable()
    {
        if (SocketDispatcher.HasInstance)
        {
            SocketDispatcher.Instance.Off(SocketEvents.DeckUpdated, OnDeckUpdated);
            SocketDispatcher.Instance.Off(SocketEvents.DeckUpdateError, OnDeckUpdateError);
        }
    }

    private void OnDeckUpdated(JsonData data)
    {
        if (data != null && data.Has("newDeck") && data["newDeck"] != null && data["newDeck"].IsArray)
        {
            var arr = data["newDeck"];
            var deck = new string[arr.Count];
            for (int i = 0; i < arr.Count; i++) deck[i] = arr[i].ToString();
            lastServerDeck = deck;
        }
        Debug.Log("[Deck] server confirmed");
    }

    private void OnDeckUpdateError(JsonData data)
    {
        Debug.LogWarning("[Deck] update rejected - rolling back to last server deck");
        if (lastServerDeck != null && lobbyUserManager != null)
        {
            if (UserManager.Instance != null && UserManager.Instance.currentUser != null)
                UserManager.Instance.currentUser.selectedUnits = lastServerDeck;
            lobbyUserManager.InitializeUnitDeck(lastServerDeck);
            lobbyUserManager.OnUnitDeckChanged?.Invoke(lobbyUserManager.unitDatabase.unitDeck);
        }
        // 토스트는 GlobalErrorHandler 가 표시
    }

    public void OnShowPanel(UnitData unit, UserUnit userUnit)
    {
        // ���� �����̳� �ʱ�ȭ (���� UI Ŭ����)
        foreach (Transform child in unitContainer.transform) { Destroy(child.gameObject); }
        changedUnitData = unit;
        changedUserUnit = userUnit;
        // ���� UI ����
        UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);
        GameObject unitObject = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

        // ���� ǥ��
        Transform levelchild = unitObject.transform.Find("Level");
        if (levelchild != null)
        {
            TMP_Text levelText = levelchild.GetComponent<TMP_Text>();
            levelText.text = $"Lv. {userUnit.lv}";
        }
    }

    public void ChangeUnit(UnitData unit)
    {
        if (lobbyUserManager == null || lobbyUserManager.unitDatabase == null)
        {
            Debug.LogError("LobbyUserManager �Ǵ� UnitDatabase�� �������� �ʾҽ��ϴ�.");
            return;
        }

        List<UnitData> unitDeck = lobbyUserManager.unitDatabase.unitDeck;

        if (unitDeck.Contains(changedUnitData)) // ���� ������ ���� ���� ���
        {
            int index = unitDeck.IndexOf(changedUnitData);
            if (unitDeck.Contains(unit)) // Ŭ���� ������ ���� �ִ� ���
            {
                int unitIndex = unitDeck.IndexOf(unit);
                unitDeck[unitIndex] = changedUnitData; // ���� ������ Ŭ���� ������ �ִ� ��ġ�� �̵�
            }
            unitDeck[index] = unit; // Ŭ���� �������� ����
            Debug.Log($"{changedUnitData.unitName}��(��) {unit.unitName}���� ��ü��.");
        }
        else if (unitDeck.Contains(unit)) // Ŭ���� ������ ���� ���� ���
        {
            int index = unitDeck.IndexOf(unit);
            unitDeck[index] = changedUnitData; // Ŭ���� ���� ��ġ�� ���� changedUnitData�� ����
            Debug.Log($"{unit.unitName}��(��) {changedUnitData.unitName}���� ��ü��.");
        }

        lobbyUserManager.OnUnitDeckChanged?.Invoke(unitDeck); // UI ���� �̺�Ʈ ȣ��

        // ����� ���� �����͸� �ݿ�
        changedUnitData = unit;

        if (UserManager.Instance == null)
        {
            Debug.LogWarning("UserManager.Instance is null!");
            return;
        }

        UserData userdata = UserManager.Instance.currentUser;
        userdata.selectedUnits = unitDeck.ConvertAll(u => u.id.Replace("CHA_", "")).ToArray(); // UserData�� selectedUnits�� ID���� "CHA_" ���� �� ����

        if (UserManager.Instance.isGuest == 0)
        {
            FileManager.SaveUserData(userdata); // ���� ���� ����
            UserManager.Instance.currentUser = FileManager.LoadUserData(); // ����� ������ �ٽ� �ε�
            Debug.Log("�� ���� ���� ���� UserManager ������Ʈ �Ϸ�.");
        } else
        {
            SendUpdateDeckEventMessageToServer(userdata, userdata.selectedUnits);
        }
        
    }


    private void SendUpdateDeckEventMessageToServer(UserData currentUser, string[] newDeck)
    {
        // 서버가 인정한 마지막 덱과 같으면 전송 안 함 (서버 "Failed to update deck" 회피)
        if (lastServerDeck != null && newDeck != null && newDeck.Length == lastServerDeck.Length)
        {
            bool same = true;
            for (int i = 0; i < newDeck.Length; i++)
                if (newDeck[i] != lastServerDeck[i]) { same = false; break; }
            if (same) { Debug.Log("[Deck] no change - skip updateDeck"); return; }
        }

        SocketBinder.Instance.SendWhenAuthed(SocketEvents.UpdateDeck, new { newDeck });
    }
}
