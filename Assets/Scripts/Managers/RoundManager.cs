using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public WaveScriptableObject[] waves;
    //public Transform spawnPoint;
    public WaypointManager waypointManager; // WaypointManager ����
    List<Transform> waypoints;
    private int currentWaveIndex = 0;
    public Transform enemyParent;

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
            Debug.LogError("Waypoints list is empty! Cannot spawn monsters.");
            return;
        }

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        WaveScriptableObject currentWave = waves[currentWaveIndex];

        for (int i = 0; i < currentWave.enemies.Length; i++)
        {
            for (int j = 0; j < currentWave.enemyCounts[i]; j++)
            {
                // Instantiate with parent
                GameObject enemyObj = Instantiate(currentWave.enemies[i].unitPrefab, waypoints[0].position, Quaternion.identity, enemyParent);
                Enemy enemy = enemyObj.GetComponent<Enemy>();

                if (enemy == null)
                {
                    Debug.LogError("The instantiated object does not have a Monster component!");
                    Destroy(enemyObj); // Optionally destroy the object to prevent further issues
                    yield break;
                }

                enemy.Initialize(waypoints, currentWave.enemies[i]);
                yield return new WaitForSeconds(3f); // ���ʹ� ������ ���� ����
            }
        }

        // ���尡 ������ ���� ���带 ��ٸ��ų� ���� ����� �Ѿ�� ����
        yield return new WaitForSeconds(5f); // ���� �� ��� �ð�
        currentWaveIndex++;
        if (currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWave());
        }
    }

    //public WaypointManager waypointManager; // WaypointManager ����
    //public GameObject monsterPrefab; // ���� ������
    //public int currentRound = 1; // ���� ����
    //public float spawnInterval = 3;
    //private float nextSpawnTime;

    //void Start()
    //{
    //    if (waypointManager == null)
    //    {
    //        Debug.LogError("WaypointManager�� �Ҵ���� �ʾҽ��ϴ�!");
    //        return;
    //    }

    //    if (monsterPrefab == null)
    //    {
    //        Debug.LogError("Monster Prefab is not assigned!");
    //        return;
    //    }

    //    SpawnMonster();
    //}

    //void Update()
    //{
    //    if (Time.time >= nextSpawnTime)
    //    {
    //        SpawnMonster();
    //        nextSpawnTime = Time.time + spawnInterval;
    //    }
    //}

    //void SpawnMonster()
    //{
    //    List<Transform> waypoints = waypointManager.waypoints;
    //    // Check if waypoints list is not empty
    //    if (waypoints == null || waypoints.Count == 0)
    //    {
    //        Debug.LogError("Waypoints list is empty! Cannot spawn monsters.");
    //        return;
    //    }

    //    GameObject monsterObj = Instantiate(monsterPrefab, waypoints[0].position, Quaternion.identity);
    //    Enemy enemy = monsterObj.GetComponent<Enemy>();

    //    if (enemy == null)
    //    {
    //        Debug.LogError("The instantiated object does not have a Monster component!");
    //        Destroy(monsterObj); // Optionally destroy the object to prevent further issues
    //        return;
    //    }

    //    //enemy.Initialize(waypoints, currentRound);
    //}
}
