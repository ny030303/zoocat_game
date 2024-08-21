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

        // �θ� ������Ʈ ��ǥ
        parentTransform = this.gameObject.transform.parent;
        Vector2 unitpos = new Vector2(this.transform.position.x, this.transform.position.y);
        // ���� ��ǥ�迡���� ��ġ�� ���� ��ǥ��� ��ȯ
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

        //�α׿� ����
        unitID1 = otherUnitset.unitData.id;
        unitID2 = unitset.unitData.id;
        Vector2 ve1 = cpyLocalPos(otherUnit);

        // ����� ����� Unit �Ӽ� Kill
        otherUnitset.Kill();

        Debug.Log("localPos"+ localPos);
        //// ���� ���� �ڸ��� �� ���� ����
        GameObject unit = unitSpawnManager.SpawnNextAlly(localPos);
        // �� ���� ���׷��̵�
        Unit newunitset = unit.GetComponent<Unit>();
        resultUnitID = newunitset.unitData.id;
        newunitset.UpgradeUnitMerged(unitset.unitData);
        // ���� ���� �����
        unitSpawnManager.KillUnit(this.gameObject);

        Debug.Log(unitID1 + unitID2 + resultUnitID + localPos);
        // �̺�Ʈ Ʈ����
        eventManager.OnUnitMerged(unitID1, ve1, unitID2, localPos, resultUnitID, localPos);
    }
}
