using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public Image lifeBarImage;  // Image 컴포넌트를 할당받을 변수

    public Sprite[] lifeSprites;  // 라이프 포인트에 따라 사용할 스프라이트 배열

    public GameObject gameOverPanel;

    void Start()
    {
        gameOverPanel.SetActive(false);  // 게임 오버 패널 비활성화
    }

    public void UpdateLifeUI(int lifePoints)
    {
        // 라이프 포인트에 따른 이미지 변경
        if (lifePoints >= 0 && lifePoints < lifeSprites.Length)
        {
            lifeBarImage.sprite = lifeSprites[lifePoints];
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);  // 게임 오버 패널 활성화
        // 추가적인 게임 오버 처리
    }
}
