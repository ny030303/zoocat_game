using UnityEngine;

/// <summary>
/// 상대의 릴레이 이벤트를 AI 쪽 보드에 재현한다(라이브 미러).
/// LogReplayManager 의 파일 재생 로직을 셀 인덱스 기반 + 실시간으로 옮긴 것.
/// PvpBattleController 가 소유하고 Apply() 를 호출한다.
/// </summary>
public class PvpOpponentView
{
    private readonly UnitSpawnManager _aiSpawn;   // owner == "ai"
    private readonly GameManager _gameManager;
    private readonly UnitDatabase _unitDatabase;

    public int OpponentWave { get; private set; } = -1;

    public PvpOpponentView(UnitSpawnManager aiSpawn, GameManager gameManager, UnitDatabase unitDatabase)
    {
        _aiSpawn = aiSpawn;
        _gameManager = gameManager;
        _unitDatabase = unitDatabase;
        if (_aiSpawn != null) _aiSpawn.Initialize();
    }

    public void Apply(PvpEvent e)
    {
        if (e == null || string.IsNullOrEmpty(e.k)) return;
        switch (e.k)
        {
            case PvpProtocol.K_Spawn:   ApplySpawn(e); break;
            case PvpProtocol.K_Merge:   ApplyMerge(e); break;
            case PvpProtocol.K_Upgrade: ApplyUpgrade(e); break;
            case PvpProtocol.K_Life:    if (_gameManager != null) _gameManager.SetOpponentLife(e.v); break;
            case PvpProtocol.K_Wave:    OpponentWave = e.v; break;
        }
    }

    private void ApplySpawn(PvpEvent e)
    {
        if (_aiSpawn == null || !_aiSpawn.IsSpawnNext()) return;
        _aiSpawn.SpawnAt(e.c, e.u);
    }

    private void ApplyMerge(PvpEvent e)
    {
        if (_aiSpawn == null) return;
        Transform parent = _aiSpawn.GetParentTransform();
        if (parent == null) return;

        Unit atTarget = FindUnit(parent, e.u, e.c);   // 타겟 셀의 유닛(unitID2)
        Unit dragged = e.n >= 0 ? FindUnit(parent, e.a, e.n) : null; // 드래그된 유닛(unitID1)

        UnitData beforeData = atTarget != null ? atTarget.unitData
                            : _unitDatabase != null ? _unitDatabase.GetUnitData("ai", e.u)
                            : null;

        if (dragged != null) dragged.Kill();
        if (atTarget != null) atTarget.Kill();

        GameObject res = _aiSpawn.SpawnAt(e.c, e.r);
        if (res != null && beforeData != null)
        {
            Unit ru = res.GetComponent<Unit>();
            if (ru != null) ru.UpgradeUnitMerged(beforeData);
        }
    }

    private void ApplyUpgrade(PvpEvent e)
    {
        if (_unitDatabase == null || e.n < 0) return;
        if (e.n >= _unitDatabase.GetUnitListCount("ai")) return;
        UnitData data = _unitDatabase.GetUnitDataToIdx("ai", e.n);
        if (data == null || data.level >= data.maxUpgradeLevel) return;

        data.LevelUp();

        // 이미 스폰된 동일 종류 AI 유닛도 레벨업
        Transform parent = _aiSpawn != null ? _aiSpawn.GetParentTransform() : null;
        if (parent != null)
        {
            foreach (Unit u in parent.GetComponentsInChildren<Unit>())
                if (u.unitData != null && u.unitData.id == data.id)
                    u.unitData.LevelUp();
        }
    }

    private Unit FindUnit(Transform parent, string unitId, int cell)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Unit u = parent.GetChild(i).GetComponent<Unit>();
            if (u == null || u.unitData == null) continue;
            if (u.unitData.id != unitId) continue;
            if (cell < 0 || _aiSpawn.PositionToCell(u.spawnPos) == cell) return u;
        }
        return null;
    }
}
