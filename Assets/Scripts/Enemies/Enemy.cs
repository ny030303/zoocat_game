using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public UnitData unitData;
    public int currentHp;
    public GameObject damageTextPrefab; // DamageText 프리팹 레퍼런스

    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private Animator animator;
    private float totalDistance; // 전체 경로의 총 길이
    private float requiredSpeed; // 조정된 이동 속도
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    public void Initialize(List<Transform> waypoints, UnitData data)
    {

        gameManager = FindAnyObjectByType<GameManager>();
        unitData = data;
        currentHp = unitData.hp; // 유닛의 체력을 초기화
        this.waypoints = waypoints;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //시작지점부터 엔드지점까지 1000이라고 과정하고 MoveSPD 수치만큼 1초마다 상승한다고 과정하고 구현
        CalculateTotalDistance(); //모든 웨이포인트 간의 총 거리를 계산
        requiredSpeed = totalDistance / unitData.moveSpeed; // 20초 동안 이동하기 위한 속도

        StartCoroutine(FadeIn()); // 유닛이 생성될 때 FadeIn
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

        ShowDamageText(actualDamage); // 데미지를 받은 후 데미지 텍스트 표시

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
        StartCoroutine(FadeOut()); // 사망할 때 FadeOut
        //Debug.Log(unitData.unitName + " has died.");
        Destroy(gameObject, 3f); // Destroy after 1 second to allow death animation to play
    }

    void OnDestroy()
    {
        gameManager.AddGold(unitData.rewardGold); // Reward gold
    }

    IEnumerator FadeIn()
    {
        float duration = 1f; // 페이드 인 지속 시간
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
        float duration = 1f; // 페이드 아웃 지속 시간
        float currentTime = 0f;
        Color color = spriteRenderer.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (currentTime / duration));
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject); // FadeOut 후 오브젝트 삭제
    }
}
