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

        // �θ� ������Ʈ ��ǥ
        parentTransform = this.gameObject.transform.parent;
        Vector2 unitpos = new Vector2(this.transform.position.x, this.transform.position.y);
        // ���� ��ǥ�迡���� ��ġ�� ���� ��ǥ��� ��ȯ
        localPos = parentTransform.InverseTransformPoint(unitpos);
    }
    public void MergeUnits(GameObject otherUnit)
    {
        // ����� ����� Unit �Ӽ� Kill
        Unit otherUnitset = otherUnit.GetComponent<Unit>();
        otherUnitset.Kill();

        Debug.Log("localPos"+ localPos);
        //// ���� ���� �ڸ��� �� ���� ����
        GameObject unit = unitSpawnManager.SpawnNextAlly(localPos);
        // �� ���� ���׷��̵�
        Unit newunitset = unit.GetComponent<Unit>();
        newunitset.UpgradeUnitMerged(unitset.unitData);
        // ���� ���� �����
        unitSpawnManager.KillUnit(this.gameObject);
    }
}
