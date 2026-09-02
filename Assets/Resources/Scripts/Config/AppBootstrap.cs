using LitJson;
using UnityEngine;

/// <summary>
/// 씬 로드 전에 1회 실행되어 활성 환경 설정을 적용한다.
///  - 런타임 로그 레벨 게이트 (Prod: Warning 미만 억제)
///  - 콘솔에 활성 환경 배너 출력
///  - LitJson 관용 임포터 등록 (서버가 bool 을 "true"/"false" 문자열로 보내는 케이스 방어)
/// 씬에 오브젝트를 둘 필요 없음.
/// </summary>
public static class AppBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        RegisterLenientJsonImporters();

        var cfg = AppConfig.Current;

        // filterLogType = Warning 이면 Log 는 억제되고 Warning/Assert/Error/Exception 은 유지된다.
        Debug.unityLogger.filterLogType = cfg.logLevel;

        Debug.LogWarning(
            $"[AppConfig] env={cfg.environmentName} socket={cfg.socketUrl} " +
            $"gpgs={cfg.gpgsEnabled} debugMenu={cfg.enableDebugMenu} logLevel={cfg.logLevel}");
    }

    /// 서버 응답의 타입이 스키마와 살짝 어긋나도(문자열 "true" ↔ bool 등) ToObject 가 죽지 않게.
    private static void RegisterLenientJsonImporters()
    {
        JsonMapper.RegisterImporter<string, bool>(s =>
            s == "true" || s == "True" || s == "1" || s == "TRUE");

        JsonMapper.RegisterImporter<string, int>(s =>
            int.TryParse(s, out int n) ? n : 0);

        JsonMapper.RegisterImporter<int, bool>(n => n != 0);
        JsonMapper.RegisterImporter<double, int>(d => (int)d);
    }
}
