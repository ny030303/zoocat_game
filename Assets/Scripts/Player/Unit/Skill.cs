using UnityEngine;

public class Skill
{
    public string skillId;
    public string skillName;
    public float skillValue;      // ��ų�� ȿ�� ��
    public float skillDuration;   // ��ų�� ���� �ð�
    public string conditionType;  // �ߵ� ���� ���� (��: "HP", "AttackCount", "TimeElapsed", "DamageTaken")
    public float conditionValue;  // ���ǿ� �ش��ϴ� �� (��: HP 30% ����)

    public Skill(UnitData data)
    {
        this.skillId = data.skillId;
        this.skillName = data.skillId;  // ��ų �̸��� ��ų ID�� ��� ���� (�ʿ�� �̸����� ��ü ����)
        this.skillValue = data.skillValue;
        this.skillDuration = data.skillDuration;

        // Ư�� �ߵ� ���� ���� (�������� HP �������� ����)
        this.conditionType = "HP";  // �� �κ��� �ʿ信 ���� �ٸ��� ���� ����
        this.conditionValue = 0.3f; // �������� HP�� 30% ������ �� �ߵ�
    }

    public bool CheckCondition(Unit unit, float elapsedTime, int attackCount, float damageTaken)
    {
        switch (conditionType)
        {
            case "HP":
                return unit.currentHp <= unit.unitData.hp * conditionValue;
            case "AttackCount":
                return attackCount >= conditionValue;
            case "TimeElapsed":
                return elapsedTime >= conditionValue;
            case "DamageTaken":
                return damageTaken >= conditionValue;
            default:
                return false;
        }
    }

    public void ApplySkill(Unit target)
    {
        // ��ų ȿ�� ���� ���� (��: ���ݷ� ����, �̵� �ӵ� ���� ��)
        Debug.Log(target.unitData.unitName + " used skill: " + skillName);
    }
}
