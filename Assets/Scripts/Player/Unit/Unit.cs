using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public int currentHp;
    public GameObject bulletPrefab;  // 불릿 프리팹 레퍼런스

    private float nextAttackTime = 0f; // 다음 공격 시간
    private Skill unitSkill;
    private float elapsedTime = 0f;
    private int attackCount = 0;
    private float damageTaken = 0f;

    private Animator animator;

    public delegate void UnitDestroyedHandler();
    public event UnitDestroyedHandler OnUnitDestroyed;
    void Start()
    {
        animator = GetComponent<Animator>();
        //if (unitData != null)
        //{
        //    // 유닛의 이름과 프리팹을 출력
        //    //Debug.Log("Unit Name: " + unitData.unitName);
        //}
    }
    public void Initialize(UnitData data)
    {
        unitData = data;
        currentHp = unitData.hp; // 유닛의 체력을 초기화
        unitSkill = new Skill(data);  // UnitData에서 스킬 생성
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
    public bool IsAlive()
    {
        return currentHp > 0;
    }

    // 데미지 받았을때
    public void TakeDamage(int damage)
    {
        currentHp -= Mathf.Max(damage - unitData.def, 0); // 방어력을 감안한 데미지 계산
        currentHp = Mathf.Max(currentHp, 0); // 체력이 음수로 내려가지 않도록 처리
    }

    public int CalculateDamage()
    {
        int damage = unitData.atk;

        // 치명타 발생 확률 체크
        if (Random.value < unitData.cri / 100f)
        {
            damage *= 2; // 치명타 시 공격력 2배
            Debug.Log(unitData.unitName + " has landed a critical hit!");
        }

        return damage;
    }

    public void Attack(Enemy target)
    {
        animator.SetTrigger("AttackTrigger"); // 공격 애니메이션을 트리거

        attackCount++;
        //target.TakeDamage(CalculateDamage()); // 공격 부여하는 방식
        ThrowBullet(target, CalculateDamage());
        //Debug.Log(unitData.unitName + " attacks " + target.unitData.unitName + " for " + damage + " damage.");

        CheckAndApplySkill();
    }
    private void ThrowBullet(Enemy target, int damage)
    {
        Vector2 spawnPosition = new Vector2(this.gameObject.transform.position.x -1f, this.gameObject.transform.position.y - 0.7f); // x축으로 0.5 단위 이동
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, this.gameObject.transform.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(target.transform, damage);
    }

    private void CheckAndApplySkill()
    {
        if (unitSkill.CheckCondition(this, elapsedTime, attackCount, damageTaken))
        {
            unitSkill.ApplySkill(this);
        }
    }

    public void UpdateTime(float deltaTime)
    {
        elapsedTime += deltaTime;
        CheckAndApplySkill();
    }
    public void AttackClosestEnemy()
    {

        Enemy closestEnemy = FindClosestEnemy();

        if (closestEnemy != null)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack(closestEnemy);
                nextAttackTime = Time.time + 1f / unitData.attackSpeed; // 다음 공격 시간을 현재 시간에 공격 속도를 반영하여 설정
            }
            
        }
    }

    private Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>(); // 씬 내의 모든 몬스터 찾기
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
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

    void Update()
    {

        AttackClosestEnemy();
       //UpdateTime(Time.deltaTime);  // 스킬 적용을 위한 시간 업데이트
    }
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
