using LitJson;
using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

public class SocketBinder : MonoBehaviour
{
    public static SocketBinder Instance;
    private WebSocket ws;
    public event Action<string> OnWebSocketMessageReceived;

    [SerializeField] private string serverAddress = "ws://192.168.1.151:3000";
    private bool isQuitting = false; // 🔄 종료 시 재연결 방지 변수 추가
    private bool isReconnecting = false; // 🔄 중복 재연결 방지 변수 추가

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeWebSocket();
    }
    public WebSocket GetWs() { return ws; }

    private void InitializeWebSocket()
    {
        if (ws != null) // 🔄 기존 WebSocket 인스턴스가 존재하면 정리 후 재생성
        {
            ws.Close();
            ws = null;
        }

        ws = new WebSocket(serverAddress);
        ws.OnMessage += ws_OnMessage;
        ws.OnOpen += ws_OnOpen;
        ws.OnClose += ws_OnClose;
        ws.OnError += ws_OnError; // 🔄 WebSocket 에러 핸들러 추가
        ws.Connect();
    }

    public void SendMessage(string message)
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Send(message);
            Debug.Log("Message sent: " + message);
        }
        else
        {
            Debug.LogError("WebSocket is not open. Message not sent.");
        }
    }

    void ws_OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            Debug.Log("Message received: " + e.Data);
            JsonData jsonData = JsonMapper.ToObject(e.Data);

            if (jsonData["event"].ToString().Equals("loginSuccess"))
            {
                JsonData userProfile = jsonData["data"]["userProfile"];
                UserManager.Instance.LoadUserFromJson(userProfile);
            }

            OnWebSocketMessageReceived?.Invoke(e.Data);
        }
        catch (JsonException jsonEx)
        {
            Debug.LogError("JSON Parsing Error: " + jsonEx.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in ws_OnMessage: " + ex.Message);
        }
    }

    void ws_OnOpen(object sender, EventArgs e)
    {
        Debug.Log("WebSocket connection opened.");
        isReconnecting = false; // 🔄 재연결 플래그 리셋
    }

    void ws_OnClose(object sender, CloseEventArgs e)
    {
        if (!isQuitting) // 🔄 앱 종료 시 재연결 방지
        {
            Debug.Log($"WebSocket closed. Reason: {e.Reason}");
            StartCoroutine(TryReconnect()); // 🔄 비동기 재연결 시도
        }
    }

    void ws_OnError(object sender, ErrorEventArgs e) // 🔄 추가: WebSocket 에러 발생 시 처리
    {
        Debug.LogError("WebSocket Error: " + e.Message);
        StartCoroutine(TryReconnect()); // 🔄 에러 발생 시에도 재연결 시도
    }

    private IEnumerator TryReconnect()
    {
        if (isReconnecting) // 🔄 중복 재연결 방지
            yield break;

        isReconnecting = true;

        Debug.Log("Attempting to reconnect...");
        yield return new WaitForSeconds(5f); // 🔄 5초 대기 후 재연결 시도

        if (!isQuitting) // 🔄 앱 종료가 아닐 때만 재연결
        {
            InitializeWebSocket();
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true; // 🔄 앱 종료 시 플래그 설정
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Close();
        }
        ws = null;
    }
}
