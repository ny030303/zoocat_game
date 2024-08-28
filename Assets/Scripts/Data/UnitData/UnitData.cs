using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

//각 유닛마다 레벨업 시 증가할 스탯과 그 증가량을 정의하는 구조체
[System.Serializable]
public class StatIncrease
{
    public float AtkIncrease { get; set; }
    public float CriIncrease { get; set; }
    public float AttackSpeedIncrease { get; set; }
}

[CreateAssetMenu(fileName = "NewUnit", menuName = "scriptable Object/Create New Unit", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string id;             // 유닛 또는 스킬의 고유 식별자
    public string unitName;       // 유닛 또는 스킬 이름
    public int grade;             // 유닛의 등급
    public string tag;               // UNIT | ENEMY 로 구별

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
    public float skillCooltime;   // 효과 발동 후 다시 발동되는데 걸리는 시간(초)

    [Header("Render Setting")]
    public Sprite unitSprite;
    public GameObject unitPrefab; // 프리펩 참조 추가
    public string idleSprite;
    public string attackSprite;
    public string walkSprite;
    public string dieSprite;

    [Header("Upgrade Information")]
    public int level = 1;         // 유닛의 레벨 (기본값 1)
    public int upgradeCost = 100;       // 업그레이드 비용
    public int maxUpgradeLevel = 5;     // 최대치

    // Method to Level Up the Unit
    public void LevelUp()
    {
        level++;
        ApplyStatIncrease(level);
        // Example: Increase upgrade cost by 100 per level
        upgradeCost += 100; 
    }
    

    private readonly Dictionary<int, StatIncrease> levelStatIncreases = new Dictionary<int, StatIncrease>
    {
        { 1, new StatIncrease { AtkIncrease = 10f, CriIncrease = 0f, AttackSpeedIncrease = 5f } },
        { 2, new StatIncrease { AtkIncrease = 20f, CriIncrease = 5f, AttackSpeedIncrease = 10f } },
        { 3, new StatIncrease { AtkIncrease = 30f, CriIncrease = 8f, AttackSpeedIncrease = 15f } },
        { 4, new StatIncrease { AtkIncrease = 40f, CriIncrease = 10f, AttackSpeedIncrease = 20f } },
        { 5, new StatIncrease { AtkIncrease = 50f, CriIncrease = 15f, AttackSpeedIncrease = 30f } }
        // 필요에 따라 추가
    };

    public void ApplyStatIncrease(int level)
    {
        if (levelStatIncreases.TryGetValue(level, out var statIncrease))
        {   // 기존에 적용된 수치에 비율을 적용
            atk += (int)(atk * (statIncrease.AtkIncrease / 100f));
            cri += (cri * (statIncrease.CriIncrease / 100f));
            attackSpeed += (attackSpeed * (statIncrease.AttackSpeedIncrease / 100f));
        }
        else
        {
            //Debug.LogWarning($"Unknown level: {level}");
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
        clone.skillCooltime = this.skillCooltime;

        // 참조형 변수 복사 (타겟, 스프라이트, 프리팹 등)
        clone.unitSprite = this.unitSprite;
        clone.unitPrefab = this.unitPrefab;

        clone.level = this.level;
        clone.upgradeCost = this.upgradeCost;
        clone.maxUpgradeLevel = this.maxUpgradeLevel;

        // 딕셔너리 복사
        //foreach (var kvp in this.levelStatIncreases)
        //{
        //    // StatIncrease 객체도 새로운 인스턴스로 복사
        //    clone.levelStatIncreases.Add(kvp.Key, new StatIncrease
        //    {
        //        AtkIncrease = kvp.Value.AtkIncrease,
        //        CriIncrease = kvp.Value.CriIncrease,
        //        AttackSpeedIncrease = kvp.Value.AttackSpeedIncrease
        //    });
        //}

        return clone;
    }
}
