using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public string userName;
    public int userScore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ��ȯ�Ǵ��� �� ��ü�� �ı����� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� ������ ���ο� ��ü�� �ı���
        }
    }
}
