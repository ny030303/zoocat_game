using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData
{
    public string id;
    public bool underage;
    public string username;
    public int level;
    public int experience;
    public string[] friends;
    public string country;
    public string language;
    public string[] selectedUnits;
    public int gold;
    public int gems;
}

[System.Serializable]
public class UserUnit
{
    public string id;
    public int unlock; // 1 = ��� ����, 0 = ���
    public int lv;     // ���� ����
    public int exp;    // ���� ����ġ
    public int piece;  // ���� ���� ��

    // �⺻ ������ �߰�
    public UserUnit() { }
}


public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public int isGuest = 0;          // 0 = 서버 세션 없음, 1 = 서버 세션 있음 (게스트 포함)
    public bool isAnonymous = true;  // true = UUID 게스트(계정 미연동), false = 구글 연동
    public UserData currentUser;
    public UserUnit[] units;  // ������ ���� ������ �迭 -> ��ȭ���, ������Ȳ ��


    public Action<UserUnit[]> OnUserUnitListChanged;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���� �����͸� JSON���κ��� �ε��ϴ� �޼���
    public void LoadUserFromJson(JsonData jsonData)
    {
        // LitJson�� ����� JSON �����͸� UserData ��ü�� ��ȯ
        currentUser = JsonMapper.ToObject<UserData>(jsonData.ToJson());
        Debug.Log("User data loaded using LitJson.");
    }

    // ������ ���� �����͸� JSON���κ��� �ε��ϴ� �޼���
    public void LoadUserUnitsFromJson(JsonData jsonData)
    {
        // LitJson�� ����� JSON �����͸� UserUnit[] �迭�� ��ȯ
        units = JsonMapper.ToObject<UserUnit[]>(jsonData.ToJson());
        Debug.Log("User units loaded using LitJson.");
    }

    public void UpdateUserScore(int newScore)
    {
        currentUser.experience += newScore;
    }

    // ���� �����͸� ������Ʈ�ϴ� �޼���
    public void UpdateUnitData(string unitId, int newLevel, int newExp)
    {
        foreach (var unit in units)
        {
            if (unit.id.Equals(unitId))
            {
                unit.lv = newLevel;
                unit.exp = newExp;
                Debug.Log($"Updated unit {unitId} to level {newLevel} with {newExp} exp.");
                break;
            }
        }
    }
}
