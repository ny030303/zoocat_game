using UnityEngine;
using UnityEngine.EventSystems;

public class AllyUnit : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public GameObject[] upgradedAllyPrefabs;  // 업그레이드 가능한 프리팹들
    private Vector2 originalPosition;
    private Canvas canvas;
    private RectTransform rectTransform;

    void Start()
    {
        // 오리지널 포지션 저장
        originalPosition = transform.position;

        // 부모 Canvas를 찾아서 저장 (필요시 사용)
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;  // 드래그 시작 시 위치 저장
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 기준으로 유닛을 이동
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
                    // 같은 타입의 유닛이면 업그레이드
                    UpgradeUnit();
                    Destroy(collider.gameObject);  // 다른 유닛 제거
                    break;
                }
            }
        }

        // 원래 위치로 되돌리지 않고 업그레이드 된 유닛이 해당 위치에 남도록 함
    }

    void UpgradeUnit()
    {
        if (upgradedAllyPrefabs.Length > 0)
        {
            // 랜덤하게 업그레이드된 프리팹 선택
            int randomIndex = Random.Range(0, upgradedAllyPrefabs.Length);
            GameObject upgradedPrefab = upgradedAllyPrefabs[randomIndex];

            // 새로운 업그레이드된 유닛 생성
            GameObject newUnit = Instantiate(upgradedPrefab, transform.position, Quaternion.identity);
            newUnit.transform.SetParent(transform.parent, false);

            Destroy(gameObject);  // 기존 유닛 제거
        }
    }
}
