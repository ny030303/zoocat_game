using UnityEngine;

/// <summary>
/// 환경(dev/prod)별 클라이언트 설정. Resources/Config/ 아래에 Env_Dev / Env_Prod 두 개의 에셋으로 존재하고,
/// 빌드 시 스크립팅 심볼(ENV_DEV / ENV_PROD)에 따라 <see cref="AppConfig"/>가 하나만 로드한다.
/// 게임 코드는 이 타입을 직접 로드하지 말고 항상 AppConfig.Current 를 통해 접근한다.
/// </summary>
[CreateAssetMenu(fileName = "Env_", menuName = "Zoocat/Environment Config")]
public class GameEnvironmentConfig : ScriptableObject
{
    [Tooltip("표시용 환경 이름 (Dev / Prod)")]
    public string environmentName = "Dev";

    [Tooltip("WebSocket 접속 주소 (ws:// 또는 wss://). 예: ws://localhost:3000, wss://game.zoocat.cloud")]
    public string socketUrl = "ws://localhost:3000";

    [Tooltip("예약 필드 - 현재 REST API 없음. 어디에도 배선되어 있지 않음")]
    public string apiBaseUrl = "";

    [Tooltip("이 레벨 미만의 로그는 런타임에서 억제된다. Dev=Log(전체), Prod=Warning")]
    public LogType logLevel = LogType.Log;

    [Tooltip("디버그 메뉴/치트 노출 여부")]
    public bool enableDebugMenu = true;

    [Tooltip("Google Play Games 초기화 여부. dev 빌드는 bundle id 불일치로 false")]
    public bool gpgsEnabled = false;
}
