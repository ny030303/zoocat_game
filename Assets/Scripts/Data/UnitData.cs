using UnityEngine;

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
    internal object transform;

    // Deep Copy Method
    public UnitData DeepCopy()
    {
        // ���ο� �ν��Ͻ� ����
        UnitData clone = ScriptableObject.CreateInstance<UnitData>();

        // ��� �ʵ带 ���������� ����
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
