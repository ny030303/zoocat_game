using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public delegate void UnitDestroyedHandler();
    public event UnitDestroyedHandler OnUnitDestroyed;
    void Start()
    {
        // ������ �̸��� �������� ���
        if (unitData != null)
        {
            //Debug.Log("Unit Name: " + unitData.unitName);
            //Instantiate(unitData.unitPrefab, transform.position, Quaternion.identity);
        }
    }
    public void Initialize(UnitData data)
    {
        unitData = data;
        // ������ �ʱ�ȭ �۾��� �����մϴ�.
        // ��: ü�� �ʱ�ȭ, ���ǵ� ���� ��
    }

    public void Kill()
    {
        // ������ �ı��� �� �̺�Ʈ �߻�
        if (OnUnitDestroyed != null)
        {
            OnUnitDestroyed();
        }
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        //// ������ �ı��� �� �̺�Ʈ �߻�
        //if (OnUnitDestroyed != null)
        //{
        //    OnUnitDestroyed();
        //}
    }

    //public void UpgradeUnit()
    //{
    //    if (unitData != null && unitData.upgradedUnit != null)
    //    {
    //        // ���׷��̵�� ������ �����͸� ���� ���� �����ͷ� ����
    //        unitData = unitData.upgradedUnit;

    //        // ���� �������� �ı��ϰ� ���ο� �������� ����
    //        Destroy(gameObject);
    //        Instantiate(unitData.unitPrefab, transform.position, Quaternion.identity);
    //    }
    //}

    //void Update()
    //{
    //    if (Time.time >= nextAttackTime)
    //    {
    //        Attack();
    //        nextAttackTime = Time.time + attackSpeed;
    //    }
    //}

    //void Attack()
    //{
    //    // ���� ����� ���͸� ã�� ����
    //}

    //public void Upgrade()
    //{
    //    if (level < 5)
    //    {
    //        level++;
    //        attackPower += 10; // ���ݷ� ����
    //        // �÷��̾��� ��带 ����
    //    }
    //}
}
