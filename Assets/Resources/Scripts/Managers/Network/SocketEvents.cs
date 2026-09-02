/// <summary>
/// 서버 WebSocket 이벤트 이름 상수. 오타 = 컴파일 에러.
/// 기준: zoocat_game_server 클라이언트 연동 명세서.
/// </summary>
public static class SocketEvents
{
    // --- 인증 ---
    public const string Login = "login";
    public const string LoginSuccess = "loginSuccess";
    public const string LoginError = "loginError";

    // --- 로비 ---
    public const string JoinLobby = "joinLobby";
    public const string UserJoined = "userJoined";

    // --- 덱 ---
    public const string UpdateDeck = "updateDeck";
    public const string DeckUpdated = "deckUpdated";
    public const string DeckUpdateError = "deckUpdateError";

    // --- 범용 ---
    public const string Error = "error";

    // --- 매칭 (Phase B) ---
    public const string Enqueue = "enqueue";
    public const string Dequeue = "dequeue";
    public const string Queued = "queued";
    public const string QueueLeft = "queueLeft";
    public const string MatchFound = "matchFound";

    // --- 대전 (Phase C) ---
    public const string MatchMessage = "matchMessage";
    public const string LeaveMatch = "leaveMatch";
    public const string OpponentLeft = "opponentLeft";
    public const string MatchEnded = "matchEnded";
}
