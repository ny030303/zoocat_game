using LitJson;
using System;
using UnityEngine;

using WebSocketSharp;//웹 소켓 라이브러리를 사용한다
using WebSocket = WebSocketSharp.WebSocket;

public class SocketBinder : MonoBehaviour
{

    public static SocketBinder Instance;

    private WebSocket ws;//소켓 선언

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
        ws = new WebSocket("ws://192.168.1.151:3000");// 127.0.0.1은 본인의 아이피 주소이다. 3333포트로 연결한다는 의미이다.
        ws.OnMessage += ws_OnMessage; //서버에서 유니티 쪽으로 메세지가 올 경우 실행할 함수를 등록한다.
        ws.OnOpen += ws_OnOpen;//서버가 연결된 경우 실행할 함수를 등록한다
        ws.OnClose += ws_OnClose;//서버가 닫힌 경우 실행할 함수를 등록한다.
        ws.Connect();//서버에 연결한다.

        //// 이벤트 이름과 데이터로 구성된 메시지 생성
        //var messageToSend = new
        //{
        //    @event = "exampleEvent",  // @ 기호를 사용하여 예약어 사용
        //    data = new
        //    {
        //        key1 = "value1",
        //        key2 = "value2"
        //    }
        //};

        //// JSON 문자열로 변환
        //string jsonMessage = JsonMapper.ToJson(messageToSend);

        //try
        //{
        //    // 서버에 메시지 전송
        //    ws.Send(jsonMessage);
        //    Console.WriteLine("서버로 메시지 전송: " + jsonMessage);
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
        Debug.Log("open"); //디버그 콘솔에 "open"이라고 찍는다.
    }
    void ws_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("close"); //디버그 콘솔에 "close"이라고 찍는다.
    }
}