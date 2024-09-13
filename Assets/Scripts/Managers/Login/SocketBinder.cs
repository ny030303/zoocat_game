using LitJson;
using System;
using UnityEngine;

using WebSocketSharp;//�� ���� ���̺귯���� ����Ѵ�
using WebSocket = WebSocketSharp.WebSocket;

public class SocketBinder : MonoBehaviour
{

    public static SocketBinder Instance;

    private WebSocket ws;//���� ����

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
        Debug.Log("jsonData event: " + jsonData["event"].ToString()); // Received message logged
                                                                // Check if the message contains an "event" and that it's "loginSuccess"

        Debug.Log("jsonData true?" + jsonData["event"].ToString().Equals("loginSuccess"));
        if (jsonData["event"].ToString().Equals("loginSuccess"))
        {
            try
            {
             // Extract user profile data
            Debug.Log("userProfile data: " + jsonData["data"]["userProfile"].ToJson());
            JsonData userProfile = jsonData["data"]["userProfile"];
            string username = userProfile["username"].ToString();
            int level = int.Parse(userProfile["level"].ToString());
            int experience = int.Parse(userProfile["experience"].ToString());
            int gold = int.Parse(userProfile["gold"].ToString());
            int gems = int.Parse(userProfile["gems"].ToString());

            // Update UserManager singleton with the received data
            UserManager.Instance.userName = username;
            UserManager.Instance.userScore = level; // Or any other relevant field in your UserManager

            // Additional data can be stored or logged as needed
            Debug.Log("User data updated: " + username);
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred: " + ex.Message);
            }
        }
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