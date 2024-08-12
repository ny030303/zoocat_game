using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextController : MonoBehaviour
{
    public Text damageText;
    public float fadeDuration = 1f; // FadeOut 지속 시간
    public float moveSpeed = 0.5f;  // 텍스트가 위로 이동하는 속도

    private float startAlpha;

    void Start()
    {
        startAlpha = damageText.color.a;
        StartCoroutine(FadeOutAndMove());
    }

    public void SetText(int damage)
    {
        damageText.text = damage.ToString();
    }

    IEnumerator FadeOutAndMove()
    {
        float currentTime = 0f;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        Vector3 originalPosition = transform.position;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;

            // 텍스트를 위로 이동
            transform.position = originalPosition + new Vector3(0, moveSpeed * currentTime, 0);

            // 투명도 조절
            canvasGroup.alpha = Mathf.Clamp01(1f - (currentTime / fadeDuration));

            yield return null;
        }

        Destroy(gameObject); // 텍스트가 사라지면 오브젝트 삭제
    }
}
