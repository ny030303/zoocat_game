using UnityEditor;
using UnityEngine;

/// <summary>
/// 로컬 개발 편의: 코드 수정 없이 에디터 Play 의 활성 환경을 전환한다.
/// 스크립팅 심볼만 토글하며, 번들ID/productName 은 건드리지 않는다 (그건 <see cref="BuildScript"/> 몫).
/// 심볼 변경은 도메인 리로드/재컴파일을 유발한다.
///
/// AppConfig 규칙: ENV_PROD 정의 시 Env_Prod, 그 외(정의 없음 포함) Env_Dev.
/// 따라서 "Set Dev" 는 명시적 표기 + DEBUG_MENU 활성화 용도이고, 정의를 모두 지워도 Dev 로 동작한다.
/// </summary>
public static class EnvironmentSwitcher
{
    private const string DevDefines = "ENV_DEV;DEBUG_MENU";
    private const string ProdDefines = "ENV_PROD";

    [MenuItem("Tools/Environment/Set Dev")]
    public static void SetDev() => Apply(DevDefines);

    [MenuItem("Tools/Environment/Set Prod")]
    public static void SetProd() => Apply(ProdDefines);

    [MenuItem("Tools/Environment/Show Current")]
    public static void ShowCurrent()
    {
        string android = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string standalone = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        string resolved = android.Contains("ENV_PROD") ? "Env_Prod" : "Env_Dev";
        Debug.Log($"[Env] Android=\"{android}\"  Standalone=\"{standalone}\"  -> AppConfig 는 {resolved} 로드");
    }

    private static void Apply(string defines)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Env] scripting defines = \"{defines}\" (Android + Standalone). 도메인 리로드 후 적용됩니다.");
    }
}
