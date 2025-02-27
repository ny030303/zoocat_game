using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeUnitButton : SelectedUnitsUpdater
{
    private LobbyUserManager lobbyUserManager;
    public ChangeUnitPanel changeUnitPanel;

    private void Start()
    {
        // LobbyUserManager 참조 가져오기
        lobbyUserManager = FindObjectOfType<LobbyUserManager>();
        if (lobbyUserManager == null)
        {
            Debug.LogError("LobbyUserManager를 찾을 수 없습니다!");
            return;
        }

        // UnitData 목록 가져와 UI 업데이트
        List<UnitData> allUnits = lobbyUserManager.unitDatabase.unitDeck;
        UpdateUnitImages(allUnits);
        lobbyUserManager.OnUnitDeckChanged += UpdateUnitImages;
        UserManager.Instance.OnUserUnitListChanged += updateCharacterList;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        lobbyUserManager.OnUnitDeckChanged -= UpdateUnitImages;
        UserManager.Instance.OnUserUnitListChanged -= updateCharacterList;
    }

    private void updateCharacterList(UserUnit[] obj)
    {
        // UnitData 목록 가져오기
        List<UnitData> allUnits = lobbyUserManager.unitDatabase.unitDeck;
        UpdateUnitImages(allUnits);
    }

    public void UpdateUnitImages(List<UnitData> selectedUnits)
    {
        // 기존 UI 제거
        foreach (Transform child in unitContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // 새로운 유닛 UI 생성
        foreach (UnitData unit in selectedUnits)
        {
            if (unit != null)
            {
                Debug.Log($"유닛 찾음: {unit.unitName}");
                string unitIdWithoutPrefix = unit.id.Replace("CHA_", ""); // "CHA_" 제거
                UserUnit[] userUnitList = UserManager.Instance.units;
                UserUnit matchedUserUnit = userUnitList.FirstOrDefault(u => u.id == unitIdWithoutPrefix); // 유저의 유닛 레벨, 경험치 등

                // 유닛 UI 생성
                GameObject gm = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

                // 레벨 표시
                Transform levelchild = gm.transform.Find("Level");
                if (levelchild != null)
                {
                    TMP_Text levelText = levelchild.GetComponent<TMP_Text>();
                    levelText.text = $"Lv. {matchedUserUnit.lv}";
                }

                // 버튼 클릭 이벤트 추가
                Button buttonComponent = gm.GetComponent<Button>();
                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(() => OnUnitButtonClick(unit));
            }
            else
            {
                Debug.LogWarning($"유닛을(를) 찾을 수 없습니다.");
            }
        }
    }

    private void OnUnitButtonClick(UnitData unit)
    {
        changeUnitPanel.ChangeUnit(unit);

        changeUnitPanel.GetComponent<RectTransform>().SetAsFirstSibling();
        // 변경된 데이터 반영하여 UI 갱신
        UpdateUnitImages(lobbyUserManager.unitDatabase.unitDeck);
    }
}
