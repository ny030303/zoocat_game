using LitJson;
using UnityEngine;

/// <summary>
/// 서버 범용 <c>error</c> / <c>deckUpdateError</c> 를 받아 사용자에게 노출한다.
/// 명세 §6 오류 문자열 카탈로그 매핑.
/// </summary>
public static class GlobalErrorHandler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SocketDispatcher.Instance.On(SocketEvents.Error, OnError);
        SocketDispatcher.Instance.On(SocketEvents.DeckUpdateError, OnDeckUpdateError);
        SocketDispatcher.Instance.On(SocketEvents.LoginError, OnLoginError);
    }

    private static string AsString(JsonData data)
        => data != null ? data.ToString() : string.Empty;

    private static void OnError(JsonData data)
    {
        string msg = AsString(data);
        Debug.LogWarning("[Socket] error: " + msg);

        switch (msg)
        {
            case "로그인이 필요합니다":
                if (SocketBinder.Instance != null) SocketBinder.Instance.RequestReLogin();
                break;
            case "요청이 너무 많습니다":
                ToastMessage.Show("잠시 후 다시 시도해주세요.");
                break;
            default:
                if (!string.IsNullOrEmpty(msg)) ToastMessage.Show(msg);
                break;
        }
    }

    private static void OnDeckUpdateError(JsonData data)
    {
        string msg = AsString(data);
        Debug.LogWarning("[Socket] deckUpdateError: " + msg);
        ToastMessage.Show(string.IsNullOrEmpty(msg) ? "덱 저장에 실패했습니다." : msg);
        // 로컬 덱 롤백은 ChangeUnitPanel 이 lastServerDeck 으로 처리
    }

    private static void OnLoginError(JsonData data)
    {
        string msg = AsString(data);
        Debug.LogError("[Socket] loginError: " + msg);
        // 로그인 패널 전용 UI 는 LoginManager 가 별도 구독하여 처리
    }
}
