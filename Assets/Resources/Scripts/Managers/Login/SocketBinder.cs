using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using WebSocketSharp;

/// <summary>
/// WebSocket 연결/재연결/raw I/O + 인증 상태(<see cref="ConnState"/>) 관리.
/// 수신 메시지는 파싱하지 않고 <see cref="SocketDispatcher"/> 로 넘긴다.
///
/// auth-session 모델:
///  - 자격증명은 <see cref="CredentialStore"/> 에 있다 (userId/deviceId/deviceSecret/token).
///  - 연결(재연결 포함) 성공 시:
///      token 있음  → resumeSession {token}
///      creds 있음  → login {userId, deviceId, deviceSecret}
///      아무것도 없음 → LoginManager 가 register 를 보낸다
///  - close 4000 = 다른 기기 로그인 → 재연결 안 함, OnKicked.
/// WebSocket 콜백은 BG 스레드 → 플래그만 세우고 <see cref="Update"/> 에서 처리.
/// </summary>
public class SocketBinder : MonoBehaviour
{
    public static SocketBinder Instance { get; private set; }

    public enum ConnState { Disconnected, Connecting, Connected, Authenticating, Authenticated, Kicked }
    public ConnState State { get; private set; } = ConnState.Disconnected;

    /// 서버가 강제 종료(close 4000, 중복 로그인)했을 때. 안내 메시지 전달.
    public event Action<string> OnKicked;
    /// register/login/resume 이 최종 실패해 사용자 조치가 필요할 때 (LoginManager 가 구독).
    public event Action<string> OnAuthFailed;

    private const int KickCloseCode = 4000;
    private const float ReconnectDelaySec = 5f;

    [Tooltip("비워두면 AppConfig.Current.socketUrl 사용")]
    [SerializeField] private string serverAddress = "";

    private WebSocket ws;
    private bool isQuitting;

    // --- BG → 메인 신호 ---
    private volatile bool _openedSignal;
    private volatile bool _closedSignal;
    private volatile int _closeCode;
    private float _reconnectAtRealtime = -1f;

    // --- Authenticated 이전 보류 송신 ---
    private readonly List<KeyValuePair<string, object>> _pending = new List<KeyValuePair<string, object>>();

    // --- 연결 전에 누른 register 보류 ---
    private object _pendingRegister; // new { userName, underage }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        var d = SocketDispatcher.Instance;
        d.On(SocketEvents.Registered, OnRegisteredState);
        d.On(SocketEvents.LoginSuccess, OnLoginSuccessState);
        d.On(SocketEvents.LoginError, OnLoginErrorState);
        d.On(SocketEvents.RegisterError, OnRegisterErrorState);
        d.On(SocketEvents.SessionExpired, OnSessionExpiredState);
        d.On(SocketEvents.LoggedOut, OnLoggedOutState);
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

    /// 최초 1회. LoginManager 가 "게스트로 계속" / "구글 로그인"에서 호출.
    /// 소켓이 아직 안 열렸으면 보류했다가 open 시 전송.
    public void SendRegister(string userName, bool underage)
    {
        var payload = new { userName, underage };
        State = ConnState.Authenticating;
        if (!SocketSender.Send(SocketEvents.Register, payload))
        {
            _pendingRegister = payload;
            Debug.Log("[SocketBinder] register queued until open");
        }
    }

    /// 저장된 자격증명으로 재인증 (연결 직후 자동, "로그인이 필요합니다" 수신 시에도).
    public void RequestReAuth()
    {
        if (!string.IsNullOrEmpty(CredentialStore.Token))
        {
            State = ConnState.Authenticating;
            SocketSender.Send(SocketEvents.ResumeSession, new { token = CredentialStore.Token });
        }
        else if (CredentialStore.HasCredentials)
        {
            SendLogin();
        }
        else
        {
            Debug.Log("[SocketBinder] 자격증명 없음 - register 필요");
            State = ConnState.Connected;
        }
    }

    private void SendLogin()
    {
        State = ConnState.Authenticating;
        SocketSender.Send(SocketEvents.Login, new
        {
            userId = CredentialStore.UserId,
            deviceId = CredentialStore.DeviceId,
            deviceSecret = CredentialStore.DeviceSecret,
        });
    }

    /// 계정 전환용. 세션 폐기 + 로컬 자격증명 삭제.
    public void LogoutAndClear()
    {
        SocketSender.Send(SocketEvents.Logout);
    }

    /// 인증이 필요한 이벤트. Authenticated 면 즉시, 아니면 보류 후 flush.
    public void SendWhenAuthed(string ev, object data = null)
    {
        if (State == ConnState.Authenticated) { SocketSender.Send(ev, data); return; }
        _pending.Add(new KeyValuePair<string, object>(ev, data));
        Debug.Log($"[SocketBinder] queued until auth: {ev}");
    }

    private void FlushPending()
    {
        if (_pending.Count == 0) return;
        foreach (var kv in _pending) SocketSender.Send(kv.Key, kv.Value);
        _pending.Clear();
    }

    // ================= WebSocket 콜백 (BG — 플래그만) =================

    private void Ws_OnMessage(object s, MessageEventArgs e) => SocketDispatcher.Instance.Enqueue(e.Data);
    private void Ws_OnOpen(object s, EventArgs e) => _openedSignal = true;
    private void Ws_OnClose(object s, CloseEventArgs e) { _closeCode = e.Code; _closedSignal = true; }
    private void Ws_OnError(object s, ErrorEventArgs e) => Debug.LogError("[SocketBinder] ws error: " + e.Message);

    // ================= 메인 스레드 =================

    private void Update()
    {
        if (_openedSignal)
        {
            _openedSignal = false;
            State = ConnState.Connected;
            Debug.Log("[SocketBinder] opened");

            if (_pendingRegister != null && !CredentialStore.HasCredentials)
            {
                State = ConnState.Authenticating;
                SocketSender.Send(SocketEvents.Register, _pendingRegister);
                _pendingRegister = null;
            }
            else
            {
                RequestReAuth(); // creds 없으면 아무것도 안 보냄 (LoginManager 가 register)
            }
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

    // ---- 인증 이벤트 상태 처리 (UI 는 LoginManager 가 별도 구독) ----

    private void OnRegisteredState(JsonData data)
    {
        if (data != null && data.Has("userId") && data.Has("deviceSecret"))
        {
            JsonData prof = data.Has("userProfile") ? data["userProfile"] : null;
            string name = prof != null ? prof.GetStr("username") : null;
            string ua = prof != null ? prof.GetStr("underage") : null;
            bool underage = ua == "True" || ua == "true" || ua == "1";

            CredentialStore.SaveRegistration(
                data.GetStr("userId"), data.GetStr("deviceId"),
                data.GetStr("deviceSecret"), data.GetStr("token"),
                name, underage);
        }
        State = ConnState.Authenticated;
        FlushPending();
    }

    private void OnLoginSuccessState(JsonData data)
    {
        // login 응답엔 새 token, resumeSession 응답엔 없음
        string token = data.GetStr("token");
        if (!string.IsNullOrEmpty(token)) CredentialStore.Token = token;
        State = ConnState.Authenticated;
        FlushPending();
    }

    private void OnLoginErrorState(JsonData data)
    {
        // 자격증명 불일치/부재 → 로컬 자격증명 폐기하고 register 유도
        Debug.LogWarning("[SocketBinder] loginError: " + (data != null ? data.ToString() : ""));
        CredentialStore.Clear();
        State = ConnState.Connected;
        OnAuthFailed?.Invoke("자격 증명이 유효하지 않습니다. 다시 시작해 주세요.");
    }

    private void OnRegisterErrorState(JsonData data)
    {
        Debug.LogError("[SocketBinder] registerError: " + (data != null ? data.ToString() : ""));
        State = ConnState.Connected;
        OnAuthFailed?.Invoke("가입에 실패했습니다. 잠시 후 다시 시도하세요.");
    }

    private void OnSessionExpiredState(JsonData _)
    {
        Debug.Log("[SocketBinder] sessionExpired → login 폴백");
        if (CredentialStore.HasCredentials) SendLogin();
        else { State = ConnState.Connected; OnAuthFailed?.Invoke("세션이 만료되었습니다."); }
    }

    private void OnLoggedOutState(JsonData _)
    {
        CredentialStore.Clear();
        _pending.Clear();
        State = ConnState.Connected;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        if (ws != null) { try { ws.CloseAsync(); } catch { } ws = null; }
    }
}
