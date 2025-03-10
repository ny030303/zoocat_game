using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    private Dictionary<int, WaveData> waves;
    private Dictionary<int, WaveGroupData> waveGroups;
    private Dictionary<int, RewardData> rewardData;
    public WaypointManager waypointManager;
    private List<Transform> waypoints;
    private int currentWaveIndex = 1;
    public Transform enemyParentToPlayer;
    public Transform enemyParentToAI;
    public float waveTimeLimit = 60f;
    public Slider waveProgressSlider;
    public TextMeshPro waveText;
    public GameObject nextWaveText;

    void Start()
    {
        waves = CSVLoader.LoadWaveData("Scripts/Data/Sheet/Wave");
        waveGroups = CSVLoader.LoadWaveGroupData("Scripts/Data/Sheet/WaveGroup");
        rewardData = RewardLoader.LoadRewardData("Scripts/Data/Sheet/Reward");

        waypoints = waypointManager.waypoints;

        waveProgressSlider.maxValue = waveTimeLimit;
        waveProgressSlider.value = 0;

        StartCoroutine(ManageWaves());
    }



    public RewardData GetWaveReward(int waveId)
    {
        if (waves.ContainsKey(waveId))
        {
            int rewardId = waves[waveId].rewardId;
            if (rewardData.ContainsKey(rewardId))
            {
                return rewardData[rewardId];
            }
        }
        return null;
    }

    public void GrantWaveReward(int waveId)
    {
        RewardData reward = GetWaveReward(waveId);
        if (reward != null)
        {
            Debug.Log($"[웨이브 {waveId} 클리어] 보상 지급: {reward.type} x {reward.amount}");
            ApplyReward(reward);
        }
        else
        {
            Debug.LogWarning($"[웨이브 {waveId}] 보상 없음.");
        }
    }

    private static void ApplyReward(RewardData reward)
    {
        switch (reward.type)
        {
            case "Gold":
                Debug.Log($"골드 {reward.amount} 지급");
                break;
            case "Gem":
                Debug.Log($"젬 {reward.amount} 지급");
                break;
            case "Item":
                Debug.Log($"아이템 지급 (ID: {reward.id})");
                break;
            default:
                Debug.LogWarning($"알 수 없는 보상 타입: {reward.type}");
                break;
        }
    }

    IEnumerator ManageWaves()
    {
        while (true)
        {
            nextWaveText.SetActive(false);
            yield return StartCoroutine(SpawnWave());
            nextWaveText.SetActive(true);

            currentWaveIndex++;
            if (!waves.ContainsKey(currentWaveIndex))
            {
                currentWaveIndex = waves.Count;
                Debug.Log("마지막 웨이브 반복");
            }

            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator SpawnWave()
    {
        if (!waves.ContainsKey(currentWaveIndex))
        {
            Debug.LogError($"WaveData에서 Key {currentWaveIndex}를 찾을 수 없습니다!");
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];
        if (!waveGroups.TryGetValue(currentWave.waveGroupId, out WaveGroupData waveGroup))
        {
            Debug.LogError($"WaveGroup ID {currentWave.waveGroupId} 없음!");
            yield break;
        }

        float waveStartTime = Time.time;
        waveProgressSlider.value = 0;
        waveProgressSlider.maxValue = currentWave.timeLimit; // 슬라이더 최대값 설정
        waveText.text = $"0 / {currentWave.timeLimit} sec"; // 초기 텍스트 표시
        bool waveRunning = true;

        // 💡 웨이브 시간이 끝날 때까지 실행
        while (Time.time - waveStartTime < currentWave.timeLimit && waveRunning)
        {
            for (int i = 0; i < waveGroup.monsterIds.Count; i++)
            {
                for (int j = 0; j < waveGroup.monsterCounts[i]; j++)
                {
                    // 생성될 몬스터 데이터 준비
                    UnitData sponEnemyData = EnemyListLoader.Instance.GetEnemyPrefabToId(waveGroup.monsterIds[i]);

                    // 몬스터 생성 (플레이어 쪽)
                    GameObject enemyObj = Instantiate(sponEnemyData.unitPrefab, waypoints[0].position, Quaternion.identity, enemyParentToPlayer);
                    Enemy enemy = enemyObj.GetComponent<Enemy>();

                    // 몬스터 생성 (AI 쪽)
                    GameObject AIEnemyObj = Instantiate(sponEnemyData.unitPrefab, waypointManager.AIWaypoints[0].position, Quaternion.identity, enemyParentToAI);
                    AIEnemyObj.GetComponent<SpriteRenderer>().flipX = !AIEnemyObj.GetComponent<SpriteRenderer>().flipX;
                    Enemy AIenemy = AIEnemyObj.GetComponent<Enemy>();

                    if (enemy == null || AIenemy == null)
                    {
                        Debug.LogError("생성된 오브젝트에 Enemy 컴포넌트가 없습니다!");
                        Destroy(enemyObj); 
                        Destroy(AIEnemyObj);
                        yield break;
                    }
                    // 웨이브 진행 시간 업데이트
                    waveProgressSlider.value = Time.time - waveStartTime;
                    waveText.text = $"{Mathf.Floor(waveProgressSlider.value)} / {currentWave.timeLimit} sec";

                    // 웨이브 번호에 따른 몬스터 스펙 증가
                    float statMultiplier = currentWave.hpAdditional;
                    enemy.Initialize("player", waypoints, sponEnemyData, statMultiplier);
                    AIenemy.Initialize("ai", waypointManager.AIWaypoints, sponEnemyData, statMultiplier);
                    Debug.LogWarning($"waveGroup.monsterIds[i]: {waveGroup.monsterIds[i]}, statMultiplier: {statMultiplier}");
                    yield return new WaitForSeconds(1.5f);
                }
            }
        }

        // 💡 웨이브 시간이 끝나면 진행 완료 처리
        Debug.Log($"웨이브 {currentWaveIndex} 완료 (지속 시간: {currentWave.timeLimit}초)");
        GrantWaveReward(currentWaveIndex);
        yield return new WaitForSeconds(3f); // 잠시 대기 후 다음 웨이브 시작
    }
}
