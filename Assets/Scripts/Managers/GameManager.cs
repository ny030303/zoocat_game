using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int lifePoints = 3;
    private int currency = 100;
    public TextMeshPro currencyTextObj;
    private LifeManager lifeManager;
    public UnitDatabase unitDatabase;

    private int summonCost = 10;
    private int maxSummonCost = 50;
    private int waveNumber = 1;

    // currency 바뀌었을때 핸들러
    public delegate void SummonStateHandler();
    public event SummonStateHandler OnCurrencyChanged;

    void Start()
    {
        lifeManager = FindObjectOfType<LifeManager>();
        unitDatabase.Initialize();
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

    public void TakeDamage()
    {
        if (lifePoints > 0)
        {
            lifePoints--;
            UpdateLifeUI();
        }

        if (lifePoints <= 0) GameOver();
    }
    void UpdateLifeUI()
    {
        if (lifeManager != null) lifeManager.UpdateLifeUI(lifePoints);
    }
    void GameOver()
    {
        if (lifeManager != null) lifeManager.GameOver();
        // 추가적인 게임 오버 처리 (예: 게임 중지, 점수 저장 등)
    }
}
