using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

using WebSocketSharp;//�� ���� ���̺귯���� ����Ѵ�
using WebSocket = WebSocketSharp.WebSocket;

public class SocketBinder : MonoBehaviour
{

    public static SocketBinder Instance;

    private WebSocket ws;//���� ����

    public event Action<string> OnWebSocketMessageReceived;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure this object persists across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }

    public WebSocket GetWs() { return ws; }
    void Start()
    {
        ws = new WebSocket("ws://192.168.1.151:3000");// 127.0.0.1�� ������ ������ �ּ��̴�. 3333��Ʈ�� �����Ѵٴ� �ǹ��̴�.
        ws.OnMessage += ws_OnMessage; //�������� ����Ƽ ������ �޼����� �� ��� ������ �Լ��� ����Ѵ�.
        ws.OnOpen += ws_OnOpen;//������ ����� ��� ������ �Լ��� ����Ѵ�
        ws.OnClose += ws_OnClose;//������ ���� ��� ������ �Լ��� ����Ѵ�.
        ws.Connect();//������ �����Ѵ�.

        //// �̺�Ʈ �̸��� �����ͷ� ������ �޽��� ����
        //var messageToSend = new
        //{
        //    @event = "exampleEvent",  // @ ��ȣ�� ����Ͽ� ����� ���
        //    data = new
        //    {
        //        key1 = "value1",
        //        key2 = "value2"
        //    }
        //};

        //// JSON ���ڿ��� ��ȯ
        //string jsonMessage = JsonMapper.ToJson(messageToSend);

        //try
        //{
        //    // ������ �޽��� ����
        //    ws.Send(jsonMessage);
        //    Console.WriteLine("������ �޽��� ����: " + jsonMessage);
        //}
        //catch (InvalidOperationException ex)
        //{
        //    // Log and handle the error
        //    Debug.LogError("WebSocket is not open: " + ex.Message);
        //}

    }
    void ws_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data); // Received message logged

        // Parse the received message as JSON
        JsonData jsonData = JsonMapper.ToObject(e.Data);
        if (jsonData["event"].ToString().Equals("loginSuccess")) {
            try {
             // Extract user profile data
                JsonData userProfile = jsonData["data"]["userProfile"];
                UserManager.Instance.LoadUserFromJson(userProfile);
            } catch (Exception ex) {
                Debug.LogError("An error occurred: " + ex.Message);
            }
        }

        // Broadcast the received message to all listeners
        OnWebSocketMessageReceived?.Invoke(e.Data);
    }

    void ws_OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("open"); //����� �ֿܼ� "open"�̶�� ��´�.
    }
    void ws_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("close"); //����� �ֿܼ� "close"�̶�� ��´�.
    }
}