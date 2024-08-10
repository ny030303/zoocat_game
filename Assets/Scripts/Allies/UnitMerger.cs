using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class UnitMerger : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    private UnitSpawner unitSpawner;
    private Transform parentTransform;
    Unit unitset;
    Vector2 localPos;
    public void Start()
    {

        unitSpawner = FindAnyObjectByType<UnitSpawner>();
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
        GameObject unit = unitSpawner.SpawnNextAlly(localPos);
        

        //unit.transform.SetParent(parentTransform, false);
        //unit.transform.position = this.transform.position; // ����ġ ��������
        //unit.transform.localScale *= 1.2f; // Adjust the scale factor as needed
        unit.transform.localScale *= 1.2f; // Adjust the scale factor as needed



        // ���� ���� �����
        unitSpawner.KillUnit(this.gameObject);

    }
}
