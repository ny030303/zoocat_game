using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int playerLifePoints;
    public LifeManager playerLifeManager;

    private int aiLifePoints;
    public LifeManager aiLifeManager;

    private int summonCost;
    private int maxSummonCost;
    private int currency;
    public TextMeshPro currencyTextObj;
    public UnitDatabase unitDatabase;

    // currency �ٲ������ �ڵ鷯
    public delegate void SummonStateHandler();
    public event SummonStateHandler OnCurrencyChanged;

    // ---- Phase C 대전 훅 ----
    /// true 면 로컬 AI 레인 누수는 무시하고, aiLifePoints 는 SetOpponentLife 로만 갱신된다.
    public bool pvpMode = false;
    public event Action<int> OnPlayerLifeChanged;   // 내 라이프가 바뀔 때 (남은 값)
    public event Action<int> OnOpponentLifeChanged; // 상대 라이프가 바뀔 때 (남은 값)
    public event Action OnLocalGameOver;            // 내 라이프 0
    public event Action OnLocalWin;                 // 상대 라이프 0 (로컬 판정) 또는 ForceEnd(true)

    public int PlayerLife => playerLifePoints;
    public int OpponentLife => aiLifePoints;
    void Awake()
    {
        var b = BalanceConfig.Current;
        playerLifePoints = aiLifePoints = b.startLives;
        summonCost = b.summonCostStart;
        maxSummonCost = b.summonCostMax;
        currency = b.startCurrency;

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
            // ���� ��ȯ ����
            currency -= summonCost;
            ChangeCurrency();
            summonCost = Mathf.Min(summonCost + BalanceConfig.Current.summonCostStep, maxSummonCost);
            return true;
        }
        else
        {
            // ��ȭ ���� �˸�
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
            // ���׷��̵� �Ұ��� �˸�
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
                OnPlayerLifeChanged?.Invoke(playerLifePoints);
            }

            if (playerLifePoints <= 0) GameOver();
        } else if(owner == "ai")
        {
            if (pvpMode) return; // 대전 중엔 로컬 AI 레인 누수 무시 (상대 라이프는 네트워크로)

            if (aiLifePoints > 0)
            {
                aiLifePoints--;
                UpdateLifeUI(aiLifeManager, aiLifePoints);
            }

            if (aiLifePoints <= 0) Win();
        }

    }

    // ---- Phase C: 상대 라이프를 네트워크 값으로 갱신 ----
    public void SetOpponentLife(int value)
    {
        aiLifePoints = Mathf.Max(0, value);
        UpdateLifeUI(aiLifeManager, aiLifePoints);
        OnOpponentLifeChanged?.Invoke(aiLifePoints);
        if (pvpMode && aiLifePoints <= 0) Win();
    }

    /// 네트워크 판정(상대 이탈 등)으로 강제 종료.
    public void ForceEnd(bool won)
    {
        if (won) Win();
        else GameOver();
    }
    void UpdateLifeUI(LifeManager lifeManager, int lifePoints)
    {
        if (lifeManager != null) lifeManager.UpdateLifeUI(lifePoints);
    }
    private bool _ended;
    void GameOver()
    {
        if (_ended) return;
        _ended = true;
        Time.timeScale = 0;
        if (playerLifeManager != null) playerLifeManager.GameOver();
        OnLocalGameOver?.Invoke();
        // �߰����� ���� ���� ó�� (��: ���� ����, ���� ���� ��)
    }
    void Win()
    {
        if (_ended) return;
        _ended = true;
        Time.timeScale = 0;
        if (aiLifeManager != null) aiLifeManager.GameOver();
        OnLocalWin?.Invoke();
        // �߰����� ���� �¸� ó�� (��: ���� ����, ���� ���� ��)
    }
}
