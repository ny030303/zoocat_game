using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour, IWaveDriver
{
    public WaveScriptableObject[] waves;
    public WaypointManager waypointManager;
    private List<Transform> waypoints;
    private int currentWaveIndex = 0;

    [Tooltip("PvP 씬에서는 false 로 두고 PvpBattleController 가 BeginWaves() 호출")]
    public bool autoStart = true;
    private bool _wavesStarted;
    public int CurrentWave => currentWaveIndex;
    public Transform enemyParentToPlayer;
    public Transform enemyParentToAI;
    public float waveTimeLimit = 60f; // ���̺꺰 �ð� ���� (�� ����)
    public Slider waveProgressSlider; // Slider ���� ����
    public TextMeshPro waveText; // Slider ���� ����
    public GameObject nextWaveText; // next wave Text

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

        if (waveProgressSlider == null)
        {
            Debug.LogError("WaveProgressSlider�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        waveProgressSlider.maxValue = waveTimeLimit; // Slider�� �ִ밪 ����
        waveProgressSlider.value = 0; // Slider �ʱ�ȭ

        if (autoStart) BeginWaves();
    }

    public void BeginWaves()
    {
        if (_wavesStarted) return;
        _wavesStarted = true;
        StartCoroutine(ManageWaves());
    }

    IEnumerator ManageWaves()
    {
        while (true) // ���� ����, ������ ���̺갡 �ݺ��ǵ��� ����
        {
            nextWaveText.SetActive(false);
            yield return StartCoroutine(SpawnWave());
            nextWaveText.SetActive(true);
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
        float waveStartTime = Time.time; // ���̺� ���� �ð�
        bool timeLimitReached = false; // �ð� ���� ���� ����

        waveProgressSlider.value = 0; // Slider �ʱ�ȭ

        while (!timeLimitReached)
        {
            float waveProgressTime = Time.time - waveStartTime; // ���� ���̺� ��� �ð� ���

            if (waveProgressTime >= waveTimeLimit)
            {
                Debug.Log("���̺� �ð� ���ѿ� �����߽��ϴ�! ���� ���̺�� �����մϴ�.");
                timeLimitReached = true;
                break;
            }

            for (int i = 0; i < currentWave.enemies.Length; i++)
            {
                for (int j = 0; j < currentWave.enemyCounts[i]; j++)
                {
                    if (timeLimitReached)
                    {
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

                    waveProgressTime = Time.time - waveStartTime; // ���̺� ��� �ð� ������Ʈ
                    waveProgressSlider.value = waveProgressTime; // Slider �� ������Ʈ
                    waveText.text = waveProgressTime.ToString();
                }
            }
        }
    }
}
