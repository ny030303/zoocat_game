using UnityEngine;

public class LevelTextUpdater : MonoBehaviour
{
    public TextMesh levelText;
    private int currentLevel = 0;

    void Start()
    {
        levelText = GetComponent<TextMesh>();
        // 초기 텍스트 설정
        UpdateLevelText();
    }

    // 레벨을 증가시키는 메서드
    public void IncreaseLevel()
    {
        currentLevel++;
        UpdateLevelText();
    }

    // 텍스트를 업데이트하는 메서드
    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Lv. " + currentLevel;
        }
    }
}
