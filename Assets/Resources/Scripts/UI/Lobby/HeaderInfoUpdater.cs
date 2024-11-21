using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class HeaderInfoUpdater : MonoBehaviour
{
    // 각 TextMeshPro와 Image 요소들을 연결하기 위한 변수
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI freeCurrencyText;
    public TextMeshProUGUI paidCurrencyText;

    private void Start()
    {
        UserData userdata = UserManager.Instance.currentUser;
        Debug.Log("userdata: " + userdata.username);
        UpdateHeaderInfo(userdata.username, "0", "0");
    }
    // 정보를 업데이트하는 함수
    public void UpdateHeaderInfo(string name, string freeCurrency, string paidCurrency)
    {
        // 텍스트 업데이트
        userNameText.text = name;
        freeCurrencyText.text = freeCurrency;
        paidCurrencyText.text = paidCurrency;
    }
}
