using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage Instance;

    public Text messageText;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;
    private Coroutine currentCoroutine;
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Show(string message, float duration = 2f)
    {
        if (Instance != null)
        {
            Instance.DisplayMessage(message, duration);
        }
    }

    private void DisplayMessage(string message, float duration)
    {
        if (isShowing)
        {
            // 메시지가 떠 있는 상태이면 메시지만 변경하고 기존 코루틴을 중단
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            messageText.text = message;
            currentCoroutine = StartCoroutine(ContinueDisplaying(duration));
        }
        else
        {
            // 메시지가 떠 있지 않다면 정상적으로 표시
            currentCoroutine = StartCoroutine(ShowMessage(message, duration));
        }
    }

    private IEnumerator ShowMessage(string message, float duration)
    {
        isShowing = true;
        messageText.text = message;

        // Fade In
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        isShowing = false;
    }

    private IEnumerator ContinueDisplaying(float duration)
    {
        // 메시지만 갱신하고 기존 표시 상태 유지
        yield return new WaitForSeconds(duration);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        isShowing = false;
    }

    private IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
