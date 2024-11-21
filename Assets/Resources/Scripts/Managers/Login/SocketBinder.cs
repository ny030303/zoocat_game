using LitJson;
using System;
using UnityEngine;
using WebSocketSharp;

public class SocketBinder : MonoBehaviour
{
    public static SocketBinder Instance;
    private WebSocket ws;
    public event Action<string> OnWebSocketMessageReceived;

    [SerializeField] private string serverAddress = "ws://192.168.1.62:3000";

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
        ws = new WebSocket(serverAddress);
        ws.OnMessage += ws_OnMessage;
        ws.OnOpen += ws_OnOpen;
        ws.OnClose += ws_OnClose;
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
    }

    void ws_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log($"WebSocket closed. Reason: {e.Reason}");
        TryReconnect();
    }

    private void TryReconnect()
    {
        if (ws.ReadyState != WebSocketState.Open && ws.ReadyState != WebSocketState.Connecting)
        {
            Debug.Log("Attempting to reconnect...");
            ws.Connect();
        }
    }

    private void OnApplicationQuit()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Close();
        }
        ws = null;
    }
}
