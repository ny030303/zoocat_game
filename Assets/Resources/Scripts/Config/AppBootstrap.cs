using UnityEngine;

/// <summary>
/// 씬 로드 전에 1회 실행되어 활성 환경 설정을 적용한다.
///  - 런타임 로그 레벨 게이트 (Prod: Warning 미만 억제)
///  - 콘솔에 활성 환경 배너 출력
/// 씬에 오브젝트를 둘 필요 없음.
/// </summary>
public static class AppBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        var cfg = AppConfig.Current;

        // filterLogType = Warning 이면 Log 는 억제되고 Warning/Assert/Error/Exception 은 유지된다.
        Debug.unityLogger.filterLogType = cfg.logLevel;

        Debug.LogWarning(
            $"[AppConfig] env={cfg.environmentName} socket={cfg.socketUrl} " +
            $"gpgs={cfg.gpgsEnabled} debugMenu={cfg.enableDebugMenu} logLevel={cfg.logLevel}");
    }
}
