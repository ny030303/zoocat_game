using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�

public class HeaderInfoUpdater : MonoBehaviour
{
    // �� TextMeshPro�� Image ��ҵ��� �����ϱ� ���� ����
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI freeCurrencyText;
    public TextMeshProUGUI paidCurrencyText;

    private void Start()
    {
        UserData userdata = UserManager.Instance.currentUser;
        Debug.Log("userdata: " + userdata.username);
        UpdateHeaderInfo(userdata.username, "0", "0");
    }
    // ������ ������Ʈ�ϴ� �Լ�
    public void UpdateHeaderInfo(string name, string freeCurrency, string paidCurrency)
    {
        // �ؽ�Ʈ ������Ʈ
        userNameText.text = name;
        freeCurrencyText.text = freeCurrency;
        paidCurrencyText.text = paidCurrency;
    }
}
