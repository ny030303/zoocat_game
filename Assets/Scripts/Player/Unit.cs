using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public delegate void UnitDestroyedHandler();
    public event UnitDestroyedHandler OnUnitDestroyed;
    void Start()
    {
        // 유닛의 이름과 프리팹을 출력
        if (unitData != null)
        {
            //Debug.Log("Unit Name: " + unitData.unitName);
            //Instantiate(unitData.unitPrefab, transform.position, Quaternion.identity);
        }
    }
    public void Initialize(UnitData data)
    {
        unitData = data;
        // 유닛의 초기화 작업을 수행합니다.
        // 예: 체력 초기화, 스피드 설정 등
    }

    public void Kill()
    {
        // 유닛이 파괴될 때 이벤트 발생
        if (OnUnitDestroyed != null)
        {
            OnUnitDestroyed();
        }
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        //// 유닛이 파괴될 때 이벤트 발생
        //if (OnUnitDestroyed != null)
        //{
        //    OnUnitDestroyed();
        //}
    }

    //public void UpgradeUnit()
    //{
    //    if (unitData != null && unitData.upgradedUnit != null)
    //    {
    //        // 업그레이드된 유닛의 데이터를 현재 유닛 데이터로 설정
    //        unitData = unitData.upgradedUnit;

    //        // 기존 프리팹을 파괴하고 새로운 프리팹을 생성
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
    //    // 가장 가까운 몬스터를 찾아 공격
    //}

    //public void Upgrade()
    //{
    //    if (level < 5)
    //    {
    //        level++;
    //        attackPower += 10; // 공격력 증가
    //        // 플레이어의 골드를 차감
    //    }
    //}
}
