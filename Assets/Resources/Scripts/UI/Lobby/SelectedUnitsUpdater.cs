using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitsUpdater : MonoBehaviour
{
    public LobbyUserManager lobbyUserManager;
    public GameObject unitContainer; // 유닛 정보를 업데이트할 UI 컨테이너
    public GameObject unitTemplate;  // 유닛 UI 템플릿 (Prefab)
    public Sprite defaultSprite;    // 기본 이미지 (null일 때 대체)

    private void Start()
    {

        // UnitData 목록 가져오기
        List<UnitData> allUnits = lobbyUserManager.unitDatabase.unitDeck;
        // 유닛 덱 변경 이벤트 구독
        lobbyUserManager.OnUnitDeckChanged += UpdateUnitImages;
        UserManager.Instance.OnUserUnitListChanged += updateCharacterList;
        UpdateUnitImages(allUnits);
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

    // 정보를 업데이트하는 함수
    public void UpdateUnitImages(List<UnitData> selectedUnits)
    {
        // 유닛 컨테이너 초기화 (기존 UI 클리어)
        foreach (Transform child in unitContainer.transform) { Destroy(child.gameObject); }


        // 선택된 유닛 정보 업데이트
        foreach (UnitData unit in selectedUnits)
        {
            // "CHA_XXXX" 형식의 ID를 가진 유닛 찾기
            //string formattedId = $"CHA_{unitId:D4}";
            //UnitData unit = allUnits.Find(u => u.id == formattedId);

            if (unit != null)
            {
                Debug.Log($"유닛 찾음: {unit.unitName}");
                string unitIdWithoutPrefix = unit.id.Replace("CHA_", ""); // "CHA_" 제거

                UserUnit[] userUnitList = UserManager.Instance.units;
                UserUnit matchedUserUnit = userUnitList.FirstOrDefault(u => u.id == unitIdWithoutPrefix); // 유저의 유닛 레벨, 경험치 등
                // 유닛 UI 생성
                GameObject unitObject = UnitTempleteCreater.CreateUnitTemplete(unit, unitTemplate, unitContainer, defaultSprite);

                // 레벨 표시
                Transform levelchild = unitObject.transform.Find("Level");
                if (levelchild != null)
                {
                    TMP_Text levelText = levelchild.GetComponent<TMP_Text>();
                    levelText.text = $"Lv. {matchedUserUnit.lv}";
                }
            }
            else
            {
                Debug.LogWarning($"유닛을(를) 찾을 수 없습니다.");
            }
        }

    }

    //// 이미지 경로를 기반으로 Sprite 로드
    //private Sprite LoadSprite(string profilePath)
    //{
    //    if (string.IsNullOrEmpty(profilePath))
    //    {
    //        Debug.LogWarning("LoadSprite: Profile 경로가 비어 있습니다.");
    //        return null;
    //    }

    //    // Resources 폴더 기준으로 경로에서 확장자 제거
    //    string resourcePath = profilePath.Replace("Assets/Resources/", "").Replace(".png", "");

    //    Sprite sprite = Resources.Load<Sprite>(resourcePath);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"LoadSprite: {resourcePath}에서 Sprite를 찾을 수 없습니다.");
    //    }
    //    return sprite;
    //}
}
