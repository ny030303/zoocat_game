using LitJson;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// auth-session 로그인 화면.
///  - 자격증명 있으면 (CredentialStore) SocketBinder 가 연결 직후 resumeSession/login 을 자동으로 보낸다.
///    → LoginManager 는 "접속 중" 표시 후 registered/loginSuccess 를 기다린다.
///  - 없으면 로그인 패널을 띄우고, 유저가 "게스트로 계속" / "구글 로그인" 을 고르면 register 를 보낸다.
///  - 유닛 로스터·프로필은 서버가 준다 (registered / loginSuccess / userJoined). 로컬 생성 없음.
/// </summary>
public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader;

    private GameObject LoginPanel;
    private GameObject GuestformPanel;
    private GameObject LobbyEntryPanel;

    void Start()
    {
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");
        if (GuestformPanel != null) GuestformPanel.SetActive(false);

        var d = SocketDispatcher.Instance;
        d.On(SocketEvents.Registered, OnAuthed);
        d.On(SocketEvents.LoginSuccess, OnAuthed);
        d.On(SocketEvents.LoginError, OnAuthFailedUI);
        d.On(SocketEvents.RegisterError, OnAuthFailedUI);
        // sessionExpired 는 SocketBinder 가 login 폴백으로 처리 (여기선 UI 무변경)

        if (SocketBinder.Instance != null)
        {
            SocketBinder.Instance.OnKicked += OnKicked;
            SocketBinder.Instance.OnAuthFailed += ShowLoginPanel;
        }

        if (CredentialStore.HasCredentials)
        {
            // SocketBinder 가 자동 resume/login → 응답 기다림
            ShowConnecting();
        }
        else
        {
            ShowLoginPanel("");
        }

        if (AppConfig.Current.gpgsEnabled)
            GPGSBinder.Inst.Init((ok, localUser) => Debug.Log("[Login] GPGS init: " + ok));
    }

    void OnDestroy()
    {
        if (SocketDispatcher.HasInstance)
        {
            var d = SocketDispatcher.Instance;
            d.Off(SocketEvents.Registered, OnAuthed);
            d.Off(SocketEvents.LoginSuccess, OnAuthed);
            d.Off(SocketEvents.LoginError, OnAuthFailedUI);
            d.Off(SocketEvents.RegisterError, OnAuthFailedUI);
        }
        if (SocketBinder.Instance != null)
        {
            SocketBinder.Instance.OnKicked -= OnKicked;
            SocketBinder.Instance.OnAuthFailed -= ShowLoginPanel;
        }
    }

    // ---------------------------------------------------------------- 버튼

    /// "게스트로 계속"
    public void ContinueAsGuest()
    {
        if (GuestformPanel != null) GuestformPanel.SetActive(false);

        if (CredentialStore.HasCredentials) { ShowConnecting(); return; } // 이미 resume 중

        string name = "게스트" + UnityEngine.Random.Range(1000, 10000);
        SocketBinder.Instance.SendRegister(name, true);
        ShowConnecting();
    }

    /// 닉네임 입력 폼 확인
    public void GuestSignup()
    {
        var input = GuestformPanel != null ? GuestformPanel.GetComponentInChildren<TMPro.TMP_InputField>() : null;
        string name = input != null ? input.text.Trim() : "";
        if (string.IsNullOrWhiteSpace(name)) { Debug.LogWarning("[Login] 닉네임 비어 있음"); return; }
        if (name.Length > 32) name = name.Substring(0, 32);

        if (input != null) input.text = "";
        if (GuestformPanel != null) GuestformPanel.SetActive(false);

        if (CredentialStore.HasCredentials) { ShowConnecting(); return; }
        SocketBinder.Instance.SendRegister(name, true);
        ShowConnecting();
    }

    /// "구글 로그인" — GPGS 인증해서 표시명만 가져오고 register/resume 는 게스트와 동일.
    /// 서버 provider 연동은 Phase 2. googleId 는 PendingGpgsLink 로 보관.
    public void GooglePlayLogin()
    {
        if (!AppConfig.Current.gpgsEnabled)
        {
            ToastMessage.Show("이 빌드에서는 게스트 로그인만 가능합니다.");
            return;
        }
        GPGSBinder.Inst.Login((success, localUser) =>
        {
            if (!success) { Debug.LogWarning("[Login] Google 로그인 실패"); ShowLoginPanel(""); return; }

            CredentialStore.PendingGpgsLink = localUser.id; // Phase 2 연동 준비
            if (CredentialStore.HasCredentials) { ShowConnecting(); return; }

            string name = !string.IsNullOrEmpty(localUser.userName) ? localUser.userName : "Player";
            if (name.Length > 32) name = name.Substring(0, 32);
            SocketBinder.Instance.SendRegister(name, false);
            ShowConnecting();
        });
    }

    public void ShowGuestLoginPanel()
    {
        if (GuestformPanel != null) GuestformPanel.SetActive(!GuestformPanel.activeSelf);
    }

    /// 계정 전환/로그아웃
    public void Logout()
    {
        SocketBinder.Instance.LogoutAndClear();
        CredentialStore.Clear();
        try { GPGSBinder.Inst.Logout(); } catch { }
        ShowLoginPanel("");
    }

    // ---------------------------------------------------------------- 서버 응답

    private void OnAuthed(JsonData data)
    {
        if (data != null && data.Has("userProfile") && data["userProfile"] != null)
            UserManager.Instance.LoadUserFromJson(data["userProfile"]);

        UserManager.Instance.isGuest = 1; // 서버 세션 있음
        UserManager.Instance.isAnonymous = string.IsNullOrEmpty(CredentialStore.PendingGpgsLink);

        Debug.Log("[Login] authed (userId=" + CredentialStore.UserId + ")");
        if (LoginPanel != null) LoginPanel.SetActive(false);
        if (GuestformPanel != null) GuestformPanel.SetActive(false);
        if (LobbyEntryPanel != null) LobbyEntryPanel.SetActive(true);
    }

    private void OnAuthFailedUI(JsonData data)
    {
        string msg = data != null ? data.ToString() : "인증에 실패했습니다.";
        Debug.LogError("[Login] auth error: " + msg);
        ShowLoginPanel(msg);
    }

    private void OnKicked(string msg)
    {
        ToastMessage.Show(msg);
        ShowLoginPanel(msg);
    }

    // ---------------------------------------------------------------- UI 상태

    private void ShowConnecting()
    {
        if (LoginPanel != null) LoginPanel.SetActive(false);
        if (GuestformPanel != null) GuestformPanel.SetActive(false);
        if (LobbyEntryPanel != null) LobbyEntryPanel.SetActive(false);
        // TODO: "접속 중..." 스피너 패널
    }

    private void ShowLoginPanel(string _)
    {
        if (LoginPanel != null) LoginPanel.SetActive(true);
        if (GuestformPanel != null) GuestformPanel.SetActive(false);
        if (LobbyEntryPanel != null) LobbyEntryPanel.SetActive(false);
    }

    public void OnLobbyEnterButtonClicked()
    {
        sceneLoader.LoadScene("LobbyTestScene");
    }
}
