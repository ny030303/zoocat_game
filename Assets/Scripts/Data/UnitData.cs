using UnityEngine;

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
    internal object transform;

    // Deep Copy Method
    public UnitData DeepCopy()
    {
        // 새로운 인스턴스 생성
        UnitData clone = ScriptableObject.CreateInstance<UnitData>();

        // 모든 필드를 개별적으로 복사
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

        clone.target = this.target;
        clone.unitSprite = this.unitSprite;
        clone.unitPrefab = this.unitPrefab;

        return clone;
    }
}
