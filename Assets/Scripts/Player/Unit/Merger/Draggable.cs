using System;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Vector3 LoadedPos;
    float startPosx;
    float startPosY;
    public bool isBeingHeld = false;
    public bool isInLine;
    float timelinePosY;

    private void Start()
    {
        LoadedPos = this.transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetSpriteRenderer(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    private void Update()
    {
        if (isBeingHeld)
        {
            Vector2 mousePos;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            this.gameObject.transform.position = new Vector2(mousePos.x - startPosx, mousePos.y - startPosY);
        }
    }

    private bool CheckIsSameUnit(GameObject otherObj)
    {
        Unit unitset = this.gameObject.GetComponent<Unit>();
        Unit otherUnitset = otherObj.GetComponent<Unit>();
        if (otherObj.CompareTag("Ally") && // ���� �����϶�
            unitset.unitData.id == otherUnitset.unitData.id && // ���� ���̵�
            unitset.unitData.grade == otherUnitset.unitData.grade) // ���� ���
        {
            return true;
        }
        return false;
    }

    private void OnMouseDown()
    {
        Unit unitset = this.gameObject.GetComponent<Unit>();
        if (Input.GetMouseButtonDown(0) && unitset.owner == "player")
        {
            //// ���� �������� �˻��Ͽ� �� ����
            GameObject locationObj = this.gameObject.transform.parent.gameObject;
            int childCount = locationObj.transform.childCount;

            // �ڽ� ������Ʈ���� GameObject �迭�� ��������
            GameObject[] childObjects = new GameObject[childCount];
            for (int i = 0; i < childCount; i++) {  childObjects[i] = locationObj.transform.GetChild(i).gameObject; }
            foreach (GameObject obj in childObjects)
            {
                if (!CheckIsSameUnit(obj))
                {
                    SpriteRenderer objSp = obj.GetComponent<SpriteRenderer>();
                    objSp.color = new Color(0.5f, 0.5f, 0.5f, 1f); // ��Ӱ�

                }
            }

            spriteRenderer.color = new Color(1f, 1f, 1f, .5f);
            Vector3 mousePos;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            startPosx = mousePos.x - this.transform.position.x;
            startPosY = mousePos.y - this.transform.position.y;
            isBeingHeld = true;

            
        }
    }

    private void OnMouseUp()
    {
        Unit unitset = this.gameObject.GetComponent<Unit>();
        if (unitset.owner == "player")
        {
            GameObject locationObj = this.gameObject.transform.parent.gameObject;
            int childCount = locationObj.transform.childCount;

            // �ڽ� ������Ʈ���� GameObject �迭�� ��������
            GameObject[] childObjects = new GameObject[childCount];
            for (int i = 0; i < childCount; i++) { childObjects[i] = locationObj.transform.GetChild(i).gameObject; }
            foreach (GameObject obj in childObjects)
            {
                SpriteRenderer objSp = obj.GetComponent<SpriteRenderer>();
                objSp.color = new Color(1f, 1f, 1f, 1f); // ����
            }

            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            isBeingHeld = false;

            CheckTriggerOnDrop();
        }
    }

    // �巡�� ���� ������ Ʈ���� �̺�Ʈ�� �������� ó���ϴ� �޼���
    private void CheckTriggerOnDrop()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 0.5f)
         .Where(collider => collider.gameObject.transform.position != this.gameObject.transform.position)
         .ToArray(); // �ʿ信 ���� �ݰ��� ����

        foreach (var collider in colliders)
        {
            if (collider.gameObject != this.gameObject)
            {
               
                if (CheckIsSameUnit(collider.gameObject)) 
                { 
                    UnitMerger merger = collider.gameObject.GetComponent<UnitMerger>();
                    if (merger != null)
                    {
                        merger.MergeUnits(this.gameObject);
                    }
                    return;
                }
               
            }
        }
        

        this.gameObject.transform.position = LoadedPos;

    }
}