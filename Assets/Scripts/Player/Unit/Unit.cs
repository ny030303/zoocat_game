using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public int currentHp;
    public GameObject bulletPrefab;  // �Ҹ� ������ ���۷���

    private float nextAttackTime = 0f; // ���� ���� �ð�
    private Skill unitSkill;
    private float elapsedTime = 0f;
    private int attackCount = 0;
    private float damageTaken = 0f;

    private Animator animator;

    public int level;
    public delegate void UnitDestroyedHandler();
    public event UnitDestroyedHandler OnUnitDestroyed;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Initialize(UnitData data)
    {
        unitData = data.DeepCopy();
        currentHp = unitData.hp; // ������ ü���� �ʱ�ȭ
        unitSkill = new Skill(data);  // UnitData���� ��ų ����
    }

    internal void LevelUpgrade()
    {
        throw new System.NotImplementedException();
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
    public bool IsAlive()
    {
        return currentHp > 0;
    }

    // ������ �޾�����
    public void TakeDamage(int damage)
    {
        currentHp -= Mathf.Max(damage - unitData.def, 0); // ������ ������ ������ ���
        currentHp = Mathf.Max(currentHp, 0); // ü���� ������ �������� �ʵ��� ó��
    }

    public int CalculateDamage()
    {
        int damage = unitData.atk;

        // ġ��Ÿ �߻� Ȯ�� üũ
        if (Random.value < unitData.cri / 100f)
        {
            damage *= 2; // ġ��Ÿ �� ���ݷ� 2��
            //Debug.Log(unitData.unitName + " has landed a critical hit!");
        }

        return damage;
    }

    public void Attack(Enemy target)
    {
        animator.SetTrigger("AttackTrigger"); // ���� �ִϸ��̼��� Ʈ����

        attackCount++;
        // ���� �ο��ϴ� ���
        //target.TakeDamage(CalculateDamage());
        ThrowBullet(target, CalculateDamage());
        CheckAndApplySkill();
    }
    private void ThrowBullet(Enemy target, int damage)
    {
        // ���ư��� �ҷ� ��ġ���� - ���� �������� �̵�
        Vector2 spawnPosition = new Vector2(this.gameObject.transform.position.x -1f, this.gameObject.transform.position.y - 0.7f); 
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, this.gameObject.transform.rotation, this.gameObject.transform.parent.parent);
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
                nextAttackTime = Time.time + 1f / unitData.attackSpeed; // ���� ���� �ð��� ���� �ð��� ���� �ӵ��� �ݿ��Ͽ� ����
            }
            
        }
    }

    private Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>(); // �� ���� ��� ���� ã��
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
    public void UpgradeUnitMerged(UnitData beforeUnitData)
    {
        Debug.Log("beforeUnitData.grade: " + beforeUnitData.grade);
        if (unitData != null && unitData.grade < 5)
        {
            unitData.grade = beforeUnitData.grade + 1;
            unitData.atk = beforeUnitData.atk + 10;
            // ����� �ö� �� ũ�� ����
            float scaleFactor = 1.0f + (unitData.grade * 0.1f);
            this.gameObject.transform.localScale *= scaleFactor;
        }
    }

    void Update()
    {

        AttackClosestEnemy();
       //UpdateTime(Time.deltaTime);  // ��ų ������ ���� �ð� ������Ʈ
    }
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
