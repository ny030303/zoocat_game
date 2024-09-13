using LitJson;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;  // �ε� ȭ�� ������Ʈ
    public Slider progressBar;        // �ε� ���� �� (Slider UI ���)
    private bool isUserDataLoaded = false; // ���� �����Ͱ� �ε�Ǿ����� Ȯ���ϴ� ����
    private JsonData units;          // �������� ���� ���� ������
    private IEnumerator WaitForSocketBinderAndSubscribe()
    {
        // SocketBinder.Instance�� null�� ��� ���� �ð� ���
        while (SocketBinder.Instance == null)
        {
            Debug.Log("Waiting for SocketBinder to initialize...");
            yield return new WaitForSeconds(0.1f);  // 0.1�� ��� �� �ٽ� Ȯ��
        }

        // SocketBinder�� �ʱ�ȭ�Ǹ� WebSocket �̺�Ʈ ����
        SocketBinder.Instance.OnWebSocketMessageReceived += OnWebSocketMessageReceived;
    }

    void OnEnable()
    {
        // Coroutine�� ���� SocketBinder�� �ʱ�ȭ�� ������ ���
        StartCoroutine(WaitForSocketBinderAndSubscribe());
    }


    void OnDisable()
    {
        SocketBinder.Instance.OnWebSocketMessageReceived -= OnWebSocketMessageReceived;
    }
    // Handle WebSocket message
    private void OnWebSocketMessageReceived(string data)
    {
        Debug.Log("OnWebSocketMessageReceived: " + data);
        // Parse the message and check if it's the user data
        JsonData jsonData = JsonMapper.ToObject(data);
        if (jsonData["event"].ToString() == "userJoined")
        {
            // Extract the user data
            units = jsonData["data"]["units"];
            isUserDataLoaded = true;
        }
    }

    // ���� �񵿱������� �ε��ϴ� �ڷ�ƾ
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadUserDataAndScene(sceneName));
    }

    // ���� �����͸� ������ ���� �ε��ϰ�, ���� �񵿱������� �ε��ϴ� �ڷ�ƾ
    private IEnumerator LoadUserDataAndScene(string sceneName)
    {
        // 1. ������ ���� �������� ���� �����͸� ��û
        yield return StartCoroutine(LoadUserDataFromServer());

        // 2. ���� �����͸� �ε��� �� ���� �ε�
        yield return StartCoroutine(LoadSceneAsync(sceneName));
    }

    // ������ ���� ���� �����͸� �񵿱������� �ε��ϴ� �޼���
    private IEnumerator LoadUserDataFromServer()
    {
        // �ε� ȭ���� Ȱ��ȭ
        loadingScreen.SetActive(true);

        // ���� �����͸� ������ ��û
        var messageToSend = new
        {
            @event = "joinLobby",  // ������ ���� �̺�Ʈ �̸�
            data = new
            {
                userId = UserManager.Instance.currentUser.id  // �ʿ信 ���� ���� ID ���� ����
            }
        };

        // JSON ���ڿ��� ��ȯ�Ͽ� ������ ����
        string jsonMessage = LitJson.JsonMapper.ToJson(messageToSend);
        try
        {
            SocketBinder.Instance.GetWs().Send(jsonMessage);  // ������ �޽��� ����
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError("WebSocket is not open: " + ex.Message);
            yield break;
        }

        // �����κ��� ������ ��ٸ� (���� �����Ͱ� �ε�� ������ ���)
        while (!isUserDataLoaded)
        {
            yield return null;
        }
        // TODO: userData�� �Ľ��ϰ� ���� ������ ����� �� �ֵ��� ó��
        // ����: var user = JsonUtility.FromJson<UserData>(userData);
        UserManager.Instance.LoadUserUnitsFromJson(units);
    }

    // �񵿱� �� �ε� �� �ε� ȭ�� ǥ��
    private IEnumerator LoadSceneAsync(string sceneName)
    {

        // �񵿱� �� �ε� ����
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // �ε� �Ϸ� �� �� ��ȯ ���

        // ���� ������ �ε�� ������ ���
        while (!asyncLoad.isDone)
        {
            // �ε� ���� ���¿� ���� �ε� �� ������Ʈ
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            progressBar.value = progress;

            // �ε��� �Ϸ�Ǹ� �� ��ȯ
            if (asyncLoad.progress >= 0.9f)
            {
                // �ʿ� �� �ε� �Ϸ� �� ��� ���
                yield return new WaitForSeconds(1f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
