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
