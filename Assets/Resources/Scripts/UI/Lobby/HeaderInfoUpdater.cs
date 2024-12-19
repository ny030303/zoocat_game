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
        if (UserManager.Instance == null)
        {
            Debug.LogWarning("UserManager.Instance is null!");
            return;
        }

        // 유저 데이터 로드
        UserData userdata = UserManager.Instance.currentUser;
        if (userdata != null)
        {
            //Debug.Log("userdata: " + userdata.username);
            //UpdateHeaderInfo(userdata.username, "0", "0");
        }
        else
        {
            Debug.LogWarning("userdata is null!");
        }
       
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
