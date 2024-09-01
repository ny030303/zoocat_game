using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;  // �ε� ȭ�� ������Ʈ
    public Slider progressBar;        // �ε� ���� �� (Slider UI ���)

    // ���� �񵿱������� �ε��ϴ� �ڷ�ƾ
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
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
