using UnityEngine;

/// <summary>
/// 활성 환경 설정에 대한 단일 접근 지점.
/// 빌드 시 정의되는 스크립팅 심볼로 어느 <see cref="GameEnvironmentConfig"/> 에셋을 로드할지 결정한다.
///   ENV_PROD  -> Resources/Config/Env_Prod
///   그 외(정의 없음/ENV_DEV) -> Resources/Config/Env_Dev
/// 게임 코드는 하드코딩된 URL 대신 AppConfig.Current 를 사용한다.
/// </summary>
public static class AppConfig
{
    private static GameEnvironmentConfig _current;

    public static GameEnvironmentConfig Current
    {
        get
        {
            if (_current != null) return _current;

#if ENV_PROD
            _current = Resources.Load<GameEnvironmentConfig>("Config/Env_Prod");
#else
            _current = Resources.Load<GameEnvironmentConfig>("Config/Env_Dev");
#endif
            if (_current == null)
            {
                Debug.LogError("[AppConfig] Env config 에셋을 찾지 못했습니다. Resources/Config/Env_Dev(또는 Env_Prod) 확인. 안전 기본값으로 대체합니다.");
                _current = ScriptableObject.CreateInstance<GameEnvironmentConfig>();
            }
            return _current;
        }
    }
}
