using LitJson;
using UnityEngine;
using WebSocketSharp;

/// <summary>
/// 서버로 <c>{ "event": ev, "data": data }</c> 봉투를 보내는 헬퍼.
/// 인증이 필요한 이벤트는 <see cref="SocketBinder.SendWhenAuthed"/> 를 쓴다.
/// </summary>
public static class SocketSender
{
    public static bool Send(string ev, object data = null)
    {
        var binder = SocketBinder.Instance;
        var ws = binder != null ? binder.GetWs() : null;

        if (ws == null || ws.ReadyState != WebSocketState.Open)
        {
            Debug.LogWarning($"[Socket] send skipped ({ev}) - socket not open");
            return false;
        }

        string json = JsonMapper.ToJson(new { @event = ev, data });
        ws.Send(json);
        return true;
    }
}
