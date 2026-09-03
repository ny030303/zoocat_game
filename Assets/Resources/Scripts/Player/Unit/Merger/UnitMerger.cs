using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class UnitMerger : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    private UnitSpawnManager unitSpawnManager;
    private Transform parentTransform;
    public GameEventManager eventManager;

    Unit unitset;
    Vector2 localPos;
    public void Start()
    {
        eventManager = FindAnyObjectByType<GameEventManager>();
        //unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        unitset = this.gameObject.GetComponent<Unit>();
        if (unitset.owner == "player") {
            GameObject manager = GameObject.Find("UnitSpawnManager");
            unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        }
        else if (unitset.owner == "ai")  {
            GameObject manager = GameObject.Find("AIUnitSpawnManager");
            unitSpawnManager = manager.GetComponent<UnitSpawnManager>();
        }

        // �θ� ������Ʈ ��ǥ
        parentTransform = this.gameObject.transform.parent;
        Vector2 unitpos = new Vector2(this.transform.position.x, this.transform.position.y);
        // ���� ��ǥ�迡���� ��ġ�� ���� ��ǥ��� ��ȯ
        localPos = parentTransform.InverseTransformPoint(unitpos);
    }

    public void MergeUnits(GameObject otherUnit)
    {
        string unitID1;
        string unitID2;
        string resultUnitID;

        Unit otherUnitset = otherUnit.GetComponent<Unit>();

        //�α׿� ����
        unitID1 = otherUnitset.unitData.id;
        unitID2 = unitset.unitData.id;
        Vector2 ve1 = otherUnitset.spawnPos;

        // 결과 유닛을 먼저 스폰 시도 - 실패하면 아무것도 죽이지 않음(원본 유지)
        GameObject unit = unitSpawnManager.SpawnNextAlly(localPos);
        if (unit == null)
        {
            Debug.LogWarning("[UnitMerger] 병합 스폰 실패 - 원본 유닛 유지 (localPos=" + localPos + ")");
            return;
        }
        Unit newunitset = unit.GetComponent<Unit>();
        if (newunitset == null) { Destroy(unit); return; }

        // 스폰 성공 후에 원본 둘 제거
        otherUnitset.Kill();
        resultUnitID = newunitset.unitData.id;
        newunitset.UpgradeUnitMerged(unitset.unitData);
        unitSpawnManager.KillUnit(this.gameObject);

        Debug.Log(unitID1 + unitID2 + resultUnitID + localPos);
        eventManager?.OnUnitMerged(unitID1, ve1, unitID2, localPos, resultUnitID, localPos);
    }

}
