/// <summary>
/// 대전 종료 후 로비에서 1회 소비하는 보상 정보.
/// PvpBattleController 가 채우고, 로비(MatchmakingController)가 표시 후 null 로 비운다.
/// </summary>
public class MatchReward
{
    public PvpResult result;
    public int gold;
    public int gems;
}

/// <summary>
/// 현재 매치 정보. 씬 전환에도 유지되도록 static.
/// matchFound 로 채워지고, leaveMatch / opponentLeft / matchEnded 로 비워진다.
/// </summary>
public static class MatchSession
{
    public static bool InMatch { get; private set; }
    public static string MatchId { get; private set; }
    public static PlayerView You { get; private set; }
    public static PlayerView Opponent { get; private set; }

    /// PvP 유닛 롤/웨이브를 양측 동일하게 맞추기 위한 공유 시드.
    /// matchFound 엔 없으므로 Phase C 에서 matchMessage {t:"seed"} 교환으로 채운다.
    public static int SharedSeed { get; set; }

    /// 직전 대전 보상. 로비 진입 시 표시하고 소비(null)한다. Clear() 로는 지우지 않는다.
    public static MatchReward PendingReward { get; set; }

    public static void Begin(MatchFoundData d)
    {
        MatchId = d.matchId;
        You = d.you;
        Opponent = d.opponent;
        SharedSeed = 0;
        InMatch = true;
    }

    public static void Clear()
    {
        InMatch = false;
        MatchId = null;
        You = null;
        Opponent = null;
        SharedSeed = 0;
    }
}
