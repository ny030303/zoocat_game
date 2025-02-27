using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitDetails : MonoBehaviour
{
    public GameObject detailsPanel; // 상세보기 패널
    public GameObject ChangeUnitPanel; // 덱 변경 패널
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Slider slider;
    public TextMeshProUGUI atkItemText;
    public TextMeshProUGUI hitItemText;
    public TextMeshProUGUI criItemText;
    public TextMeshProUGUI attackSpdItemText;
    public TextMeshProUGUI splashRangeItemText;

    public Button upgradeBtn;
    public Button useBtn;
    public TextMeshProUGUI upgradeCostBtnText;

    public Transform unitDisplayContainer; // 유닛 애니메이션을 표시할 부모 Transform
    private GameObject currentUnitInstance; // 현재 표시 중인 유닛 인스턴스
    private UnitData currentUnitData;
    private UserUnit currentUserUnit;
    private UnitUpgradeData currentUnitUpgradeData;

    public UnitUpgradeManager unitUpgradeManager;
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
            unitUpgradeManager.OnUnitUpgraded += ShowDetails;
            Debug.Log("UnitDetails panel initialized and hidden.");
        }
        else
        {
            Debug.LogWarning("Details panel is not assigned in the inspector!");
        }
    }
    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        unitUpgradeManager.OnUnitUpgraded -= ShowDetails;
    }
    public void ShowDetails(UnitData unit, UserUnit userUnit, UnitUpgradeData unitUpgradeData)
    {
        // 업그레이드를 위해 데이터 객체 내부에서 공유 
        currentUnitData = unit;
        currentUserUnit = userUnit;
        currentUnitUpgradeData = unitUpgradeData;

        updateDetails();

        // 패널 활성화
        detailsPanel.SetActive(true);
        ChangeUnitPanel.GetComponent<ChangeUnitPanel>().OnShowPanel(unit, userUnit); // 덱 변경 창 미리 적용
    }

    private void updateDetails()
    {
        UnitData unit = currentUnitData;
        UserUnit userUnit = currentUserUnit;
        UnitUpgradeData unitUpgradeData = currentUnitUpgradeData;
        // 텍스트 정보 업데이트
        nameText.text = "Name: " + unit.name;
        levelText.text = $"lv. {userUnit.lv}";

        atkItemText.text = $"+{unit.atk}";
        hitItemText.text = $"+{unit.hit}%";
        criItemText.text = $"+{unit.cri}%";
        attackSpdItemText.text = $"+{unit.attackSpeed}";
        splashRangeItemText.text = $"+{unit.splashRange}";

        //string unitIdWithoutPrefix = unit.id.Replace("CHA_", ""); // "CHA_" 제거
        //슬라이더 값 넣기
        Transform handleTransform = slider.transform.Find("Fill Area/Fill");// Handle 색상 변경을 위한 Fill 이미지 찾기
        Image handleImage = handleTransform != null ? handleTransform.GetComponent<Image>() : null;
        if (userUnit.lv == 10)
        {
            slider.maxValue = 1;
            slider.value = 1;
            handleImage.color = Color.yellow;
            Transform valChild = slider.transform.Find("ValueText");
            TMP_Text valText = valChild.GetComponent<TMP_Text>();
            valText.text = $"MAX";
        }
        else if (unitUpgradeData != null) {
            slider.maxValue = unitUpgradeData.cost;
            slider.value = userUnit.piece;
            Transform valChild = slider.transform.Find("ValueText");
            TMP_Text valText = valChild.GetComponent<TMP_Text>();
            valText.text = $"{userUnit.piece} / {unitUpgradeData.cost}";
            upgradeCostBtnText.text = $"{unitUpgradeData.cost}";
        }
        else {
            slider.maxValue = 0;
            slider.value = 1;
            Transform valChild = slider.transform.Find("ValueText");
            TMP_Text valText = valChild.GetComponent<TMP_Text>();
            upgradeCostBtnText.text = "-";
            valText.text = $"- / -";
        }

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


        if (userUnit.unlock == 0)
        {
            upgradeBtn.interactable = false;
            useBtn.interactable = false;
            useBtn.transform.GetChild(0).GetComponent<Text>().text = "미보유";
        }
        else
        {
            upgradeBtn.interactable = true;
            useBtn.interactable = true;
            useBtn.transform.GetChild(0).GetComponent<Text>().text = "사용";
        }
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
    public void UpgradeUnit()
    {
        string unitIdWithoutPrefix = currentUnitData.id.Replace("CHA_", ""); // "CHA_" 제거

        if (currentUserUnit.lv == 10)
        {
            ToastMessage.Show("더 이상 강화 할 수 없습니다!", 1.5f);

        }
        else if(unitUpgradeManager.CanUpgradeUnit(unitIdWithoutPrefix))
        {
            unitUpgradeManager.UpgradeUnit(unitIdWithoutPrefix);

            ToastMessage.Show("강화에 성공했습니다!", 1.5f);
        } 
        else
        {
            ToastMessage.Show("강화 조각이 부족합니다.", 1.5f);
        }
    }

    public void ChangeUnitDeck()
    {
        HideDetails();
    }
}
