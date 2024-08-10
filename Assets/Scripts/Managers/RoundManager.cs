using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public WaypointManager waypointManager; // WaypointManager ����
    public GameObject monsterPrefab; // ���� ������
    public int currentRound = 1; // ���� ����
    public float spawnInterval = 3;
    private float nextSpawnTime;
    void Start()
    {
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        SpawnMonster();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnMonster();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnMonster()
    {
        List<Transform> waypoints = waypointManager.waypoints;
        // Check if waypoints list is not empty
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("Waypoints list is empty! Cannot spawn monsters.");
            return;
        }

        GameObject monsterObj = Instantiate(monsterPrefab, waypoints[0].position, Quaternion.identity);
        Monster monster = monsterObj.GetComponent<Monster>();
        monster.Initialize(waypoints, currentRound);
    }
}
