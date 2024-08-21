using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public WaveScriptableObject[] waves;
    public WaypointManager waypointManager;
    private List<Transform> waypoints;
    private int currentWaveIndex = 0;
    public Transform enemyParentToPlayer;
    public Transform enemyParentToAI;
    public float waveTimeLimit = 60f; // 웨이브별 시간 제한 (초 단위)

    void Start()
    {
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager가 할당되지 않았습니다!");
            return;
        }

        waypoints = waypointManager.waypoints;

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("Waypoints 리스트가 비어 있습니다! 몬스터를 생성할 수 없습니다.");
            return;
        }

        StartCoroutine(ManageWaves());
    }

    IEnumerator ManageWaves()
    {
        while (true) // 무한 루프, 마지막 웨이브가 반복되도록 설정
        {
            yield return StartCoroutine(SpawnWave());

            currentWaveIndex++;
            if (currentWaveIndex >= waves.Length)
            {
                currentWaveIndex = waves.Length - 1; // 마지막 웨이브를 반복하도록 인덱스 고정
                Debug.Log("마지막 웨이브를 반복합니다! 몬스터의 스펙이 계속 증가합니다.");
            }

            yield return new WaitForSeconds(5f); // 웨이브 간 대기 시간
        }
    }

    IEnumerator SpawnWave()
    {
        WaveScriptableObject currentWave = waves[currentWaveIndex];
        float waveProgressTime = 0f; // 현재 웨이브에서 경과된 시간 추적
        bool timeLimitReached = false; // 시간 제한 도달 여부

        while (!timeLimitReached)
        {
            for (int i = 0; i < currentWave.enemies.Length; i++)
            {
                for (int j = 0; j < currentWave.enemyCounts[i]; j++)
                {
                    if (waveProgressTime >= waveTimeLimit)
                    {
                        Debug.Log("웨이브 시간 제한에 도달했습니다! 다음 웨이브로 진행합니다.");
                        timeLimitReached = true;
                        break;
                    }

                    // 부모 오브젝트와 함께 Instantiate
                    GameObject enemyObj = Instantiate(currentWave.enemies[i].unitPrefab, waypoints[0].position, Quaternion.identity, enemyParentToPlayer);
                    Enemy enemy = enemyObj.GetComponent<Enemy>();

                    GameObject AIEnemyObj = Instantiate(currentWave.enemies[i].unitPrefab, waypointManager.AIWaypoints[0].position, Quaternion.identity, enemyParentToAI);
                    AIEnemyObj.GetComponent<SpriteRenderer>().flipX = !AIEnemyObj.GetComponent<SpriteRenderer>().flipX;
                    Enemy AIenemy = AIEnemyObj.GetComponent<Enemy>();
                    if (enemy == null || AIenemy == null)
                    {
                        Debug.LogError("생성된 오브젝트에 Enemy 컴포넌트가 없습니다!");
                        Destroy(enemyObj); // 추가 문제 방지를 위해 오브젝트 파괴
                        Destroy(AIEnemyObj); // 추가 문제 방지를 위해 오브젝트 파괴
                        yield break;
                    }

                    // 웨이브 번호에 따른 몬스터 스펙 증가
                    float statMultiplier = 1f + (currentWaveIndex * 0.1f);
                    enemy.Initialize("player", waypoints, currentWave.enemies[i], statMultiplier);
                    AIenemy.Initialize("ai", waypointManager.AIWaypoints, currentWave.enemies[i], statMultiplier);
                    yield return new WaitForSeconds(1.5f); // 에너미 사이의 스폰 간격

                    waveProgressTime += 3f; // 웨이브 경과 시간 업데이트
                }

                if (timeLimitReached)
                {
                    break;
                }
            }
        }
    }
}
