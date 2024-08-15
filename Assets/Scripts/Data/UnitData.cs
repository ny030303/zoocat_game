using System.Collections.Generic;
using UnityEngine;

//각 유닛마다 레벨업 시 증가할 스탯과 그 증가량을 정의하는 구조체
[System.Serializable]
public struct StatIncrease
{
    public string statName;  // 스탯 이름
    public int increaseAmount; // 증가량
}

[CreateAssetMenu(fileName = "NewUnit", menuName = "scriptable Object/Create New Unit", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string id;             // 유닛 또는 스킬의 고유 식별자
    public string unitName;       // 유닛 또는 스킬 이름
    public int grade;             // 유닛의 등급

    [Header("Combat Stats")]
    public int atk;               // 공격력
    public int hit;               // 명중률 (백분율로 표현)
    public float cri;             // 치명타 확률 (백분율로 표현)
    public float attackSpeed;       // 공격 속도
    public float attackRange;     // 공격 범위
    public float splashRange;     // 스플래시 범위
    public int hp;                // 체력
    public int def;               // 방어력
    public float moveSpeed;         // 이동 속도
    public int rewardGold;          // 처치 보상

    [Header("Skill Information")]
    public string skillId;        // 스킬의 고유 식별자
    public float skillValue;      // 스킬 효과 값 (백분율로 표현)
    public float skillDuration;   // 스킬 지속 시간 (초 단위)

    [Header("Render Setting")]
    public Collider2D target;
    public Sprite unitSprite;
    public GameObject unitPrefab; // 프리펩 참조 추가

    [Header("Upgrade Information")]
    public int level = 1;         // 유닛의 레벨 (기본값 1)
    public int upgradeCost = 100;       // 업그레이드 비용
    public int maxUpgradeLevel = 5;     // 최대치

    [Header("Level Up Stat Increases")]
    public List<StatIncrease> statIncreases;
    private object axUpgradeLevel;

    // Method to Level Up the Unit
    public void LevelUp()
    {
        level++;
        foreach (var statIncrease in statIncreases)
        {
            ApplyStatIncrease(statIncrease);
        }
        upgradeCost += 100; // Example: Increase upgrade cost by 100 per level
    }

    private void ApplyStatIncrease(StatIncrease statIncrease)
    {
        switch (statIncrease.statName)
        {
            case "atk":
                atk += statIncrease.increaseAmount;
                break;
            case "hp":
                hp += statIncrease.increaseAmount;
                break;
            case "def":
                def += statIncrease.increaseAmount;
                break;
            // Add cases for other stats as needed
            default:
                Debug.LogWarning($"Unknown stat: {statIncrease.statName}");
                break;
        }
    }

    // Deep Copy Method
    public UnitData DeepCopy()
    {
        // 새로운 인스턴스 생성
        UnitData clone = ScriptableObject.CreateInstance<UnitData>();

        // 기본 필드 복사
        clone.id = this.id;
        clone.unitName = this.unitName;
        clone.grade = this.grade;

        clone.atk = this.atk;
        clone.hit = this.hit;
        clone.cri = this.cri;
        clone.attackSpeed = this.attackSpeed;
        clone.attackRange = this.attackRange;
        clone.splashRange = this.splashRange;
        clone.hp = this.hp;
        clone.def = this.def;
        clone.moveSpeed = this.moveSpeed;
        clone.rewardGold = this.rewardGold;

        clone.skillId = this.skillId;
        clone.skillValue = this.skillValue;
        clone.skillDuration = this.skillDuration;

        // 타겟, 스프라이트, 프리팹 복사 (이 객체들은 참조형이므로 깊은 복사가 필요 없다)
        clone.target = this.target;
        clone.unitSprite = this.unitSprite;
        clone.unitPrefab = this.unitPrefab;

        clone.level = this.level;
        clone.upgradeCost = this.upgradeCost;
        clone.maxUpgradeLevel = this.maxUpgradeLevel;

        // List<StatIncrease> 복사
        clone.statIncreases = new List<StatIncrease>(this.statIncreases.Count);
        foreach (var statIncrease in this.statIncreases)
        {
            clone.statIncreases.Add(new StatIncrease
            {
                statName = statIncrease.statName,
                increaseAmount = statIncrease.increaseAmount
            });
        }

        return clone;
    }
}
