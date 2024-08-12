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

    [Header("Skill Information")]
    public string skillId;        // ��ų�� ���� �ĺ���
    public float skillValue;      // ��ų ȿ�� �� (������� ǥ��)
    public float skillDuration;   // ��ų ���� �ð� (�� ����)

    [Header("Render Setting")]
    public Collider2D target;
    public Sprite unitSprite;
    public GameObject unitPrefab; // ������ ���� �߰�
    internal object transform;
}
