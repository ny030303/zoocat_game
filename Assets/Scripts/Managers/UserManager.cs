using LitJson;
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
    public int[] selectedUnits;
    public int gold;
    public int gems;
}

[System.Serializable]
public class UserUnit
{
    public int id;
    public int unlock; // 1 = ��� ����, 0 = ���
    public int lv;     // ���� ����
    public int exp;    // ���� ����ġ
    public int piece;  // ���� ���� ��
}

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public UserData currentUser;
    public UserUnit[] units;  // ������ ���� ������ �迭

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
    public void UpdateUnitData(int unitId, int newLevel, int newExp)
    {
        foreach (var unit in units)
        {
            if (unit.id == unitId)
            {
                unit.lv = newLevel;
                unit.exp = newExp;
                Debug.Log($"Updated unit {unitId} to level {newLevel} with {newExp} exp.");
                break;
            }
        }
    }
}
