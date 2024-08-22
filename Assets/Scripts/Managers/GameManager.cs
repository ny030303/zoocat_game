using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int playerLifePoints = 3;
    public LifeManager playerLifeManager;

    private int aiLifePoints = 3;
    public LifeManager aiLifeManager;

    private int summonCost = 10;
    private int maxSummonCost = 50;
    private int currency = 100;
    public TextMeshPro currencyTextObj;
    public UnitDatabase unitDatabase;

    // currency 바뀌었을때 핸들러
    public delegate void SummonStateHandler();
    public event SummonStateHandler OnCurrencyChanged;
    void Awake()
    {
        unitDatabase.Initialize();
    }
     void Start()
    {
        //playerLifeManager = FindObjectOfType<LifeManager>();
    }
    public bool CheckButtonState()  { return currency >= summonCost ? true : false; }
    public bool CheckLevelUpgradeState(int upgradeCost) { return currency >= upgradeCost ? true : false; }
    public bool SummonUnit()
    {
        if (currency >= summonCost)
        {
            // 유닛 소환 로직
            currency -= summonCost;
            ChangeCurrency();
            summonCost = Mathf.Min(summonCost + 10, maxSummonCost);
            return true;
        }
        else
        {
            // 재화 부족 알림
            return false;
        }
    }

    public void UpgradeUnit(int upgradeCost)
    {
        if (currency >= upgradeCost)
        {
            currency -= upgradeCost;
            ChangeCurrency();
        }
        else
        {
            // 업그레이드 불가능 알림
        }
    }
    public void AddGold(int rewardGold)
    {
        currency += rewardGold;
        ChangeCurrency();
    }

    private void ChangeCurrency()
    {
        currencyTextObj.text = currency.ToString();
        if (OnCurrencyChanged != null) OnCurrencyChanged();
    }

    public void TakeDamage(string owner)
    {
        if(owner == "player")
        {
            if (playerLifePoints > 0)
            {
                playerLifePoints--;
                UpdateLifeUI(playerLifeManager, playerLifePoints);
            }

            if (playerLifePoints <= 0) GameOver();
        } else if(owner == "ai")
        {
            if (aiLifePoints > 0)
            {
                aiLifePoints--;
                UpdateLifeUI(aiLifeManager, aiLifePoints);
            }

            if (aiLifePoints <= 0) Win();
        }
        
    }
    void UpdateLifeUI(LifeManager lifeManager, int lifePoints)
    {
        if (lifeManager != null) lifeManager.UpdateLifeUI(lifePoints);
    }
    void GameOver()
    {
        Time.timeScale = 0;
        if (playerLifeManager != null) playerLifeManager.GameOver();
        // 추가적인 게임 오버 처리 (예: 게임 중지, 점수 저장 등)
    }
    void Win()
    {
        Time.timeScale = 0;
        if (aiLifeManager != null) aiLifeManager.GameOver();
        // 추가적인 게임 승리 처리 (예: 게임 중지, 점수 저장 등)
    }
}
