using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;  // 로딩 화면 오브젝트
    public Slider progressBar;        // 로딩 진행 바 (Slider UI 요소)

    // 씬을 비동기적으로 로드하는 코루틴
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // 비동기 씬 로드 및 로딩 화면 표시
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 화면을 활성화
        loadingScreen.SetActive(true);

        // 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 로딩 완료 후 씬 전환 대기

        // 씬이 완전히 로드될 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상태에 따라 로딩 바 업데이트
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            progressBar.value = progress;

            // 로딩이 완료되면 씬 전환
            if (asyncLoad.progress >= 0.9f)
            {
                // 필요 시 로딩 완료 후 잠시 대기
                yield return new WaitForSeconds(1f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
