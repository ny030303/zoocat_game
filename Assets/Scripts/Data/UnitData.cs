using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "scriptable Object/Create New Unit", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int hp; 
    public int atk; //attackPower
    public float def;// ���ֿ��� ���� �޾����� �����ϴ� ��ġ
    public float spd; //������ �ϴ� �ӵ��� ��Ÿ���� ��ġ
    public float move; //������ �ϴ� �ӵ��� ��Ÿ���� ��ġ
    public Collider2D target;
    public float skill;
    public Sprite unitSprite;
    public GameObject unitPrefab; // ������ ���� �߰�
    internal object transform;
}
