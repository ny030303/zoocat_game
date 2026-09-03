using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CSV(Wave / WaveGroup / Reward) 기반 웨이브 진행. authored 웨이브를 다 쓰면
/// <see cref="BalanceConfig"/> 의 endless* 파라미터로 무한 연장한다(HP/수 증가, 시간 감소).
/// PvP 는 이 무한 연장 덕분에 결판이 난다(강한 쪽이 영원히 안 죽는 상황 방지).
/// </summary>
public class WaveManager : MonoBehaviour, IWaveDriver
{
    private Dictionary<int, WaveData> waves;
    private Dictionary<int, WaveGroupData> waveGroups;
    private Dictionary<int, RewardData> rewardData;

    public WaypointManager waypointManager;
    private List<Transform> waypoints;
    private int currentWaveIndex = 1;

    [Tooltip("PvP 씬에서는 false 로 두고 PvpBattleController 가 BeginWaves() 호출")]
    public bool autoStart = true;
    private bool _wavesStarted;
    public int CurrentWave => currentWaveIndex;

    public Transform enemyParentToPlayer;
    public Transform enemyParentToAI;
    public float waveTimeLimit = 60f;
    public Slider waveProgressSlider;
    public TextMeshPro waveText;
    public GameObject nextWaveText;

    private int _authoredMaxId;      // CSV 에 정의된 마지막 웨이브 번호
    private WaveData _lastAuthored;  // 무한 연장의 기준
    private GameManager _gm;

    void Start()
    {
        waves = CSVLoader.LoadWaveData("Scripts/Data/Sheet/Wave");
        waveGroups = CSVLoader.LoadWaveGroupData("Scripts/Data/Sheet/WaveGroup");
        rewardData = RewardLoader.LoadRewardData("Scripts/Data/Sheet/Reward");
        _gm = FindAnyObjectByType<GameManager>();

        ResolveAuthoredTail();

        waypoints = waypointManager.waypoints;
        waveProgressSlider.maxValue = waveTimeLimit;
        waveProgressSlider.value = 0;

        if (autoStart) BeginWaves();
    }

    public void BeginWaves()
    {
        if (_wavesStarted) return;
        _wavesStarted = true;
        StartCoroutine(ManageWaves());
    }

    // ---------------------------------------------------------------- 웨이브 데이터

    private void ResolveAuthoredTail()
    {
        _authoredMaxId = 0;
        foreach (int k in waves.Keys)
            if (k > _authoredMaxId) _authoredMaxId = k;

        if (_authoredMaxId > 0)
        {
            _lastAuthored = waves[_authoredMaxId];
        }
        else
        {
            // CSV 비었음: 최소 폴백 (아무 waveGroup 하나)
            int gid = 1;
            foreach (int k in waveGroups.Keys) { gid = k; break; }
            _lastAuthored = new WaveData(1, 30f, 0, 0, 0, gid);
            Debug.LogWarning("[WaveManager] Wave CSV 비어 있음 - 폴백 웨이브로 무한 진행");
        }
    }

    /// authored 면 CSV 값, 아니면 마지막 authored 를 스케일해 합성.
    private WaveData GetWave(int index)
    {
        if (waves.TryGetValue(index, out var w)) return w;

        var b = BalanceConfig.Current;
        int extra = index - _authoredMaxId; // >= 1
        float timeLimit = Mathf.Max(
            b.endlessMinTimeLimit,
            _lastAuthored.timeLimit - extra * b.endlessTimeReducePerWave);
        int hpAdd = _lastAuthored.hpAdditional + Mathf.RoundToInt(extra * b.endlessHpAddPerWave);

        return new WaveData(index, timeLimit, hpAdd, _lastAuthored.spdAdditional,
                            _lastAuthored.rewardId, _lastAuthored.waveGroupId);
    }

    /// 무한 연장 구간의 몬스터 수 배수 (authored 는 1.0).
    private float EndlessCountMul(int index)
    {
        if (index <= _authoredMaxId) return 1f;
        return 1f + (index - _authoredMaxId) * BalanceConfig.Current.endlessCountRampPerWave;
    }

    // ---------------------------------------------------------------- 보상

    public RewardData GetWaveReward(int waveId)
    {
        var wave = GetWave(waveId);
        return (wave != null && rewardData.TryGetValue(wave.rewardId, out var r)) ? r : null;
    }

    public void GrantWaveReward(int waveId)
    {
        RewardData reward = GetWaveReward(waveId);
        if (reward == null || reward.entries.Count == 0) return; // rewardId 0 등 - 보상 없음은 정상
        foreach (var e in reward.entries) ApplyReward(e);
    }

    private void ApplyReward(RewardEntry e)
    {
        switch (e.itemId)
        {
            case MaterialItemLoader.GoldItemId: // 무료 재화 = 인게임 소환 화폐
                if (_gm != null) _gm.AddGold(e.count);
                Debug.Log($"[Wave] +{e.count} gold");
                break;

            default:
                // 유료 재화 / 플레이 포인트 등 영속 메타 보상 - 별도 시스템 + 서버 동기화 필요 (백로그)
                Debug.Log($"[Wave] item {e.itemId} x{e.count} - 메타 보상 시스템 연결 대기");
                break;
        }
    }

    // ---------------------------------------------------------------- 진행

    IEnumerator ManageWaves()
    {
        while (true)
        {
            nextWaveText.SetActive(false);
            yield return StartCoroutine(SpawnWave());
            nextWaveText.SetActive(true);

            currentWaveIndex++; // 무한 - 리셋하지 않음
            yield return new WaitForSeconds(BalanceConfig.Current.betweenWaveDelay);
        }
    }

    IEnumerator SpawnWave()
    {
        WaveData currentWave = GetWave(currentWaveIndex);
        if (!waveGroups.TryGetValue(currentWave.waveGroupId, out WaveGroupData waveGroup))
        {
            Debug.LogError($"WaveGroup ID {currentWave.waveGroupId} 없음!");
            yield break;
        }

        float countMul = EndlessCountMul(currentWaveIndex);
        float waveStartTime = Time.time;
        waveProgressSlider.value = 0;
        waveProgressSlider.maxValue = currentWave.timeLimit;
        waveText.text = $"0 / {currentWave.timeLimit:0} sec";

        while (Time.time - waveStartTime < currentWave.timeLimit)
        {
            for (int i = 0; i < waveGroup.monsterIds.Count; i++)
            {
                UnitData sponEnemyData = EnemyListLoader.Instance.GetEnemyPrefabToId(waveGroup.monsterIds[i]);
                if (sponEnemyData == null)
                {
                    Debug.LogError($"[WaveManager] 몬스터 id '{waveGroup.monsterIds[i]}' 를 찾을 수 없음");
                    continue;
                }

                int baseCount = i < waveGroup.monsterCounts.Count ? waveGroup.monsterCounts[i] : 0;
                int spawnCount = Mathf.CeilToInt(baseCount * countMul);
                for (int j = 0; j < spawnCount; j++)
                {
                    GameObject enemyObj = Instantiate(sponEnemyData.unitPrefab, waypoints[0].position, Quaternion.identity, enemyParentToPlayer);
                    Enemy enemy = enemyObj.GetComponent<Enemy>();

                    GameObject AIEnemyObj = Instantiate(sponEnemyData.unitPrefab, waypointManager.AIWaypoints[0].position, Quaternion.identity, enemyParentToAI);
                    var sr = AIEnemyObj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.flipX = !sr.flipX;
                    Enemy AIenemy = AIEnemyObj.GetComponent<Enemy>();

                    if (enemy == null || AIenemy == null)
                    {
                        Debug.LogError("생성된 오브젝트에 Enemy 컴포넌트가 없습니다!");
                        Destroy(enemyObj);
                        Destroy(AIEnemyObj);
                        yield break;
                    }

                    waveProgressSlider.value = Time.time - waveStartTime;
                    waveText.text = $"{Mathf.Floor(waveProgressSlider.value):0} / {currentWave.timeLimit:0} sec";

                    float statMultiplier = currentWave.hpAdditional;
                    enemy.Initialize("player", waypoints, sponEnemyData, statMultiplier);
                    AIenemy.Initialize("ai", waypointManager.AIWaypoints, sponEnemyData, statMultiplier);

                    yield return new WaitForSeconds(BalanceConfig.Current.enemySpawnInterval);

                    if (Time.time - waveStartTime >= currentWave.timeLimit) break;
                }
                if (Time.time - waveStartTime >= currentWave.timeLimit) break;
            }
        }

        Debug.Log($"웨이브 {currentWaveIndex} 완료 ({currentWave.timeLimit:0}s, x{countMul:0.00})");
        GrantWaveReward(currentWaveIndex);
        // 웨이브 간 대기는 ManageWaves 에서 한 번만.
    }
}
