using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using WebSocketSharp;

/// <summary>
/// WebSocket 연결/재연결/raw I/O + 인증 상태(<see cref="ConnState"/>) 관리.
/// 수신 메시지는 파싱하지 않고 <see cref="SocketDispatcher"/> 로 넘긴다.
/// 재연결 성공 시 캐시된 자격증명으로 <c>login</c> 을 자동 재전송한다.
/// WebSocket 콜백은 BG 스레드에서 오므로 플래그만 세우고 <see cref="Update"/> 에서 처리한다.
/// </summary>
public class SocketBinder : MonoBehaviour
{
    public static SocketBinder Instance { get; private set; }

    public enum ConnState { Disconnected, Connecting, Connected, Authenticating, Authenticated, Kicked }
    public ConnState State { get; private set; } = ConnState.Disconnected;

    /// 서버가 강제 종료(close 4000, 중복 로그인 등)했을 때. 안내 메시지 전달.
    public event Action<string> OnKicked;

    private const int KickCloseCode = 4000;
    private const float ReconnectDelaySec = 5f;

    [Tooltip("비워두면 AppConfig.Current.socketUrl 사용. 값을 넣으면 수동 오버라이드")]
    [SerializeField] private string serverAddress = "";

    private WebSocket ws;
    private bool isQuitting;

    // --- BG 스레드 → 메인 스레드 신호 ---
    private volatile bool _openedSignal;
    private volatile bool _closedSignal;
    private volatile int _closeCode;
    private float _reconnectAtRealtime = -1f;

    // --- login 자격증명 캐시 (메모리) ---
    private string _cid, _cname, _cunderage;
    private bool HasCachedLogin => !string.IsNullOrEmpty(_cid);

    // --- Authenticated 이전 보류 송신 ---
    private readonly List<KeyValuePair<string, object>> _pending = new List<KeyValuePair<string, object>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 상태 관리용 자체 구독 (UI 는 LoginManager 가 별도 구독)
        SocketDispatcher.Instance.On(SocketEvents.LoginSuccess, OnLoginSuccessState);
        SocketDispatcher.Instance.On(SocketEvents.LoginError, OnLoginErrorState);
        Connect();
    }

    public WebSocket GetWs() => ws;

    private string ResolveServerAddress()
        => string.IsNullOrWhiteSpace(serverAddress) ? AppConfig.Current.socketUrl : serverAddress;

    private void Connect()
    {
        if (ws != null) { try { ws.CloseAsync(); } catch { } ws = null; }

        State = ConnState.Connecting;
        string addr = ResolveServerAddress();
        Debug.Log("[SocketBinder] connecting to " + addr);

        ws = new WebSocket(addr);
        ws.OnMessage += Ws_OnMessage;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;
        ws.OnError += Ws_OnError;
        ws.ConnectAsync();
    }

    // ================= 송신 =================

    /// LoginManager 가 login 을 보낼 때 호출 — 재연결 시 자동 재로그인용.
    public void CacheLoginPayload(string id, string userName, string underage)
    {
        _cid = id; _cname = userName; _cunderage = underage;
    }

    /// 인증이 필요한 이벤트. Authenticated 면 즉시, 아니면 보류 후 인증되면 flush.
    public void SendWhenAuthed(string ev, object data = null)
    {
        if (State == ConnState.Authenticated) { SocketSender.Send(ev, data); return; }
        _pending.Add(new KeyValuePair<string, object>(ev, data));
        Debug.Log($"[SocketBinder] queued until auth: {ev}");
    }

    /// "로그인이 필요합니다" 수신 시(GlobalErrorHandler) 또는 재연결 후 자동 호출.
    public void RequestReLogin()
    {
        if (!HasCachedLogin)
        {
            Debug.LogWarning("[SocketBinder] re-login requested but no cached credentials");
            return;
        }
        State = ConnState.Authenticating;
        SocketSender.Send(SocketEvents.Login, new { id = _cid, userName = _cname, underage = _cunderage });
    }

    private void FlushPending()
    {
        if (_pending.Count == 0) return;
        foreach (var kv in _pending) SocketSender.Send(kv.Key, kv.Value);
        _pending.Clear();
    }

    // ================= WebSocket 콜백 (BG 스레드 — 플래그만) =================

    private void Ws_OnMessage(object s, MessageEventArgs e) => SocketDispatcher.Instance.Enqueue(e.Data);

    private void Ws_OnOpen(object s, EventArgs e) => _openedSignal = true;

    private void Ws_OnClose(object s, CloseEventArgs e)
    {
        _closeCode = e.Code;
        _closedSignal = true;
    }

    private void Ws_OnError(object s, ErrorEventArgs e)
    {
        Debug.LogError("[SocketBinder] ws error: " + e.Message);
        // 재연결은 OnClose 가 처리 (중복 방지)
    }

    // ================= 메인 스레드 처리 =================

    private void Update()
    {
        if (_openedSignal)
        {
            _openedSignal = false;
            State = ConnState.Connected;
            Debug.Log("[SocketBinder] opened");
            if (HasCachedLogin) RequestReLogin();
        }

        if (_closedSignal)
        {
            _closedSignal = false;
            Debug.Log($"[SocketBinder] closed (code {_closeCode})");

            if (_closeCode == KickCloseCode)
            {
                State = ConnState.Kicked;
                _pending.Clear();
                OnKicked?.Invoke("다른 기기에서 로그인되었습니다.");
            }
            else if (!isQuitting)
            {
                State = ConnState.Disconnected;
                _reconnectAtRealtime = Time.realtimeSinceStartup + ReconnectDelaySec;
            }
        }

        if (_reconnectAtRealtime > 0f && Time.realtimeSinceStartup >= _reconnectAtRealtime && !isQuitting)
        {
            _reconnectAtRealtime = -1f;
            Debug.Log("[SocketBinder] reconnecting...");
            Connect();
        }
    }

    private void OnLoginSuccessState(JsonData _)
    {
        State = ConnState.Authenticated;
        FlushPending();
    }

    private void OnLoginErrorState(JsonData _)
    {
        if (State == ConnState.Authenticating) State = ConnState.Connected;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        if (ws != null) { try { ws.CloseAsync(); } catch { } ws = null; }
    }
}
