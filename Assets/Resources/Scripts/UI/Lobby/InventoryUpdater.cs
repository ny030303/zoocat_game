using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUpdater : MonoBehaviour
{
    public GameObject unitTemplate; // 캐릭터 Prefab
    public GameObject unitContainer; // 캐릭터 컨테이너(부모)
    public Sprite defaultSprite; // 기본프로필
    public List<UnitData> unitList;

    void Start()
    {
        unitList = UnitListLoader.Instance.unitList;
        GenerateCharacterList();
    }
    void GenerateCharacterList()
    {
        // 기존 컨테이너 하위의 모든 자식 삭제
        foreach (Transform child in unitContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (UnitData unit in unitList)
        {
            // 유닛 UI 생성
            GameObject unitObject = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

            // 버튼에 클릭 이벤트 추가
            Button button = unitObject.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log($"Button found for unit: {unit.name}");
                button.onClick.AddListener(() => OnUnitClicked(unit));
            }
            else
            {
                Debug.LogWarning($"Button component missing on unit template for {unit.name}");
            }
        }
    }

    void OnUnitClicked(UnitData unit)
    {
        if (UnitDetails.Instance != null)
        {
            UnitDetails.Instance.ShowDetails(unit);
        }
        else
        {
            Debug.LogWarning("UnitDetails instance is not initialized.");
        }
    }

}
