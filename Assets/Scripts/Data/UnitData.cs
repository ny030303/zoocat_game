using System.Collections.Generic;
using UnityEngine;

//�� ���ָ��� ������ �� ������ ���Ȱ� �� �������� �����ϴ� ����ü
[System.Serializable]
public struct StatIncrease
{
    public string statName;  // ���� �̸�
    public int increaseAmount; // ������
}

[CreateAssetMenu(fileName = "NewUnit", menuName = "scriptable Object/Create New Unit", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string id;             // ���� �Ǵ� ��ų�� ���� �ĺ���
    public string unitName;       // ���� �Ǵ� ��ų �̸�
    public int grade;             // ������ ���

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

    [Header("Render Setting")]
    public Collider2D target;
    public Sprite unitSprite;
    public GameObject unitPrefab; // ������ ���� �߰�

    [Header("Upgrade Information")]
    public int level = 1;         // ������ ���� (�⺻�� 1)
    public int upgradeCost = 100;       // ���׷��̵� ���
    public int maxUpgradeLevel = 5;     // �ִ�ġ

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

        // Ÿ��, ��������Ʈ, ������ ���� (�� ��ü���� �������̹Ƿ� ���� ���簡 �ʿ� ����)
        clone.target = this.target;
        clone.unitSprite = this.unitSprite;
        clone.unitPrefab = this.unitPrefab;

        clone.level = this.level;
        clone.upgradeCost = this.upgradeCost;
        clone.maxUpgradeLevel = this.maxUpgradeLevel;

        // List<StatIncrease> ����
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
