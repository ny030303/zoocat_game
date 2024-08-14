using System;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public Vector3 LoadedPos;
    float startPosx;
    float startPosY;
    public bool isBeingHeld = false;
    public bool isInLine;
    float timelinePosY;

    private void Start()
    {
        LoadedPos = this.transform.position;
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
        if (Input.GetMouseButtonDown(0))
        {
            //// ���� �������� �˻��Ͽ� �� ����
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Ally");
            foreach (GameObject obj in objectsWithTag)
            {

                //Debug.Log(obj.name);
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
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject obj in objectsWithTag)
        {
            SpriteRenderer objSp = obj.GetComponent<SpriteRenderer>();
            objSp.color = new Color(1f, 1f, 1f, 1f); // ����
        }

            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        isBeingHeld = false;

        CheckTriggerOnDrop();
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