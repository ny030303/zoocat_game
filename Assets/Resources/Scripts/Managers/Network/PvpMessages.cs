using System;
using System.Collections.Generic;

/// <summary>
/// Phase C 대전(matchMessage) 페이로드 타입 + 승패 판정 규칙.
/// 서버는 matchMessage 의 data 를 상대에게 그대로 중계한다 → 스키마는 클라가 정의.
/// LitJson JsonMapper 로 (역)직렬화하므로 필드는 전부 public, 기본 타입만 사용.
/// </summary>
public static class PvpProtocol
{
    public const int Version = 1;

    // matchMessage.data.t (control) 값
    public const string T_Hello = "hello";
    public const string T_Events = "ev";
    public const string T_End = "end";

    // 이벤트 배치 항목 kind
    public const string K_Spawn = "spawn";
    public const string K_Merge = "merge";
    public const string K_Upgrade = "upg";
    public const string K_Life = "life";
    public const string K_Wave = "wave";

    /// 양쪽 모두 "죽은" 것으로 간주하는 하드 타임캡(ms). int 범위(무난) 유지.
    public const int MatchCapMs = 5 * 60 * 1000;
}

/// 대전 시작 핸드셰이크. 양쪽이 보내고, seed 는 min 으로 합의한다.
[Serializable]
public class PvpHello
{
    public string matchId;
    public string t = PvpProtocol.T_Hello;
    public int seed;
    public string wave;   // 사용 중인 웨이브 드라이버 이름(정보용)
    public int ver = PvpProtocol.Version;
}

/// 0.5s 마다 flush 되는 게임플레이 이벤트 배치.
[Serializable]
public class PvpEventBatch
{
    public string matchId;
    public string t = PvpProtocol.T_Events;
    public List<PvpEvent> batch = new List<PvpEvent>();
}

/// 배치 항목. 필드는 kind 에 따라 선택적으로 채워진다(안 쓰는 값은 기본값).
[Serializable]
public class PvpEvent
{
    public string k;      // spawn | merge | upg | life | wave
    public string u;      // spawn: unitID / upg: unitID
    public string a;      // merge: 소비된(사라진) unitID
    public string r;      // merge: 결과 unitID
    public int c = -1;    // spawn/merge: 그리드 셀 인덱스
    public int n = -1;    // upg: unitNumber
    public int v = -1;    // life: 남은 라이프 / wave: 웨이브 인덱스
    public int ts;        // 전투 시작 이후 경과(ms)
}

/// 로컬 게임오버/승리 시 1회 전송. 상대의 상호검증용.
[Serializable]
public class PvpEnd
{
    public string matchId;
    public string t = PvpProtocol.T_End;
    public string res;    // win | lose | draw (보낸 쪽 기준)
    public int wave;      // 사망 시점 웨이브
    public int ts;        // 사망 시점(ms). 생존 중이면 -1
}

public enum PvpResult { Undecided, Win, Lose, Draw }

/// <summary>
/// 양쪽 클라가 동일 입력으로 동일 결과를 내는 결정적 판정.
/// 입력: 내/상대의 (사망ms, 사망웨이브), 생존 중이면 deadTs = -1.
/// </summary>
public static class PvpJudge
{
    public static PvpResult Decide(
        int myDeadTs, int myWave,
        int oppDeadTs, int oppWave,
        bool opponentGone)
    {
        if (opponentGone) return PvpResult.Win;

        bool meDead = myDeadTs >= 0;
        bool oppDead = oppDeadTs >= 0;

        if (!meDead && oppDead) return PvpResult.Win;
        if (meDead && !oppDead) return PvpResult.Lose;

        if (meDead && oppDead)
        {
            if (myWave != oppWave) return myWave > oppWave ? PvpResult.Win : PvpResult.Lose;
            if (myDeadTs != oppDeadTs) return myDeadTs > oppDeadTs ? PvpResult.Win : PvpResult.Lose;
            return PvpResult.Draw;
        }

        // 둘 다 생존(타임캡 도달) → 진행 웨이브로만 비교
        if (myWave != oppWave) return myWave > oppWave ? PvpResult.Win : PvpResult.Lose;
        return PvpResult.Draw;
    }

    public static string ToWire(PvpResult r)
    {
        switch (r)
        {
            case PvpResult.Win: return "win";
            case PvpResult.Lose: return "lose";
            case PvpResult.Draw: return "draw";
            default: return "undecided";
        }
    }
}
