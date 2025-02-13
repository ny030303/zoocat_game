using System.Collections;
using System.Collections.Generic;
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

                // 유닛 UI 생성
                GameObject gm = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

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
       

        // 변경된 데이터 반영하여 UI 갱신
        UpdateUnitImages(lobbyUserManager.unitDatabase.unitDeck);
    }
}
