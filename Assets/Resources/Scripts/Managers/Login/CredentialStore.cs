using UnityEngine;

/// <summary>
/// 서버 auth-session 자격증명 저장소.
///  - Android: androidx.security EncryptedSharedPreferences (파일명 zoocat_creds)
///  - 그 외/에디터: PlayerPrefs 폴백
///
/// deviceSecret 이 진짜 자격증명이다. 로그/분석/URL/클립보드에 절대 노출 금지.
/// register 응답으로 userId·deviceId·deviceSecret·token 을 받아 SaveRegistration 으로 저장,
/// login/resumeSession 응답의 새 token 은 Token 세터로 갱신한다.
/// </summary>
public static class CredentialStore
{
    private const string KUserId = "cred.userId";
    private const string KDeviceId = "cred.deviceId";
    private const string KSecret = "cred.deviceSecret";
    private const string KToken = "cred.token";
    private const string KName = "cred.name";
    private const string KUnderage = "cred.underage";
    private const string KPendingLink = "cred.pendingGpgsLink"; // Phase 2 계정연동 준비용

    public static bool HasCredentials =>
        !string.IsNullOrEmpty(Get(KUserId)) &&
        !string.IsNullOrEmpty(Get(KDeviceId)) &&
        !string.IsNullOrEmpty(Get(KSecret));

    public static string UserId => Get(KUserId);
    public static string DeviceId => Get(KDeviceId);
    public static string DeviceSecret => Get(KSecret);
    public static string Name => Get(KName);
    public static bool Underage => Get(KUnderage) == "1";

    public static string Token
    {
        get => Get(KToken);
        set { Set(KToken, value); Flush(); }
    }

    /// GPGS 로그인은 했지만 서버 provider 연동(Phase 2)이 아직인 경우 그 googleId 를 보관.
    public static string PendingGpgsLink
    {
        get => Get(KPendingLink);
        set { Set(KPendingLink, value); Flush(); }
    }

    public static void SaveRegistration(string userId, string deviceId, string deviceSecret,
                                        string token, string name, bool underage)
    {
        Set(KUserId, userId);
        Set(KDeviceId, deviceId);
        Set(KSecret, deviceSecret);
        Set(KToken, token);
        Set(KName, name);
        Set(KUnderage, underage ? "1" : "0");
        Flush();
    }

    public static void Clear()
    {
        foreach (var k in new[] { KUserId, KDeviceId, KSecret, KToken, KName, KUnderage, KPendingLink })
            Remove(k);
        Flush();
    }

    // ------------------------------------------------------------------ backend

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject _prefs;

    private static AndroidJavaObject Prefs
    {
        get
        {
            if (_prefs != null) return _prefs;
            try
            {
                using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = up.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var appCtx = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var masterKeys = new AndroidJavaClass("androidx.security.crypto.MasterKeys"))
                using (var spec = masterKeys.GetStatic<AndroidJavaObject>("AES256_GCM_SPEC"))
                using (var esp = new AndroidJavaClass("androidx.security.crypto.EncryptedSharedPreferences"))
                using (var keyScheme = new AndroidJavaClass("androidx.security.crypto.EncryptedSharedPreferences$PrefKeyEncryptionScheme"))
                using (var valScheme = new AndroidJavaClass("androidx.security.crypto.EncryptedSharedPreferences$PrefValueEncryptionScheme"))
                {
                    string alias = masterKeys.CallStatic<string>("getOrCreate", spec);
                    _prefs = esp.CallStatic<AndroidJavaObject>("create",
                        "zoocat_creds", alias, appCtx,
                        keyScheme.GetStatic<AndroidJavaObject>("AES256_SIV"),
                        valScheme.GetStatic<AndroidJavaObject>("AES256_GCM"));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CredentialStore] EncryptedSharedPreferences init 실패, PlayerPrefs 폴백: " + e.Message);
                _prefs = null;
            }
            return _prefs;
        }
    }

    private static string Get(string k)
    {
        var p = Prefs;
        if (p == null) return PlayerPrefs.GetString(k, null);
        string v = p.Call<string>("getString", k, null);
        return string.IsNullOrEmpty(v) ? null : v;
    }

    private static void Set(string k, string v)
    {
        var p = Prefs;
        if (p == null) { PlayerPrefs.SetString(k, v ?? ""); return; }
        using (var editor = p.Call<AndroidJavaObject>("edit"))
        {
            editor.Call<AndroidJavaObject>("putString", k, v ?? "");
            editor.Call<bool>("commit");
        }
    }

    private static void Remove(string k)
    {
        var p = Prefs;
        if (p == null) { PlayerPrefs.DeleteKey(k); return; }
        using (var editor = p.Call<AndroidJavaObject>("edit"))
        {
            editor.Call<AndroidJavaObject>("remove", k);
            editor.Call<bool>("commit");
        }
    }

    private static void Flush() { }
#else
    private static string Get(string k)
    {
        string v = PlayerPrefs.GetString(k, null);
        return string.IsNullOrEmpty(v) ? null : v;
    }
    private static void Set(string k, string v) => PlayerPrefs.SetString(k, v ?? "");
    private static void Remove(string k) => PlayerPrefs.DeleteKey(k);
    private static void Flush() => PlayerPrefs.Save();
#endif
}
