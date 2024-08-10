using UnityEngine;
using UnityEngine.EventSystems;

public class AllyUnit : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public GameObject[] upgradedAllyPrefabs;  // ���׷��̵� ������ �����յ�
    private Vector2 originalPosition;
    private Canvas canvas;
    private RectTransform rectTransform;

    void Start()
    {
        // �������� ������ ����
        originalPosition = transform.position;

        // �θ� Canvas�� ã�Ƽ� ���� (�ʿ�� ���)
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;  // �巡�� ���� �� ��ġ ����
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ���콺 ��ġ�� �������� ������ �̵�
        Vector2 newPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out newPosition);

        transform.position = canvas.transform.TransformPoint(newPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Ally"))
            {
                AllyUnit otherUnit = collider.GetComponent<AllyUnit>();
                if (otherUnit != null && otherUnit.GetType() == this.GetType())
                {
                    // ���� Ÿ���� �����̸� ���׷��̵�
                    UpgradeUnit();
                    Destroy(collider.gameObject);  // �ٸ� ���� ����
                    break;
                }
            }
        }

        // ���� ��ġ�� �ǵ����� �ʰ� ���׷��̵� �� ������ �ش� ��ġ�� ������ ��
    }

    void UpgradeUnit()
    {
        if (upgradedAllyPrefabs.Length > 0)
        {
            // �����ϰ� ���׷��̵�� ������ ����
            int randomIndex = Random.Range(0, upgradedAllyPrefabs.Length);
            GameObject upgradedPrefab = upgradedAllyPrefabs[randomIndex];

            // ���ο� ���׷��̵�� ���� ����
            GameObject newUnit = Instantiate(upgradedPrefab, transform.position, Quaternion.identity);
            newUnit.transform.SetParent(transform.parent, false);

            Destroy(gameObject);  // ���� ���� ����
        }
    }
}
