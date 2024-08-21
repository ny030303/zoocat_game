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
        unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        unitset = this.gameObject.GetComponent<Unit>();

        // 부모 오브젝트 좌표
        parentTransform = this.gameObject.transform.parent;
        Vector2 unitpos = new Vector2(this.transform.position.x, this.transform.position.y);
        // 월드 좌표계에서의 위치를 로컬 좌표계로 변환
        localPos = parentTransform.InverseTransformPoint(unitpos);
    }
    Vector2 cpyLocalPos(GameObject obj) {
        Vector2 pos = new Vector2(obj.transform.position.x, obj.transform.position.y);
        pos = parentTransform.InverseTransformPoint(pos);
        return pos; 
    }

    public void MergeUnits(GameObject otherUnit)
    {
        string unitID1;
        string unitID2;
        string resultUnitID;

        Unit otherUnitset = otherUnit.GetComponent<Unit>();

        //로그용 저장
        unitID1 = otherUnitset.unitData.id;
        unitID2 = unitset.unitData.id;
        Vector2 ve1 = cpyLocalPos(otherUnit);

        // 끌어온 대상은 Unit 속성 Kill
        otherUnitset.Kill();

        Debug.Log("localPos"+ localPos);
        //// 기존 유닛 자리에 새 유닛 생성
        GameObject unit = unitSpawnManager.SpawnNextAlly(localPos);
        // 새 유닛 업그레이드
        Unit newunitset = unit.GetComponent<Unit>();
        resultUnitID = newunitset.unitData.id;
        newunitset.UpgradeUnitMerged(unitset.unitData);
        // 기존 유닛 지우기
        unitSpawnManager.KillUnit(this.gameObject);

        Debug.Log(unitID1 + unitID2 + resultUnitID + localPos);
        // 이벤트 트리거
        eventManager.OnUnitMerged(unitID1, ve1, unitID2, localPos, resultUnitID, localPos);
    }
}
