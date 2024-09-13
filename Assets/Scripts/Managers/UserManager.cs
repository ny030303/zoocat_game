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
            DontDestroyOnLoad(gameObject); // 씬이 전환되더라도 이 객체가 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 새로운 객체를 파괴함
        }
    }
}
