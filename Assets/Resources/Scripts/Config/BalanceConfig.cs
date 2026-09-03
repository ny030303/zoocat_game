using UnityEngine;

/// <summary>
/// 전투 / 성장 / 경제 / 웨이브 / 보상 밸런스 수치의 단일 소스.
/// 코드에서 리터럴(1.1f, 0.5f, 3 …) 대신 <c>BalanceConfig.Current.X</c> 를 읽는다.
///
/// 에셋: <c>Assets/Resources/Config/Balance.asset</c>
///  - 없으면 이 스크립트의 필드 기본값으로 폴백 (게임은 그대로 동작).
///  - 만들려면 Project 창에서 <b>Create ▸ Config ▸ Balance Config</b>, 파일명 <c>Balance</c>.
///
/// 밸런스 패치 = 이 에셋 하나 수정 → PR. 런타임(서버 원격 config) 오버라이드는 <see cref="Override"/>.
/// </summary>
[CreateAssetMenu(fileName = "Balance", menuName = "Config/Balance Config")]
public class BalanceConfig : ScriptableObject
{
    [Header("전투")]
    public int startLives = 3;                    // 플레이어/AI 시작 라이프
    [Min(1f)] public float critMultiplier = 2f;   // 치명타 데미지 배수

    [Header("유닛 성장")]
    [Min(1f)] public float levelUpAtkMul = 1.1f;  // 레벨당 공격력 배수 (1.1 = +10%/lv)
    [Min(0f)] public float mergeAtkBonus = 0.5f;  // 머지 시 공격력 보너스 (0.5 = +50%)
    public int maxGrade = 5;                      // 머지 최대 등급
    public float mergeScalePerGrade = 0.1f;       // 등급당 유닛 크기 증가

    [Header("인게임 경제 (소환 화폐)")]
    public int summonCostStart = 10;
    public int summonCostMax = 50;
    public int summonCostStep = 10;               // 소환할 때마다 증가
    public int startCurrency = 100;               // 전투 시작 화폐

    [Header("웨이브")]
    [Min(0.05f)] public float enemySpawnInterval = 1.5f; // 적 스폰 간격(초)
    [Min(0f)] public float betweenWaveDelay = 5f;        // 웨이브 사이 대기(초)

    [Header("웨이브 무한 연장 (authored CSV 소진 후)")]
    public float endlessHpAddPerWave = 20f;        // 추가 웨이브당 hpAdditional 증가량
    public float endlessCountRampPerWave = 0.12f;  // 추가 웨이브당 몬스터 수 배수 증가 (0.12 = +12%/wave)
    public float endlessTimeReducePerWave = 1.5f;  // 추가 웨이브당 timeLimit 감소(초)
    [Min(5f)] public float endlessMinTimeLimit = 25f; // timeLimit 하한

    [Header("PvP 대전 보상")]
    public int pvpWinGold = 120;
    public int pvpWinGems = 2;
    public int pvpLoseGold = 30;
    public int pvpLoseGems = 0;
    public int pvpDrawGold = 60;
    public int pvpDrawGems = 1;

    [Header("신규 게스트 시작 재화")]
    public int guestStartGold = 1000;
    public int guestStartGems = 0;

    // ---------------------------------------------------------------- 접근점

    private static BalanceConfig _current;

    public static BalanceConfig Current
    {
        get
        {
            if (_current != null) return _current;
            _current = Resources.Load<BalanceConfig>("Config/Balance");
            if (_current == null)
            {
                Debug.LogWarning("[BalanceConfig] Resources/Config/Balance 에셋이 없어 스크립트 기본값을 사용합니다.");
                _current = CreateInstance<BalanceConfig>();
            }
            return _current;
        }
    }

    /// 서버 원격 config 등으로 런타임에 밸런스를 교체할 때 사용.
    public static void Override(BalanceConfig cfg)
    {
        if (cfg != null) _current = cfg;
    }
}
