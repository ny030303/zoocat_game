using LitJson;
using System;
using UnityEngine;

using WebSocketSharp;//�� ���� ���̺귯���� ����Ѵ�
using WebSocket = WebSocketSharp.WebSocket;

public class SocketBinder : MonoBehaviour
{
   

    private static WebSocket ws;//���� ����

    public static WebSocket GetWs() { return ws; }
    void Start()
    {
        ws = new WebSocket("ws://192.168.1.151:3000");// 127.0.0.1�� ������ ������ �ּ��̴�. 3333��Ʈ�� �����Ѵٴ� �ǹ��̴�.
        ws.OnMessage += ws_OnMessage; //�������� ����Ƽ ������ �޼����� �� ��� ������ �Լ��� ����Ѵ�.
        ws.OnOpen += ws_OnOpen;//������ ����� ��� ������ �Լ��� ����Ѵ�
        ws.OnClose += ws_OnClose;//������ ���� ��� ������ �Լ��� ����Ѵ�.
        ws.Connect();//������ �����Ѵ�.

        // �̺�Ʈ �̸��� �����ͷ� ������ �޽��� ����
        var messageToSend = new
        {
            @event = "exampleEvent",  // @ ��ȣ�� ����Ͽ� ����� ���
            data = new
            {
                key1 = "value1",
                key2 = "value2"
            }
        };

        // JSON ���ڿ��� ��ȯ
        string jsonMessage = JsonMapper.ToJson(messageToSend);

        try
        {
            // ������ �޽��� ����
            ws.Send(jsonMessage);
            Console.WriteLine("������ �޽��� ����: " + jsonMessage);
        }
        catch (InvalidOperationException ex)
        {
            // Log and handle the error
            Debug.LogError("WebSocket is not open: " + ex.Message);
        }

    }
    void ws_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);//���� �޼����� ����� �ֿܼ� ����Ѵ�.
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