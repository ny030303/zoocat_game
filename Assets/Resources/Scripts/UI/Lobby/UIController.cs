using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject uiPanel; // UI 창
    public Button openButton; // 열기 버튼
    public Button closeButton; // 닫기 버튼

    void Start()
    {
        // 초기 UI 창 비활성화
        uiPanel.SetActive(false);

        // 버튼 이벤트 연결
        openButton.onClick.AddListener(OpenUIPanel);
        closeButton.onClick.AddListener(CloseUIPanel);

    }

    // UI 창 열기
    void OpenUIPanel()
    {
        uiPanel.SetActive(true);
    }

    // UI 창 닫기
    void CloseUIPanel()
    {
        uiPanel.SetActive(false);
    }
}
