using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class UnitMerger : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    private UnitSpawnManager unitSpawnManager;
    private Transform parentTransform;
    Unit unitset;
    Vector2 localPos;
    public void Start()
    {

        unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        unitset = this.gameObject.GetComponent<Unit>();

        // 부모 오브젝트 좌표
        parentTransform = this.gameObject.transform.parent;
        Vector2 unitpos = new Vector2(this.transform.position.x, this.transform.position.y);
        // 월드 좌표계에서의 위치를 로컬 좌표계로 변환
        localPos = parentTransform.InverseTransformPoint(unitpos);
    }
    public void MergeUnits(GameObject otherUnit)
    {
        // 끌어온 대상은 Unit 속성 Kill
        Unit otherUnitset = otherUnit.GetComponent<Unit>();
        otherUnitset.Kill();

        Debug.Log("localPos"+ localPos);
        //// 기존 유닛 자리에 새 유닛 생성
        GameObject unit = unitSpawnManager.SpawnNextAlly(localPos);
        // 새 유닛 업그레이드
        Unit newunitset = unit.GetComponent<Unit>();
        newunitset.UpgradeUnitMerged(unitset.unitData);
        // 기존 유닛 지우기
        unitSpawnManager.KillUnit(this.gameObject);
    }
}
