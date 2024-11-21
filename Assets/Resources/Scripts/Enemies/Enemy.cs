using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public UnitData unitData;
    public bool isDie;
    public int currentHp;
    public GameObject damageTextPrefab; // DamageText 프리팹 레퍼런스

    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private Animator animator;
    private float totalDistance;
    private float requiredSpeed; // 조정된 이동 속도
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

        // 모든 웨이포인트 간의 총 거리를 계산
        CalculateTotalDistance();

        // requiredSpeed를 유닛이 목표 시간에 도착하도록 계산
        requiredSpeed = totalDistance / (1000f / unitData.moveSpeed); // 1000을 이동하려면 moveSpeed에 따른 시간에 도달하도록 설정
        startTime = Time.time;  // 유닛이 이동을 시작한 시간 기록
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
            //Debug.Log($"유닛이 도착했습니다. 총 경과 시간: {totalTime}초");
            Destroy(gameObject); // 웨이포인트가 없거나 마지막 웨이포인트에 도달한 경우 객체 파괴
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        float step = requiredSpeed * Time.deltaTime; // 여기에 계산된 requiredSpeed를 사용합니다.

        // Animator의 속도 파라미터 업데이트
        animator.SetFloat("Speed", requiredSpeed);

        if (direction.magnitude <= step)
        {
            transform.position = target.position;
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                //float endTime = Time.time;
                //float totalTime = endTime - startTime;
                //Debug.Log($"유닛이 도착했습니다. 총 경과 시간: {totalTime}초");
                gameManager.TakeDamage(owner); // 플레이어의 체력을 감소시킵니다.
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

        ShowDamageText(actualDamage); // 데미지를 받은 후 데미지 텍스트 표시

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
        StartCoroutine(FadeOut()); // 사망할 때 FadeOut
        //Debug.Log(unitData.unitName + " has died.");
        Destroy(gameObject, 3f); // Destroy after 1 second to allow death animation to play
    }
    // 트리거나 Bool 값을 리셋하는 함수
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
