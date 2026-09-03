using LitJson;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LoginManager : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader ��ũ��Ʈ�� ����

    private GameObject LoginPanel;
    private GameObject GuestformPanel;
    private GameObject LobbyEntryPanel;

    private const string UUID_KEY = "GuestUUID";

    void Start()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
        // Ŭ���� ������ Guestform ������ �ʱ�ȭ
        LoginPanel = GameObject.Find("Login Panel");
        GuestformPanel = GameObject.Find("Guest Form Panel");
        LobbyEntryPanel = GameObject.Find("Lobby Entry Panel");

        // ���������� ã�������� Ȯ���ϴ� ���� �����ϴ�.
        if (GuestformPanel != null && LobbyEntryPanel != null && LobbyEntryPanel != null) { GuestformPanel.SetActive(false); }
        else { Debug.LogError("Panel�� ã�� �� �����ϴ�. �̸��� Ȯ���ϼ���."); }

        // 서버 로그인 응답 구독
        SocketDispatcher.Instance.On(SocketEvents.LoginSuccess, OnLoginSuccess);
        SocketDispatcher.Instance.On(SocketEvents.LoginError, OnLoginError);

        if (AppConfig.Current.gpgsEnabled)
        {
        GPGSBinder.Inst.Init((isLoggedIn, localUser) => {
            if (isLoggedIn)
            {
                Debug.Log("User is logged in." + localUser);
                SendGoogleLoginEventMessageToServer(localUser);
                UserManager.Instance.isGuest = 1;
                UserManager.Instance.isAnonymous = false; // 구글 연동됨
                // ������ �α��� �� ���� ó��
                LoginPanel.SetActive(false);
                LobbyEntryPanel.SetActive(true);
            }
            else {
                Debug.Log("Googlegames User failed to log in.");
                // �α��� ���� �� ó���� ����
                LoginPanel.SetActive(true);
                LobbyEntryPanel.SetActive(false);
            }
        });
        }
        else
        {
            // dev 빌드: GPGS 초기화 스킵. 선택 화면 표시
            LoginPanel.SetActive(true);
            LobbyEntryPanel.SetActive(false);
        }
        // 자동 게스트 로그인 안 함 — 유저가 LoginPanel 에서 "게스트로 계속" / "구글 로그인" 을 명시적으로 선택
    }
    public void Logout()
    {
        GPGSBinder.Inst.Logout();
        FileManager.DeleteDataFile();
        LoginPanel.SetActive(true);
        LobbyEntryPanel.SetActive(false);
    }
    public void GooglePlayLogin() {
        if (!AppConfig.Current.gpgsEnabled)
        {
            // dev 빌드(.dev 패키지)는 GPGS 미설정 → 구글 로그인 불가
            Debug.LogWarning("[Login] GPGS disabled in this build - use guest login");
            ToastMessage.Show("이 빌드에서는 게스트 로그인만 가능합니다.");
            return;
        }
        GPGSBinder.Inst.Login((success, localUser) => {
            if (success)
            {
                UserManager.Instance.isGuest = 1;          // 서버(구글) 로그인 사용자
                UserManager.Instance.isAnonymous = false;  // 구글 연동됨
                SendGoogleLoginEventMessageToServer(localUser);
                LoginPanel.SetActive(false);
                LobbyEntryPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[Login] Google Play sign-in failed");
                LoginPanel.SetActive(true);
                LobbyEntryPanel.SetActive(false);
            }
        });
    }

    public void SendGoogleLoginEventMessageToServer(ILocalUser localUser)
    {
        string id = localUser.id;
        string userName = localUser.userName;
        string underage = localUser.underage.ToString();

        // 재연결 시 자동 재로그인용으로 캐시
        SocketBinder.Instance.CacheLoginPayload(id, userName, underage);
        SocketSender.Send(SocketEvents.Login, new { id, userName, underage });
    }

    // 서버 login 응답 → 유저 프로필 로드
    private void OnLoginSuccess(JsonData data)
    {
        if (data != null && data.Has("userProfile") && data["userProfile"] != null)
        {
            UserManager.Instance.LoadUserFromJson(data["userProfile"]);
        }

        bool isNewUser = data != null && data.Has("isNewUser") && (bool)data["isNewUser"];
        Debug.Log($"[Login] success (isNewUser={isNewUser})");
        // TODO: isNewUser 면 튜토리얼 분기
    }

    private void OnLoginError(JsonData data)
    {
        string msg = data != null ? data.ToString() : "로그인에 실패했습니다.";
        Debug.LogError("[Login] error: " + msg);
        if (LoginPanel != null) LoginPanel.SetActive(true);
        if (LobbyEntryPanel != null) LobbyEntryPanel.SetActive(false);
        // TODO: 로그인 패널에 에러 텍스트 노출
    }
    //�Խ�Ʈ �α���
    public void GuestLogin()
    {
        // 로컬 캐시 먼저 (즉시 UI 표시용 — 서버 loginSuccess 오면 덮어씀)
        UserManager.Instance.currentUser = FileManager.LoadUserData();
        UserManager.Instance.units = FileManager.LoadUnits();
        UserManager.Instance.isGuest = 1;        // 서버 세션 사용
        UserManager.Instance.isAnonymous = true; // UUID 게스트 (아직 계정 미연동)

        // 서버에 게스트 UUID 로 로그인 (서버 auth 는 id 문자열뿐, 없으면 자동 가입)
        UserData local = UserManager.Instance.currentUser;
        string uuid = local != null ? local.id : null;
        string name = local != null && !string.IsNullOrEmpty(local.username) ? local.username : "Guest";

        if (string.IsNullOrEmpty(uuid))
        {
            Debug.LogError("[Login] guest UUID missing - cannot server-login");
            return;
        }

        SocketBinder.Instance.CacheLoginPayload(uuid, name, "true");
        SocketSender.Send(SocketEvents.Login, new { id = uuid, userName = name, underage = "true" });
    }
    //�Խ�Ʈ ȸ������
    // 닉네임 지정해서 새 게스트 계정 생성 (닉네임 폼용). 폼 없이 자동 생성도 가능.
    private void CreateGuestAccount(string playerName)
    {
        string newUUID = Guid.NewGuid().ToString();
        FileManager.SaveData(UUID_KEY, newUUID);
        FileManager.SaveData("GuestPlayerName", playerName);

        UserUnit[] units =
        {
            new UserUnit { id = "1001", unlock = 1, lv = 1, exp = 0, piece = 30 },
            new UserUnit { id = "1002", unlock = 1, lv = 1, exp = 0, piece = 20 },
            new UserUnit { id = "1003", unlock = 1, lv = 1, exp = 0, piece = 0 },
            new UserUnit { id = "1004", unlock = 1, lv = 1, exp = 0, piece = 0 },
            new UserUnit { id = "1005", unlock = 1, lv = 1, exp = 0, piece = 0 },
            new UserUnit { id = "1006", unlock = 0, lv = 0, exp = 0, piece = 0 },
            new UserUnit { id = "1007", unlock = 1, lv = 1, exp = 0, piece = 0 },
            new UserUnit { id = "1008", unlock = 0, lv = 0, exp = 0, piece = 0 },
            new UserUnit { id = "1009", unlock = 0, lv = 0, exp = 0, piece = 0 }
        };
        UserData user = new UserData
        {
            id = newUUID,
            underage = true,
            username = playerName,
            level = 1,
            experience = 0,
            friends = new string[] { },
            country = "",
            language = "ko",
            selectedUnits = new string[] { "1001", "1002", "1003", "1004", "1005" },
            gold = BalanceConfig.Current.guestStartGold,
            gems = BalanceConfig.Current.guestStartGems
        };
        FileManager.SaveUnits(units);
        FileManager.SaveUserData(user);
        Debug.Log("[Login] guest account created: " + playerName);
    }

    // 닉네임 입력 폼의 확인 버튼
    public void GuestSignup()
    {
        TMP_InputField input = GuestformPanel != null ? GuestformPanel.GetComponentInChildren<TMP_InputField>() : null;
        string playerName = input != null ? input.text : "";
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("[Login] guest name empty");
            return;
        }

        CreateGuestAccount(playerName.Trim());
        GuestLogin();

        if (input != null) input.text = "";
        if (GuestformPanel != null) GuestformPanel.SetActive(false);
        LoginPanel.SetActive(false);
        LobbyEntryPanel.SetActive(true);
    }



    public void ShowGuestLoginPanel()
    {
        if (GuestformPanel != null)
        {  GuestformPanel.SetActive(!GuestformPanel.activeSelf); }
        else {  Debug.LogError("Guestform�� null�Դϴ�. �ʱ�ȭ�� ������ ���� �� �ֽ��ϴ�."); }
    }

    // 저장된 게스트 데이터(UUID)가 있는지
    private bool HasSavedGuest()
    {
        GameData gamedata = FileManager.LoadData();
        return gamedata != null && gamedata.dataDictionary.ContainsKey(UUID_KEY);
    }

    /// "게스트로 계속" 버튼.
    /// 저장된 게스트가 있으면 그대로 이어서, 없으면 닉네임 입력 없이 자동 생성 후 로그인.
    /// (닉네임을 직접 정하고 싶으면 GuestformPanel + GuestSignup 경로를 별도로 두면 됨)
    public void ContinueAsGuest()
    {
        if (!HasSavedGuest())
            CreateGuestAccount("게스트" + UnityEngine.Random.Range(1000, 10000));

        GuestLogin(); // 로컬 로드 + 서버 login(UUID)
        LoginPanel.SetActive(false);
        if (GuestformPanel != null) GuestformPanel.SetActive(false);
        LobbyEntryPanel.SetActive(true);
    }

    public void OnLobbyEnterButtonClicked()
    {
        // �κ� ������ ��ȯ
        sceneLoader.LoadScene("LobbyTestScene");
    }
}
