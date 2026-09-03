using System;

/// <summary>
/// 서버 수신 페이로드용 직렬화 타입. LitJson JsonMapper.ToObject&lt;T&gt; 로 파싱.
/// (matchMessage 의 payload 는 스키마가 유동적이라 JsonData 로 별도 취급)
/// </summary>

[Serializable]
public class RegisteredData
{
    public string userId;
    public string deviceId;
    public string deviceSecret;
    public string token;
    public UserData userProfile;
}

[Serializable]
public class LoginSuccessData
{
    public string token;        // login 응답에만. resumeSession 응답엔 없음
    public UserData userProfile;
}

[Serializable]
public class PlayerView
{
    public string userId;
    public string username;
    public int level;
    public string[] deck;
}

[Serializable]
public class MatchFoundData
{
    public string matchId;
    public PlayerView you;
    public PlayerView opponent;
}

[Serializable]
public class MatchEndedData
{
    public string matchId;
    public string reason;
}

[Serializable]
public class MatchRef
{
    public string matchId;
}
