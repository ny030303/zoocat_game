using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

//�� ���ָ��� ������ �� ������ ���Ȱ� �� �������� �����ϴ� ����ü
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
    public string id;             // ���� �Ǵ� ��ų�� ���� �ĺ���
    public string unitName;       // ���� �Ǵ� ��ų �̸�
    public int grade;             // ������ ���
    public string tag;               // UNIT | ENEMY �� ����

    [Header("Combat Stats")]
    public int atk;               // ���ݷ�
    public int hit;               // ���߷� (������� ǥ��)
    public float cri;             // ġ��Ÿ Ȯ�� (������� ǥ��)
    public float attackSpeed;       // ���� �ӵ�
    public float attackRange;     // ���� ����
    public float splashRange;     // ���÷��� ����
    public int hp;                // ü��
    public int def;               // ����
    public float moveSpeed;         // �̵� �ӵ�
    public int rewardGold;          // óġ ����

    [Header("Skill Information")]
    public string skillId;        // ��ų�� ���� �ĺ���
    public float skillValue;      // ��ų ȿ�� �� (������� ǥ��)
    public float skillDuration;   // ��ų ���� �ð� (�� ����)
    public float skillCooltime;   // ȿ�� �ߵ� �� �ٽ� �ߵ��Ǵµ� �ɸ��� �ð�(��)

    [Header("Render Setting")]
    public Sprite unitSprite;
    public GameObject unitPrefab; // ������ ���� �߰�
    public string idleSprite;
    public string attackSprite;
    public string walkSprite;
    public string dieSprite;

    [Header("Upgrade Information")]
    public int level = 1;         // ������ ���� (�⺻�� 1)
    public int upgradeCost = 100;       // ���׷��̵� ���
    public int maxUpgradeLevel = 5;     // �ִ�ġ

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
        // �ʿ信 ���� �߰�
    };

    public void ApplyStatIncrease(int level)
    {
        if (levelStatIncreases.TryGetValue(level, out var statIncrease))
        {   // ������ ����� ��ġ�� ������ ����
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
        // ���ο� �ν��Ͻ� ����
        UnitData clone = ScriptableObject.CreateInstance<UnitData>();

        // �⺻ �ʵ� ����
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

        // ������ ���� ���� (Ÿ��, ��������Ʈ, ������ ��)
        clone.unitSprite = this.unitSprite;
        clone.unitPrefab = this.unitPrefab;

        clone.level = this.level;
        clone.upgradeCost = this.upgradeCost;
        clone.maxUpgradeLevel = this.maxUpgradeLevel;

        // ��ųʸ� ����
        //foreach (var kvp in this.levelStatIncreases)
        //{
        //    // StatIncrease ��ü�� ���ο� �ν��Ͻ��� ����
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
