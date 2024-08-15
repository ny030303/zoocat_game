using UnityEngine;

public class LevelTextUpdater : MonoBehaviour
{
    public TextMesh levelText;
    private int currentLevel = 0;

    void Start()
    {
        levelText = GetComponent<TextMesh>();
        // �ʱ� �ؽ�Ʈ ����
        UpdateLevelText();
    }

    // ������ ������Ű�� �޼���
    public void IncreaseLevel()
    {
        currentLevel++;
        UpdateLevelText();
    }

    // �ؽ�Ʈ�� ������Ʈ�ϴ� �޼���
    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Lv. " + currentLevel;
        }
    }
}
