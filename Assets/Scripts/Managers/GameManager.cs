using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int playerHealth = 3;
    public int enemyHealth = 3;
    public int playerGold = 100;
    public Text playerHealthText;
    public Text playerGoldText;
    public GameObject victoryScreen;
    public GameObject defeatScreen;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        playerHealthText.text = "Health: " + playerHealth;
        playerGoldText.text = "Gold: " + playerGold;
    }

    public void TakeDamage(bool isPlayer, int damage)
    {
        if (isPlayer)
        {
            playerHealth -= damage;
            if (playerHealth <= 0)
            {
                Defeat();
            }
        }
        else
        {
            enemyHealth -= damage;
            if (enemyHealth <= 0)
            {
                Victory();
            }
        }
        UpdateUI();
    }

    void Victory()
    {
        victoryScreen.SetActive(true);
        // 추가 리워드 계산
    }

    void Defeat()
    {
        defeatScreen.SetActive(true);
        // 추가 패배 처리
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        UpdateUI();
    }
}
