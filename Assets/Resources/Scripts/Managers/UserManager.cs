using LitJson;
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
    public int unlock; // 1 = 잠금 해제, 0 = 잠금
    public int lv;     // 유닛 레벨
    public int exp;    // 유닛 경험치
    public int piece;  // 유닛 조각 수

    // 기본 생성자 추가
    public UserUnit() { }
}


public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public int isGuest = 0; // 0 = guest, 1 = 구글 게임즈 계정연결 유저
    public UserData currentUser;
    public UserUnit[] units;  // 유저의 유닛 데이터 배열

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

    // 유저 데이터를 JSON으로부터 로드하는 메서드
    public void LoadUserFromJson(JsonData jsonData)
    {
        // LitJson을 사용해 JSON 데이터를 UserData 객체로 변환
        currentUser = JsonMapper.ToObject<UserData>(jsonData.ToJson());
        Debug.Log("User data loaded using LitJson.");
    }

    // 유저의 유닛 데이터를 JSON으로부터 로드하는 메서드
    public void LoadUserUnitsFromJson(JsonData jsonData)
    {
        // LitJson을 사용해 JSON 데이터를 UserUnit[] 배열로 변환
        units = JsonMapper.ToObject<UserUnit[]>(jsonData.ToJson());
        Debug.Log("User units loaded using LitJson.");
    }

    public void UpdateUserScore(int newScore)
    {
        currentUser.experience += newScore;
    }

    // 유닛 데이터를 업데이트하는 메서드
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
