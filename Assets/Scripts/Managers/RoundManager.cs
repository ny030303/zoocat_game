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
    public float waveTimeLimit = 60f; // ���̺꺰 �ð� ���� (�� ����)

    void Start()
    {
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        waypoints = waypointManager.waypoints;

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("Waypoints ����Ʈ�� ��� �ֽ��ϴ�! ���͸� ������ �� �����ϴ�.");
            return;
        }

        StartCoroutine(ManageWaves());
    }

    IEnumerator ManageWaves()
    {
        while (true) // ���� ����, ������ ���̺갡 �ݺ��ǵ��� ����
        {
            yield return StartCoroutine(SpawnWave());

            currentWaveIndex++;
            if (currentWaveIndex >= waves.Length)
            {
                currentWaveIndex = waves.Length - 1; // ������ ���̺긦 �ݺ��ϵ��� �ε��� ����
                Debug.Log("������ ���̺긦 �ݺ��մϴ�! ������ ������ ��� �����մϴ�.");
            }

            yield return new WaitForSeconds(5f); // ���̺� �� ��� �ð�
        }
    }

    IEnumerator SpawnWave()
    {
        WaveScriptableObject currentWave = waves[currentWaveIndex];
        float waveProgressTime = 0f; // ���� ���̺꿡�� ����� �ð� ����
        bool timeLimitReached = false; // �ð� ���� ���� ����

        while (!timeLimitReached)
        {
            for (int i = 0; i < currentWave.enemies.Length; i++)
            {
                for (int j = 0; j < currentWave.enemyCounts[i]; j++)
                {
                    if (waveProgressTime >= waveTimeLimit)
                    {
                        Debug.Log("���̺� �ð� ���ѿ� �����߽��ϴ�! ���� ���̺�� �����մϴ�.");
                        timeLimitReached = true;
                        break;
                    }

                    // �θ� ������Ʈ�� �Բ� Instantiate
                    GameObject enemyObj = Instantiate(currentWave.enemies[i].unitPrefab, waypoints[0].position, Quaternion.identity, enemyParentToPlayer);
                    Enemy enemy = enemyObj.GetComponent<Enemy>();

                    GameObject AIEnemyObj = Instantiate(currentWave.enemies[i].unitPrefab, waypointManager.AIWaypoints[0].position, Quaternion.identity, enemyParentToAI);
                    AIEnemyObj.GetComponent<SpriteRenderer>().flipX = !AIEnemyObj.GetComponent<SpriteRenderer>().flipX;
                    Enemy AIenemy = AIEnemyObj.GetComponent<Enemy>();
                    if (enemy == null || AIenemy == null)
                    {
                        Debug.LogError("������ ������Ʈ�� Enemy ������Ʈ�� �����ϴ�!");
                        Destroy(enemyObj); // �߰� ���� ������ ���� ������Ʈ �ı�
                        Destroy(AIEnemyObj); // �߰� ���� ������ ���� ������Ʈ �ı�
                        yield break;
                    }

                    // ���̺� ��ȣ�� ���� ���� ���� ����
                    float statMultiplier = 1f + (currentWaveIndex * 0.1f);
                    enemy.Initialize("player", waypoints, currentWave.enemies[i], statMultiplier);
                    AIenemy.Initialize("ai", waypointManager.AIWaypoints, currentWave.enemies[i], statMultiplier);
                    yield return new WaitForSeconds(1.5f); // ���ʹ� ������ ���� ����

                    waveProgressTime += 3f; // ���̺� ��� �ð� ������Ʈ
                }

                if (timeLimitReached)
                {
                    break;
                }
            }
        }
    }
}
