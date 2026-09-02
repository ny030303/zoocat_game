using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("UI (선택 - 인스펙터에서 연결)")]
    [SerializeField] private Button matchButton;        // 눌러서 큐 진입/취소 토글
    [SerializeField] private TMP_Text statusText;       // "매칭 상대를 찾는 중..." 등
    [SerializeField] private TMP_Text matchButtonLabel; // 버튼 라벨 (선택)

    [Header("이벤트")]
    public UnityEvent onQueued;
    public UnityEvent onIdle;
    public UnityEvent onMatched;      // 상대 정보는 MatchSession.Opponent 에서 읽는다
    public UnityEvent onMatchClosed;  // 상대 이탈 / 매치 종료

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
    private void Update()
    {
        // 소켓 인증 상태가 바뀌면 버튼/상태텍스트 갱신 (이벤트가 없어서 폴링)
        bool authed = SocketBinder.Instance != null
            && SocketBinder.Instance.State == SocketBinder.ConnState.Authenticated;
        if (authed != _lastAuthed)
        {
            _lastAuthed = authed;
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
                SocketSender.Send(SocketEvents.Enqueue);
                break;
            case State.Queued:
                SocketSender.Send(SocketEvents.Dequeue);
                break;
            case State.Matched:
                // 이미 매칭됨 — 대전 진입은 Phase C
                break;
        }
    }

    private void OnQueued(JsonData _)
    {
        CurrentState = State.Queued;
        Refresh();
        onQueued?.Invoke();
    }

    private void OnQueueLeft(JsonData _)
    {
        if (CurrentState == State.Matched) return; // 매칭 성사 뒤 도착한 queueLeft 무시
        CurrentState = State.Idle;
        Refresh();
        onIdle?.Invoke();
    }

    private void OnMatchFound(JsonData data)
    {
        MatchFoundData d;
        try { d = JsonMapper.ToObject<MatchFoundData>(data.ToJson()); }
        catch (System.Exception ex) { Debug.LogError("[Matchmaking] matchFound parse fail: " + ex); return; }

        MatchSession.Begin(d);
        CurrentState = State.Matched;
        Debug.Log($"[Matchmaking] matched! matchId={d.matchId} opponent={d.opponent?.username} Lv{d.opponent?.level}");
        Refresh();
        onMatched?.Invoke();
        // Phase C: matchMessage {t:"seed"} 교환 + 대전 씬 로드
    }

    private void OnMatchClosedEvent(JsonData _)
    {
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
                CurrentState = State.Queued; Refresh(); break;
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

        if (statusText != null)
        {
            statusText.text =
                !Available ? "게스트는 매칭 불가"
                : !authed ? "서버 연결 중..."
                : CurrentState == State.Queued ? "매칭 상대를 찾는 중..."
                : CurrentState == State.Matched ? $"매칭 성공! 상대: {MatchSession.Opponent?.username} Lv.{MatchSession.Opponent?.level}"
                : "";
        }

        if (matchButtonLabel != null)
        {
            matchButtonLabel.text =
                CurrentState == State.Queued ? "매칭 취소"
                : CurrentState == State.Matched ? "대전 준비중"
                : "매칭 시작";
        }
    }
}
