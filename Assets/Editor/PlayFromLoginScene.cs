#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 에디터에서 Play 를 누르면 항상 LoginScene 부터 시작하도록 한다.
///
/// 실기기 APK 는 빌드 씬 0번(LoginScene)에서 부팅하므로 SocketBinder / UserManager /
/// LoginManager 같은 LoginScene 상주 매니저가 생성되지만, 에디터는 "열려 있는 씬"에서
/// 시작하기 때문에 GameScenePvP / LobbyTestScene 에서 Play 하면 그 매니저들이 없어
/// 서버 연결·로그인·매칭이 전부 동작하지 않는다.
///
/// 메뉴 <b>Tools ▸ Play From LoginScene</b> 로 켜고 끈다 (에디터 환경설정에 저장).
/// </summary>
[InitializeOnLoad]
public static class PlayFromLoginScene
{
    private const string MenuPath = "Tools/Play From LoginScene";
    private const string PrefKey = "zoocat.playFromLoginScene";
    private const string LoginScenePath = "Assets/Resources/Scenes/LoginScene.unity";

    static PlayFromLoginScene() => EditorApplication.delayCall += Apply;

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, false);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        Enabled = !Enabled;
        Apply();
        Debug.Log($"[PlayFromLoginScene] {(Enabled ? "ON — Play 는 항상 LoginScene 부터" : "OFF — 열린 씬에서 Play")}");
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void Apply()
    {
        EditorSceneManager.playModeStartScene = Enabled
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(LoginScenePath)
            : null;
    }
}
#endif
