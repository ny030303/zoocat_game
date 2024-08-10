using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "scriptable Object/Create New Unit", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int hp; 
    public int atk; //attackPower
    public float def;// 유닛에게 공격 받았을때 감소하는 수치
    public float spd; //공격을 하는 속도를 나타내는 수치
    public float move; //공격을 하는 속도를 나타내는 수치
    public Collider2D target;
    public float skill;
    public Sprite unitSprite;
    public GameObject unitPrefab; // 프리펩 참조 추가
    internal object transform;
}
