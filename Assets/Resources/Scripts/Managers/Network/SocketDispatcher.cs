using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

/// <summary>
/// 서버 수신 메시지의 단일 라우터.
///  - <see cref="Enqueue"/> 는 WebSocket BG 스레드에서 호출됨 (파싱 안 함, 큐잉만).
///  - <see cref="Update"/> 는 메인 스레드에서 큐를 비우며 {event, data} 파싱 후 event별 핸들러 호출.
/// 씬에 오브젝트를 둘 필요 없음 — 최초 접근 시 자동 생성 + DontDestroyOnLoad.
/// </summary>
public class SocketDispatcher : MonoBehaviour
{
    private static SocketDispatcher _instance;
    public static bool HasInstance => _instance != null;

    public static SocketDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[SocketDispatcher]");
                _instance = go.AddComponent<SocketDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly ConcurrentQueue<string> _inbox = new ConcurrentQueue<string>();
    private readonly Dictionary<string, List<Action<JsonData>>> _handlers
        = new Dictionary<string, List<Action<JsonData>>>();

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// BG 스레드에서 호출 가능. ConcurrentQueue 라 lock-free.
    public void Enqueue(string raw) => _inbox.Enqueue(raw);

    public void On(string ev, Action<JsonData> handler)
    {
        if (handler == null) return;
        if (!_handlers.TryGetValue(ev, out var list))
            _handlers[ev] = list = new List<Action<JsonData>>();
        if (!list.Contains(handler)) list.Add(handler);
    }

    public void Off(string ev, Action<JsonData> handler)
    {
        if (handler != null && _handlers.TryGetValue(ev, out var list))
            list.Remove(handler);
    }

    private void Update()
    {
        while (_inbox.TryDequeue(out var raw))
        {
            JsonData root;
            try { root = JsonMapper.ToObject(raw); }
            catch (Exception ex)
            {
                Debug.LogError("[Socket] parse fail: " + ex.Message + "\n" + raw);
                continue;
            }

            if (root == null || !root.IsObject || !root.Has("event"))
            {
                Debug.LogWarning("[Socket] malformed message: " + raw);
                continue;
            }

            string ev = root["event"].ToString();
            JsonData data = root.Has("data") ? root["data"] : null;

            if (_handlers.TryGetValue(ev, out var handlers) && handlers.Count > 0)
            {
                foreach (var h in handlers.ToArray())
                {
                    try { h(data); }
                    catch (Exception ex) { Debug.LogError($"[Socket] handler '{ev}' threw: {ex}"); }
                }
            }
            else
            {
                Debug.LogWarning("[Socket] unhandled event: " + ev);
            }
        }
    }
}
