using System.Collections;
using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 로비의 매칭 큐 진입/취소 + queued / queueLeft / matchFound / opponentLeft / matchEnded 수신.
/// 실제 대전(matchMessage, 시드 교환, 대전 씬)은 Phase C.
/// 여기서는 matchFound 까지 처리해 <see cref="MatchSession"/> 을 채우고 onMatched 를 발화한다.
/// 게스트(isGuest == 0)는 매칭 불가.
/// </summary>
public class MatchmakingController : MonoBehaviour
{
    public enum State { Idle, Queued, Matched }
    public State CurrentState { get; private set; } = State.Idle;

    /// 이 세션에서 유저가 직접 "매칭 시작" 을 눌렀는지.
    /// 서버가 이전 세션의 큐 상태를 재통지(재접속/큐 잔존)하는 경우를 걸러낸다.
    private bool _wantQueue;

    [Header("UI (선택 - 인스펙터에서 연결)")]
    [SerializeField] private Button matchButton;        // 눌러서 큐 진입/취소 토글
    [SerializeField] private TMP_Text statusText;       // "매칭 상대를 찾는 중..." 등
    [SerializeField] private TMP_Text matchButtonLabel; // 버튼 라벨 (선택)

    [Header("이벤트")]
    public UnityEvent onQueued;
    public UnityEvent onIdle;
    public UnityEvent onMatched;      // 상대 정보는 MatchSession.Opponent 에서 읽는다
    public UnityEvent onMatchClosed;  // 상대 이탈 / 매치 종료

    [Header("대전 씬 (Phase C)")]
    [SerializeField] private bool loadBattleSceneOnMatch = true;
    [SerializeField] private string battleSceneName = "GameScenePvP";
    [SerializeField] private float battleLoadDelaySec = 1.5f;

    private bool Available =>
        UserManager.Instance != null && UserManager.Instance.isGuest != 0 && SocketBinder.Instance != null;

    private void OnEnable()
    {
        var d = SocketDispatcher.Instance;
        d.On(SocketEvents.Queued, OnQueued);
        d.On(SocketEvents.QueueLeft, OnQueueLeft);
        d.On(SocketEvents.MatchFound, OnMatchFound);
        d.On(SocketEvents.OpponentLeft, OnMatchClosedEvent);
        d.On(SocketEvents.MatchEnded, OnMatchClosedEvent);
        d.On(SocketEvents.Error, OnError);

        if (matchButton != null)
        {
            matchButton.onClick.RemoveListener(OnMatchButtonClicked);
            matchButton.onClick.AddListener(OnMatchButtonClicked);
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (SocketDispatcher.HasInstance)
        {
            var d = SocketDispatcher.Instance;
            d.Off(SocketEvents.Queued, OnQueued);
            d.Off(SocketEvents.QueueLeft, OnQueueLeft);
            d.Off(SocketEvents.MatchFound, OnMatchFound);
            d.Off(SocketEvents.OpponentLeft, OnMatchClosedEvent);
            d.Off(SocketEvents.MatchEnded, OnMatchClosedEvent);
            d.Off(SocketEvents.Error, OnError);
        }
        if (matchButton != null) matchButton.onClick.RemoveListener(OnMatchButtonClicked);
    }

    private bool _lastAuthed;
    private bool _cleanedOnAuth;
    private void Update()
    {
        // 소켓 인증 상태가 바뀌면 버튼/상태텍스트 갱신 (이벤트가 없어서 폴링)
        bool authed = SocketBinder.Instance != null
            && SocketBinder.Instance.State == SocketBinder.ConnState.Authenticated;
        if (authed != _lastAuthed)
        {
            _lastAuthed = authed;

            // 로그인 직후 1회: 유저가 매칭을 요청하지 않았다면 서버 큐 잔존분을 정리.
            // (dequeue 는 큐에 없어도 무해 — 서버가 무시하거나 queueLeft 로 응답)
            if (authed && !_cleanedOnAuth && !_wantQueue && CurrentState == State.Idle)
            {
                _cleanedOnAuth = true;
                SocketSender.Send(SocketEvents.Dequeue);
            }

            Refresh();
        }
    }

    /// 버튼: 상태에 따라 큐 진입 / 취소.
    public void OnMatchButtonClicked()
    {
        if (!Available)
        {
            if (statusText != null) statusText.text = "게스트는 매칭할 수 없습니다.";
            return;
        }

        // 인증(서버 로그인)된 상태에서만 — 보류 큐에 enqueue 가 중복 적재되는 것 방지
        if (SocketBinder.Instance.State != SocketBinder.ConnState.Authenticated)
        {
            if (statusText != null) statusText.text = "서버 연결 중... 잠시 후 다시 시도하세요.";
            return;
        }

        switch (CurrentState)
        {
            case State.Idle:
                _wantQueue = true;
                SocketSender.Send(SocketEvents.Enqueue);
                break;
            case State.Queued:
                _wantQueue = false;
                SocketSender.Send(SocketEvents.Dequeue);
                break;
            case State.Matched:
                // 이미 매칭됨 — 대전 진입은 Phase C
                break;
        }
    }

    private void OnQueued(JsonData _)
    {
        if (!_wantQueue)
        {
            // 유저가 요청하지 않았는데 큐에 들어가 있음 → 이전 세션 잔존분. 정리하고 Idle 유지.
            Debug.Log("[Matchmaking] unsolicited 'queued' — dequeuing stale entry");
            SocketSender.Send(SocketEvents.Dequeue);
            CurrentState = State.Idle;
            Refresh();
            return;
        }
        CurrentState = State.Queued;
        Refresh();
        onQueued?.Invoke();
    }

    private void OnQueueLeft(JsonData _)
    {
        if (CurrentState == State.Matched) return; // 매칭 성사 뒤 도착한 queueLeft 무시
        _wantQueue = false;
        CurrentState = State.Idle;
        Refresh();
        onIdle?.Invoke();
    }

    private void OnMatchFound(JsonData data)
    {
        MatchFoundData d;
        try { d = JsonMapper.ToObject<MatchFoundData>(data.ToJson()); }
        catch (System.Exception ex) { Debug.LogError("[Matchmaking] matchFound parse fail: " + ex); return; }

        if (!_wantQueue && CurrentState != State.Queued)
        {
            // 요청한 적 없는 매치 (이전 세션 큐 잔존분이 매칭됨) → 즉시 이탈
            Debug.LogWarning("[Matchmaking] unsolicited matchFound — leaving " + d.matchId);
            SocketSender.Send(SocketEvents.LeaveMatch, new { matchId = d.matchId });
            return;
        }

        MatchSession.Begin(d);
        CurrentState = State.Matched;
        Debug.Log($"[Matchmaking] matched! matchId={d.matchId} opponent={d.opponent?.username} Lv{d.opponent?.level}");
        Refresh();
        onMatched?.Invoke();

        if (loadBattleSceneOnMatch && isActiveAndEnabled)
            StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {
        if (battleLoadDelaySec > 0f) yield return new WaitForSeconds(battleLoadDelaySec);
        if (!MatchSession.InMatch) yield break; // 그 사이 취소/이탈됨
        SceneManager.LoadScene(battleSceneName);
    }

    private void OnMatchClosedEvent(JsonData _)
    {
        _wantQueue = false;
        MatchSession.Clear();
        CurrentState = State.Idle;
        Refresh();
        onMatchClosed?.Invoke();
    }

    private void OnError(JsonData data)
    {
        string msg = data != null ? data.ToString() : "";
        switch (msg)
        {
            case "이미 매칭 대기 중입니다":
                if (_wantQueue) { CurrentState = State.Queued; Refresh(); }
                else { SocketSender.Send(SocketEvents.Dequeue); CurrentState = State.Idle; Refresh(); }
                break;
            case "이미 매치 중입니다":
                CurrentState = State.Matched; Refresh(); break;
            case "상대 정보를 불러오지 못했습니다":
            case "상대 연결이 끊겼습니다":
                CurrentState = State.Idle; Refresh(); break;
        }
        // 토스트 노출은 GlobalErrorHandler 담당
    }

    private void Refresh()
    {
        bool authed = SocketBinder.Instance != null
            && SocketBinder.Instance.State == SocketBinder.ConnState.Authenticated;

        if (matchButton != null)
            matchButton.interactable = Available && authed && CurrentState != State.Matched;

        // 유저가 요청 안 했는데 Queued 로 남아 있으면(잔존/이상) 대기중으로 표시하지 않음
        bool showQueued = CurrentState == State.Queued && _wantQueue;

        if (statusText != null)
        {
            statusText.text =
                !Available ? "게스트는 매칭 불가"
                : !authed ? "서버 연결 중..."
                : showQueued ? "매칭 상대를 찾는 중..."
                : CurrentState == State.Matched ? $"매칭 성공! 상대: {MatchSession.Opponent?.username} Lv.{MatchSession.Opponent?.level}"
                : "";
        }

        if (matchButtonLabel != null)
        {
            matchButtonLabel.text =
                showQueued ? "매칭 취소"
                : CurrentState == State.Matched ? "대전 준비중"
                : "매칭 시작";
        }
    }
}
