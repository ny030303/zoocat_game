using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

/// <summary>
/// Phase C 대전 릴레이. 로컬 이벤트를 0.5s 배치로 matchMessage 전송,
/// 상대 matchMessage 를 파싱해 콜백으로 넘긴다.
/// PvpBattleController 가 소유하고 Tick / Start / Stop 을 호출한다(싱글턴 아님).
/// </summary>
public class PvpRelay
{
    private readonly string _matchId;
    private readonly float _flushInterval;
    private float _sinceFlush;
    private readonly List<PvpEvent> _out = new List<PvpEvent>();

    private PvpEnd _pendingEnd;      // 전송 실패 시 재시도용
    private float _endRetryLeft;

    private bool _started;

    public Action<PvpHello> OnOpponentHello;
    public Action<PvpEvent> OnOpponentEvent;
    public Action<PvpEnd> OnOpponentEnd;

    public PvpRelay(string matchId, float flushInterval = 0.5f)
    {
        _matchId = matchId;
        _flushInterval = flushInterval;
    }

    public void Start()
    {
        if (_started) return;
        _started = true;
        SocketDispatcher.Instance.On(SocketEvents.MatchMessage, HandleInbound);
    }

    public void Stop()
    {
        if (!_started) return;
        _started = false;
        if (SocketDispatcher.HasInstance)
            SocketDispatcher.Instance.Off(SocketEvents.MatchMessage, HandleInbound);
        Flush(); // 남은 배치 마지막으로 밀어냄
    }

    // ---- 아웃바운드 ----
    // 서버는 matchMessage 를 { matchId, from, payload } 로 상대에게 중계한다.
    // 실제 메시지는 반드시 payload 안에 넣어야 상대에게 전달된다.

    private bool SendMsg(object payload)
        => SocketSender.Send(SocketEvents.MatchMessage, new { matchId = _matchId, payload });

    public void SendHello(int seed, string waveDriver)
        => SendMsg(new PvpHello { matchId = _matchId, seed = seed, wave = waveDriver });

    public void QueueOut(PvpEvent e)
    {
        if (e == null) return;
        _out.Add(e);
        // life / wave 는 지연 없이 바로 보낸다(승패 판정 신뢰성).
        if (e.k == PvpProtocol.K_Life || e.k == PvpProtocol.K_Wave)
            Flush();
    }

    public void SendEnd(PvpEnd end)
    {
        end.matchId = _matchId;
        if (!SendMsg(end))
        {
            _pendingEnd = end;
            _endRetryLeft = 5f;
        }
    }

    /// controller.Update 에서 Time.unscaledDeltaTime 로 호출(게임오버 시 timeScale=0 대비).
    public void Tick(float unscaledDt)
    {
        _sinceFlush += unscaledDt;
        if (_sinceFlush >= _flushInterval)
        {
            _sinceFlush = 0f;
            Flush();
        }

        if (_pendingEnd != null)
        {
            _endRetryLeft -= unscaledDt;
            if (SendMsg(_pendingEnd))
                _pendingEnd = null;
            else if (_endRetryLeft <= 0f)
                _pendingEnd = null; // 포기 - opponentLeft 로 상대가 처리
        }
    }

    private void Flush()
    {
        if (_out.Count == 0) return;
        var batch = new PvpEventBatch { matchId = _matchId, batch = new List<PvpEvent>(_out) };
        if (SendMsg(batch))
            _out.Clear();
        // 전송 실패(소켓 닫힘 등)면 버퍼 유지 → 다음 flush 에서 재시도
    }

    // ---- 인바운드 ----
    // 서버 중계 형태: { matchId, from, payload:{ t, ... } }

    private int _inboundLogged;

    private void HandleInbound(JsonData data)
    {
        if (data == null || !data.IsObject) return;

        JsonData msg =
            (data.Has("payload") && data["payload"] != null && data["payload"].IsObject) ? data["payload"]
            : (data.Has("message") && data["message"] != null && data["message"].IsObject) ? data["message"]
            : data; // 서버가 그대로 중계하는 경우 대비

        string t = msg.GetStr("t");

        if (_inboundLogged < 4)
        {
            _inboundLogged++;
            Debug.Log($"[PvpRelay] inbound #{_inboundLogged}: t={t ?? "<none>"} env={data.ToJson()}");
        }

        if (string.IsNullOrEmpty(t)) return;

        try
        {
            string raw = msg.ToJson();
            switch (t)
            {
                case PvpProtocol.T_Hello:
                    OnOpponentHello?.Invoke(JsonMapper.ToObject<PvpHello>(raw));
                    break;

                case PvpProtocol.T_Events:
                    var b = JsonMapper.ToObject<PvpEventBatch>(raw);
                    if (b?.batch != null)
                        foreach (var e in b.batch)
                            OnOpponentEvent?.Invoke(e);
                    break;

                case PvpProtocol.T_End:
                    OnOpponentEnd?.Invoke(JsonMapper.ToObject<PvpEnd>(raw));
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[PvpRelay] inbound parse fail (t=" + t + "): " + ex);
        }
    }
}
