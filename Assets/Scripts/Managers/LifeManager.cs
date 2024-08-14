using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public Image lifeBarImage;  // Image ������Ʈ�� �Ҵ���� ����

    public Sprite[] lifeSprites;  // ������ ����Ʈ�� ���� ����� ��������Ʈ �迭

    public GameObject gameOverPanel;

    void Start()
    {
        gameOverPanel.SetActive(false);  // ���� ���� �г� ��Ȱ��ȭ
    }

    public void UpdateLifeUI(int lifePoints)
    {
        // ������ ����Ʈ�� ���� �̹��� ����
        if (lifePoints >= 0 && lifePoints < lifeSprites.Length)
        {
            lifeBarImage.sprite = lifeSprites[lifePoints];
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);  // ���� ���� �г� Ȱ��ȭ
        // �߰����� ���� ���� ó��
    }
}
