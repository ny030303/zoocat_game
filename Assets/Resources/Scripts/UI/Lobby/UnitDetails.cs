using UnityEngine;
using TMPro;

public class UnitDetails : MonoBehaviour
{
    public GameObject detailsPanel; // 상세보기 패널
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI descriptionText;

    public Transform unitDisplayContainer; // 유닛 애니메이션을 표시할 부모 Transform
    private GameObject currentUnitInstance; // 현재 표시 중인 유닛 인스턴스
    public static UnitDetails Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDetailsPanel();
        }
        else
        {
            Debug.LogWarning("Multiple UnitDetails instances found!");
            Destroy(gameObject);
        }
    }

    private void InitializeDetailsPanel()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false); // 초기화 시 패널 비활성화
            Debug.Log("UnitDetails panel initialized and hidden.");
        }
        else
        {
            Debug.LogWarning("Details panel is not assigned in the inspector!");
        }
    }

    public void ShowDetails(UnitData unit)
    {
        // 텍스트 정보 업데이트
        nameText.text = "Name: " + unit.name;
        //levelText.text = "Level: " + unit.level.ToString();

        // 기존 유닛 인스턴스 제거
        if (currentUnitInstance != null)
        {
            Destroy(currentUnitInstance);
        }

        // 새 GameObject 생성
        currentUnitInstance = new GameObject(unit.name); // 오브젝트 이름 설정
        currentUnitInstance.transform.SetParent(unitDisplayContainer, false); // 부모 설정
        currentUnitInstance.transform.localPosition = Vector3.zero;
        currentUnitInstance.transform.localRotation = Quaternion.identity;
        currentUnitInstance.transform.localScale = Vector3.one;
        // Tag 및 Layer 설정
        currentUnitInstance.tag = "Ally"; // 설정할 태그
        currentUnitInstance.layer = LayerMask.NameToLayer("unit"); // 설정할 레이어
        // 위치를 설정할 때 Z 좌표를 0으로 설정
        currentUnitInstance.transform.localPosition = new Vector3(
        currentUnitInstance.transform.localPosition.x,
        currentUnitInstance.transform.localPosition.y,
        0f);


        // SpriteRenderer 추가 및 설정
        SpriteRenderer spriteRenderer = currentUnitInstance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>(unit.idleSprite); // UnitData에 연결된 스프라이트
        spriteRenderer.sortingLayerName = "Units"; // 정렬 레이어
        spriteRenderer.sortingOrder = 0; // 정렬 순서

        // Animator 추가 및 설정
        Animator animator = currentUnitInstance.AddComponent<Animator>();
        Animator prefanim = unit.unitPrefab.GetComponent<Animator>();
        animator.runtimeAnimatorController = prefanim.runtimeAnimatorController;
        animator.applyRootMotion = false; // 필요에 따라 설정
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // 패널 활성화
        detailsPanel.SetActive(true);
    }

    public void HideDetails()
    {
        detailsPanel.SetActive(false); // 상세보기 패널 비활성화

        // 유닛 인스턴스 제거
        if (currentUnitInstance != null)
        {
            Destroy(currentUnitInstance);
        }
    }
}
