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
    private string userData;          // �������� ���� ���� ������

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
        // ���� �����͸� ������ ��û
        var messageToSend = new
        {
            @event = "joinLobby",  // ������ ���� �̺�Ʈ �̸�
            data = new
            {
                userId = "some_user_id"  // �ʿ信 ���� ���� ID ���� ����
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

        // ������� ���� �����͸� ��� (userData�� ����� ������)
        Debug.Log("User data received: " + userData);

        // TODO: userData�� �Ľ��ϰ� ���� ������ ����� �� �ֵ��� ó��
        // ����: var user = JsonUtility.FromJson<UserData>(userData);
    }

    // �����κ��� ���� �����͸� �޾��� �� �ݹ����� ó���ϴ� �Լ�
    public void OnUserDataReceived(string data)
    {
        userData = data; // �������� ���� �����͸� ����
        isUserDataLoaded = true; // ���� �����Ͱ� �ε�Ǿ����� ǥ��
    }

    // �񵿱� �� �ε� �� �ε� ȭ�� ǥ��
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // �ε� ȭ���� Ȱ��ȭ
        loadingScreen.SetActive(true);

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
