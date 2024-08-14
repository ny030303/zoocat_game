using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public UnitData unitData;
    public int currentHp;
    public GameObject damageTextPrefab; // DamageText ������ ���۷���

    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private Animator animator;
    private float totalDistance; // ��ü ����� �� ����
    private float requiredSpeed; // ������ �̵� �ӵ�
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    public void Initialize(List<Transform> waypoints, UnitData data)
    {

        gameManager = FindAnyObjectByType<GameManager>();
        unitData = data;
        currentHp = unitData.hp; // ������ ü���� �ʱ�ȭ
        this.waypoints = waypoints;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //������������ ������������ 1000�̶�� �����ϰ� MoveSPD ��ġ��ŭ 1�ʸ��� ����Ѵٰ� �����ϰ� ����
        CalculateTotalDistance(); //��� ��������Ʈ ���� �� �Ÿ��� ���
        requiredSpeed = totalDistance / unitData.moveSpeed; // 20�� ���� �̵��ϱ� ���� �ӵ�

        StartCoroutine(FadeIn()); // ������ ������ �� FadeIn
    }

    void Update()
    {
        if (currentHp > 0)
        {
            Move();
        }
    }

    void CalculateTotalDistance()
    {
        totalDistance = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        }
    }

    void Move()
    {
        if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
        {
            Destroy(gameObject); // Destroy if no waypoints or reached the end
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        float step = requiredSpeed * Time.deltaTime;

        // Update speed parameter for the Animator
        animator.SetFloat("Speed", requiredSpeed);

        if (direction.magnitude <= step)
        {
            transform.position = target.position;
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                gameManager.TakeDamage(true, 1); // Reduce player health
                Destroy(gameObject);
                //Die();
            }
        }
        else
        {
            transform.position += direction.normalized * step;
        }
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - unitData.def, 0);
        currentHp -= actualDamage;
        currentHp = Mathf.Max(currentHp, 0);

        ShowDamageText(actualDamage); // �������� ���� �� ������ �ؽ�Ʈ ǥ��

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void ShowDamageText(int damage)
    {

        // Instantiate the damage text prefab at the enemy's position
        GameObject damageText = Instantiate(damageTextPrefab, transform.position + new Vector3(0, (float)2, 0), Quaternion.identity);

        // Access the DamageTxt script attached to the damage text prefab
        DamageTextController damageTxtScript = damageText.GetComponent<DamageTextController>();

        // Set the damage value to the text
        damageTxtScript.damage = damage.ToString();

        // Optionally adjust the prefab's properties based on the type of damage (e.g., critical damage)
        if (damage > 10) // Example condition for critical damage
        {
            damageText.name = "CriticalDmgTxt";
        }
    }

    void Die()
    {
        // Update isDead parameter for the Animator
        animator.SetBool("IsDead", true);
        StartCoroutine(FadeOut()); // ����� �� FadeOut
        //Debug.Log(unitData.unitName + " has died.");
        Destroy(gameObject, 3f); // Destroy after 1 second to allow death animation to play
    }

    void OnDestroy()
    {
        gameManager.AddGold(unitData.rewardGold); // Reward gold
    }

    IEnumerator FadeIn()
    {
        float duration = 1f; // ���̵� �� ���� �ð�
        float currentTime = 0f;
        Color color = spriteRenderer.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            color.a = Mathf.Clamp01(currentTime / duration);
            spriteRenderer.color = color;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float duration = 1f; // ���̵� �ƿ� ���� �ð�
        float currentTime = 0f;
        Color color = spriteRenderer.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (currentTime / duration));
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject); // FadeOut �� ������Ʈ ����
    }
}
