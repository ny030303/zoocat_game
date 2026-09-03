using System.Collections;
using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Phase C 1:1 대전 오케스트레이터. GameScenePvP 에 빈 GameObject 로 붙인다.
///
/// 흐름:
///  1) Start: MatchSession.InMatch 확인 → pvpMode 켜고 hello 교환
///  2) 양쪽 hello(또는 워치독) → 공유 seed 확정, Random.InitState, 웨이브 시작
///  3) 로컬 GameEventManager 이벤트 → 0.5s 배치로 상대에게 릴레이
///  4) 상대 이벤트 → PvpOpponentView 로 AI 보드에 재현
///  5) 라이프 0 / 상대 이탈 / 타임캡 → PvpJudge 결정적 판정 → 결과 UI → leaveMatch → 로비
///
/// MatchSession.InMatch 가 아니면(씬 단독 테스트) 네트워킹 없이 solo 로 웨이브만 시작.
/// </summary>
public class PvpBattleController : MonoBehaviour
{
    [Header("씬 참조 (비우면 자동 탐색)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameEventManager gameEventManager;
    [SerializeField] private UnitSpawnManager playerSpawn;   // owner == "player"
    [SerializeField] private UnitSpawnManager aiSpawn;       // owner == "ai"
    [SerializeField] private UnitDatabase unitDatabase;
    [Tooltip("WaveManager. 비우면 활성화된 IWaveDriver 자동 탐색")]
    [SerializeField] private MonoBehaviour waveDriverObject;

    [Header("결과 UI (선택)")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button toLobbyButton;
    [SerializeField] private string lobbySceneName = "LobbyTestScene";

    [Header("타이밍")]
    [SerializeField] private float helloTimeoutSec = 8f;   // 상대 hello 없어도 이 시간 후 시작
    [SerializeField] private float autoReturnSec = 4f;     // 결과 UI 없을 때 자동 복귀

    // 승/패/무 보상 재화는 BalanceConfig(Resources/Config/Balance) 에서 읽는다.

    private IWaveDriver _wave;
    private PvpRelay _relay;
    private PvpOpponentView _oppView;

    private int _mySeed;
    private int _oppSeed;
    private bool _iSaidHello, _oppSaidHello, _battleRunning, _resolved;
    private PvpResult _finalResult = PvpResult.Undecided;
    private bool _rewardGranted;

    private float _battleStart;
    private int _lastSentWave = -1;

    // 판정 입력
    private int _meDeadTs = -1, _oppDeadTs = -1;
    private int _myWaveAtEnd = -1, _oppWaveAtEnd = -1;
    private bool _opponentGone;
    private string _oppReportedRes;

    private int NowMs => _battleStart <= 0f ? 0
        : (int)((Time.realtimeSinceStartup - _battleStart) * 1000f);

    // ---------------------------------------------------------------- lifecycle

    private void Start()
    {
        ResolveRefs();

        if (!MatchSession.InMatch)
        {
            Debug.LogWarning("[PvP] MatchSession 없음 → solo 폴백 (네트워킹 비활성)");
            _wave?.BeginWaves();
            return;
        }

        if (gameManager != null) gameManager.pvpMode = true;

        _oppView = new PvpOpponentView(aiSpawn, gameManager, unitDatabase);

        _relay = new PvpRelay(MatchSession.MatchId);
        _relay.OnOpponentHello = OnOpponentHello;
        _relay.OnOpponentEvent = OnOpponentEvent;
        _relay.OnOpponentEnd   = OnOpponentEnd;
        _relay.Start();

        if (gameEventManager != null) gameEventManager.OnActionRecorded += OnLocalAction;
        if (gameManager != null)
        {
            gameManager.OnPlayerLifeChanged += OnLocalLifeChanged;
            gameManager.OnLocalGameOver     += OnLocalDead;
            gameManager.OnLocalWin          += OnLocalWon;
        }
        SocketDispatcher.Instance.On(SocketEvents.OpponentLeft, OnOpponentGone);
        SocketDispatcher.Instance.On(SocketEvents.MatchEnded,   OnOpponentGone);

        if (toLobbyButton != null) toLobbyButton.onClick.AddListener(ReturnToLobby);
        if (resultPanel != null) resultPanel.SetActive(false);

        _mySeed = UnityEngine.Random.Range(1, int.MaxValue);
        _relay.SendHello(_mySeed, _wave != null ? _wave.GetType().Name : "none");
        _iSaidHello = true;

        StartCoroutine(HelloWatchdog());
    }

    private void OnDestroy()
    {
        if (gameEventManager != null) gameEventManager.OnActionRecorded -= OnLocalAction;
        if (gameManager != null)
        {
            gameManager.OnPlayerLifeChanged -= OnLocalLifeChanged;
            gameManager.OnLocalGameOver     -= OnLocalDead;
            gameManager.OnLocalWin          -= OnLocalWon;
        }
        if (SocketDispatcher.HasInstance)
        {
            SocketDispatcher.Instance.Off(SocketEvents.OpponentLeft, OnOpponentGone);
            SocketDispatcher.Instance.Off(SocketEvents.MatchEnded,   OnOpponentGone);
        }
        _relay?.Stop();
    }

    private void Update()
    {
        _relay?.Tick(Time.unscaledDeltaTime);

        if (!_battleRunning) return;

        if (_wave != null && _wave.CurrentWave != _lastSentWave)
        {
            _lastSentWave = _wave.CurrentWave;
            _relay.QueueOut(new PvpEvent { k = PvpProtocol.K_Wave, v = _lastSentWave, ts = NowMs });
        }

        if (!_resolved && NowMs >= PvpProtocol.MatchCapMs)
        {
            // 타임캡: 양쪽 다 "종료"로 간주, 진행 웨이브로 비교
            _meDeadTs = NowMs;
            _myWaveAtEnd = _wave != null ? _wave.CurrentWave : 0;
            ResolveAndSend();
        }
    }

    // ---------------------------------------------------------------- handshake

    private void OnOpponentHello(PvpHello h)
    {
        if (h == null) return;
        _oppSeed = h.seed;
        _oppSaidHello = true;
        TryBeginBattle();
    }

    private IEnumerator HelloWatchdog()
    {
        float t = 0f;
        while (t < helloTimeoutSec && !_oppSaidHello) { t += Time.unscaledDeltaTime; yield return null; }
        if (!_battleRunning)
        {
            if (!_oppSaidHello) Debug.LogWarning("[PvP] 상대 hello 타임아웃 → 내 seed 로 시작");
            TryBeginBattle();
        }
    }

    private void TryBeginBattle()
    {
        if (_battleRunning || !_iSaidHello) return;

        int seed = _oppSaidHello ? Mathf.Min(_mySeed, _oppSeed) : _mySeed;
        MatchSession.SharedSeed = seed;
        UnityEngine.Random.InitState(seed);

        _battleStart = Time.realtimeSinceStartup;
        _battleRunning = true;
        _wave?.BeginWaves();
        Debug.Log($"[PvP] battle start seed={seed} match={MatchSession.MatchId}");
    }

    // ---------------------------------------------------------------- local → wire

    private void OnLocalAction(PlayerAction a)
    {
        if (_relay == null || a == null) return;
        PvpEvent e = null;

        switch (a.actionType)
        {
            case "UnitSpawn":
                if (a.actionData is UnitSpawnEvent s)
                    e = new PvpEvent
                    {
                        k = PvpProtocol.K_Spawn,
                        u = s.unitID,
                        c = CellOf(playerSpawn, s.position),
                        ts = NowMs
                    };
                break;

            case "UnitMerge":
                if (a.actionData is UnitMergeEvent m)
                    e = new PvpEvent
                    {
                        k = PvpProtocol.K_Merge,
                        a = m.unitID1,                              // 드래그되어 사라진 유닛
                        n = CellOf(playerSpawn, m.startPosition),   // 그 원래 셀
                        u = m.unitID2,                              // 타겟 셀에 있던 유닛
                        c = CellOf(playerSpawn, m.endPosition),     // 결과가 놓이는 셀
                        r = m.resultUnitID,
                        ts = NowMs
                    };
                break;

            case "UnitLevelUpgrade":
                if (a.actionData is UnitLevelUpgradeEvent up)
                    e = new PvpEvent
                    {
                        k = PvpProtocol.K_Upgrade,
                        u = up.unitID,
                        n = up.unitNumber,
                        ts = NowMs
                    };
                break;
        }

        if (e != null) _relay.QueueOut(e);
    }

    private void OnLocalLifeChanged(int life)
    {
        _relay?.QueueOut(new PvpEvent { k = PvpProtocol.K_Life, v = life, ts = NowMs });
    }

    private static int CellOf(UnitSpawnManager sm, Vector3 pos)
        => sm != null ? sm.PositionToCell(new Vector2(pos.x, pos.y)) : -1;

    // ---------------------------------------------------------------- wire → local

    private void OnOpponentEvent(PvpEvent e)
    {
        _oppView?.Apply(e);
        if (e != null && e.k == PvpProtocol.K_Wave) _oppWaveAtEnd = Mathf.Max(_oppWaveAtEnd, e.v);
    }

    private void OnOpponentEnd(PvpEnd end)
    {
        if (end == null) return;
        _oppDeadTs = end.ts;
        _oppWaveAtEnd = end.wave;
        _oppReportedRes = end.res;
        ResolveResult();
    }

    private void OnOpponentGone(JsonData _)
    {
        _opponentGone = true;
        ResolveResult();
    }

    // ---------------------------------------------------------------- end / result

    private void OnLocalDead()
    {
        _meDeadTs = NowMs;
        _myWaveAtEnd = _wave != null ? _wave.CurrentWave : 0;
        ResolveAndSend();
    }

    private void OnLocalWon()
    {
        // 상대 라이프 0 을 로컬에서 확인 (relay life 이벤트 경유)
        if (_myWaveAtEnd < 0) _myWaveAtEnd = _wave != null ? _wave.CurrentWave : 0;
        if (_oppDeadTs < 0) _oppDeadTs = NowMs;
        ResolveAndSend();
    }

    private void ResolveAndSend()
    {
        if (_resolved) return;
        PvpResult r = PvpJudge.Decide(_meDeadTs, _myWaveAtEnd, _oppDeadTs, _oppWaveAtEnd, _opponentGone);
        _relay?.SendEnd(new PvpEnd
        {
            res = PvpJudge.ToWire(r),
            wave = Mathf.Max(0, _myWaveAtEnd),
            ts = _meDeadTs
        });
        ResolveResult();
    }

    private void ResolveResult()
    {
        if (_resolved) return;

        bool meEnded = _meDeadTs >= 0;
        bool oppEnded = _oppDeadTs >= 0 || _opponentGone || !string.IsNullOrEmpty(_oppReportedRes);
        if (!_opponentGone && !(meEnded && oppEnded)) return; // 아직 양쪽 결과가 안 모임

        _resolved = true;
        PvpResult r = PvpJudge.Decide(_meDeadTs, _myWaveAtEnd, _oppDeadTs, _oppWaveAtEnd, _opponentGone);

        if (!string.IsNullOrEmpty(_oppReportedRes) && !_opponentGone)
        {
            bool complementary =
                (r == PvpResult.Win && _oppReportedRes == "lose") ||
                (r == PvpResult.Lose && _oppReportedRes == "win") ||
                (r == PvpResult.Draw && _oppReportedRes == "draw");
            if (!complementary)
                Debug.LogWarning($"[PvP] 결과 불일치 me={r} opp={_oppReportedRes} → 로컬 판정 유지");
        }

        Debug.Log($"[PvP] result={r} (meDead={_meDeadTs} w{_myWaveAtEnd} / oppDead={_oppDeadTs} w{_oppWaveAtEnd} / gone={_opponentGone})");
        ShowResult(r);
    }

    private void ShowResult(PvpResult r)
    {
        Time.timeScale = 0f;
        _finalResult = r;

        RewardFor(r, out int gold, out int gems);
        string label = (r == PvpResult.Win ? "승리!" : r == PvpResult.Lose ? "패배" : "무승부")
                       + $"  +{gold} 골드"
                       + (gems > 0 ? $"  +{gems} 젬" : "");

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultText != null) resultText.text = label;
        }
        else
        {
            Debug.Log("[PvP] " + label + " (결과 패널 미연결 → 자동 복귀)");
            StartCoroutine(AutoReturn());
        }
        // TODO(server backlog): matchResult 이벤트 생기면 여기서 전적/보상을 서버에도 반영
    }

    private void RewardFor(PvpResult r, out int gold, out int gems)
    {
        var b = BalanceConfig.Current;
        switch (r)
        {
            case PvpResult.Win:  gold = b.pvpWinGold;  gems = b.pvpWinGems;  break;
            case PvpResult.Lose: gold = b.pvpLoseGold; gems = b.pvpLoseGems; break;
            default:             gold = b.pvpDrawGold; gems = b.pvpDrawGems; break; // Draw / Undecided
        }
    }

    /// UserManager.currentUser 재화에 보상 적용 + 로컬 저장 + 로비에서 표시할 MatchSession.PendingReward 세팅.
    private void GrantReward()
    {
        if (_rewardGranted || _finalResult == PvpResult.Undecided) return;
        _rewardGranted = true;

        RewardFor(_finalResult, out int gold, out int gems);

        var um = UserManager.Instance;
        if (um != null && um.currentUser != null)
        {
            um.currentUser.gold += gold;
            um.currentUser.gems += gems;
            FileManager.SaveUserData(um.currentUser); // 로컬 영속(게스트). 서버 동기화는 백로그.
        }
        MatchSession.PendingReward = new MatchReward { result = _finalResult, gold = gold, gems = gems };
        Debug.Log($"[PvP] reward granted: {_finalResult} +{gold}G +{gems} gem");
    }

    private IEnumerator AutoReturn()
    {
        float t = 0f;
        while (t < autoReturnSec) { t += Time.unscaledDeltaTime; yield return null; }
        ReturnToLobby();
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;
        GrantReward(); // 씬 이동 전에 재화 적용 → 로비 헤더가 갱신된 값을 읽음
        SocketSender.Send(SocketEvents.LeaveMatch, new { matchId = MatchSession.MatchId });
        MatchSession.Clear();
        if (gameManager != null) gameManager.pvpMode = false;
        SceneManager.LoadScene(lobbySceneName);
    }

    // ---------------------------------------------------------------- refs

    private void ResolveRefs()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (gameEventManager == null) gameEventManager = FindObjectOfType<GameEventManager>();
        if (unitDatabase == null && gameManager != null) unitDatabase = gameManager.unitDatabase;

        if (playerSpawn == null || aiSpawn == null)
        {
            foreach (var sm in FindObjectsOfType<UnitSpawnManager>())
            {
                if (sm.owner == "player" && playerSpawn == null) playerSpawn = sm;
                else if (sm.owner == "ai" && aiSpawn == null) aiSpawn = sm;
            }
        }

        // 인스펙터 ref 가 비활성 컴포넌트를 가리키면 무시하고 재탐색
        _wave = null;
        if (waveDriverObject != null && waveDriverObject.isActiveAndEnabled && waveDriverObject is IWaveDriver wd0)
            _wave = wd0;
        if (_wave == null)
        {
            foreach (var mb in FindObjectsOfType<MonoBehaviour>())
                if (mb is IWaveDriver wd && mb.isActiveAndEnabled) { _wave = wd; break; }
        }
        if (_wave == null)
            Debug.LogError("[PvP] 활성화된 WaveManager 를 찾지 못했습니다 - 웨이브가 시작되지 않습니다");
        else
            Debug.Log("[PvP] wave driver = " + ((MonoBehaviour)_wave).GetType().Name);
    }
}
