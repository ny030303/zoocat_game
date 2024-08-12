using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextController : MonoBehaviour
{
    public Text damageText;
    public float fadeDuration = 1f; // FadeOut ���� �ð�
    public float moveSpeed = 0.5f;  // �ؽ�Ʈ�� ���� �̵��ϴ� �ӵ�

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

            // �ؽ�Ʈ�� ���� �̵�
            transform.position = originalPosition + new Vector3(0, moveSpeed * currentTime, 0);

            // ���� ����
            canvasGroup.alpha = Mathf.Clamp01(1f - (currentTime / fadeDuration));

            yield return null;
        }

        Destroy(gameObject); // �ؽ�Ʈ�� ������� ������Ʈ ����
    }
}
