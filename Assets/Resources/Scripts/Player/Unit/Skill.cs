using UnityEngine;

public class Skill
{
    public string skillId;
    public string skillName;
    public float skillValue;      // 스킬의 효과 값
    public float skillDuration;   // 스킬의 지속 시간
    public string conditionType;  // 발동 조건 유형 (예: "HP", "AttackCount", "TimeElapsed", "DamageTaken")
    public float conditionValue;  // 조건에 해당하는 값 (예: HP 30% 이하)

    public Skill(UnitData data)
    {
        this.skillId = data.skillId;
        this.skillName = data.skillId;  // 스킬 이름을 스킬 ID로 대신 설정 (필요시 이름으로 대체 가능)
        this.skillValue = data.skillValue;
        this.skillDuration = data.skillDuration;

        // 특정 발동 조건 설정 (예제에서 HP 조건으로 설정)
        this.conditionType = "HP";  // 이 부분은 필요에 따라 다르게 설정 가능
        this.conditionValue = 0.3f; // 예제에서 HP가 30% 이하일 때 발동
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
        // 스킬 효과 적용 로직 (예: 공격력 증가, 이동 속도 감소 등)
        Debug.Log(target.unitData.unitName + " used skill: " + skillName);
    }
}
