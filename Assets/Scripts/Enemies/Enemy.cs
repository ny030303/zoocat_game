using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public UnitData unitData;
    public bool isDie;
    public int currentHp;
    public GameObject damageTextPrefab; // DamageText ������ ���۷���

    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private Animator animator;
    private float totalDistance;
    private float requiredSpeed; // ������ �̵� �ӵ�
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager; 
    private float startTime;
    private string owner;
    public void Initialize(string owner, List<Transform> waypoints, UnitData data, float statMultiplier)
    {

        gameManager = FindAnyObjectByType<GameManager>();
        unitData = data.DeepCopy();
        unitData.hp = (int)(unitData.hp * statMultiplier);
        unitData.def = (int)(unitData.def * statMultiplier);
        unitData.moveSpeed = unitData.moveSpeed * statMultiplier;

        currentHp = unitData.hp;
        this.waypoints = waypoints;
        this.owner = owner;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ��� ��������Ʈ ���� �� �Ÿ��� ���
        CalculateTotalDistance();

        // requiredSpeed�� ������ ��ǥ �ð��� �����ϵ��� ���
        requiredSpeed = totalDistance / (1000f / unitData.moveSpeed); // 1000�� �̵��Ϸ��� moveSpeed�� ���� �ð��� �����ϵ��� ����
        startTime = Time.time;  // ������ �̵��� ������ �ð� ���
        StartCoroutine(FadeIn());
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
            //float endTime = Time.time;
            //float totalTime = endTime - startTime;
            //Debug.Log($"������ �����߽��ϴ�. �� ��� �ð�: {totalTime}��");
            Destroy(gameObject); // ��������Ʈ�� ���ų� ������ ��������Ʈ�� ������ ��� ��ü �ı�
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        float step = requiredSpeed * Time.deltaTime; // ���⿡ ���� requiredSpeed�� ����մϴ�.

        // Animator�� �ӵ� �Ķ���� ������Ʈ
        animator.SetFloat("Speed", requiredSpeed);

        if (direction.magnitude <= step)
        {
            transform.position = target.position;
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                //float endTime = Time.time;
                //float totalTime = endTime - startTime;
                //Debug.Log($"������ �����߽��ϴ�. �� ��� �ð�: {totalTime}��");
                gameManager.TakeDamage(owner); // �÷��̾��� ü���� ���ҽ�ŵ�ϴ�.
                Destroy(gameObject);
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

        if (currentHp <= 0 && !isDie) 
        {
            Die();
        }
    }

    void ShowDamageText(int damage)
    {

        // Instantiate the damage text prefab at the enemy's position
        GameObject damageText = Instantiate(damageTextPrefab, transform.position + new Vector3(0, (float)2, 0), Quaternion.identity, this.gameObject.transform.parent.parent);

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
        isDie = true;
        animator.SetBool("IsDead", true);
        StartCoroutine(FadeOut()); // ����� �� FadeOut
        //Debug.Log(unitData.unitName + " has died.");
        Destroy(gameObject, 3f); // Destroy after 1 second to allow death animation to play
    }
    // Ʈ���ų� Bool ���� �����ϴ� �Լ�
    void ResetIsDead()
    {
        animator.SetBool("IsDead", false);
    }
    void OnDestroy()
    {
        if (currentHp <= 0) gameManager.AddGold(unitData.rewardGold); // Reward gold
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
