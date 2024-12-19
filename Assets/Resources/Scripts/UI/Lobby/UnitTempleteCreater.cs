using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitTempleteCreater : MonoBehaviour
{ // 유닛 프로필 템플릿 크리에이터

    public static GameObject CreateUnitTemplete(UnitData unit, GameObject unitTemplate, GameObject unitContainer, Sprite defaultSprite) {
        // 유닛 UI 생성
        GameObject unitUI = Instantiate(unitTemplate, unitContainer.transform);

        // 프로필 이미지 업데이트
        Image profileImage = unitUI.GetComponentInChildren<Image>();
        if (profileImage != null)
        {
            Sprite loadedSprite = LoadSprite(unit.profile);
            profileImage.sprite = loadedSprite != null ? loadedSprite : defaultSprite;
        }

        // 유닛 이름 업데이트 (TextMeshPro 사용)
        TMPro.TextMeshProUGUI nameText = unitUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = unit.unitName;
        }


        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)unitContainer.GetComponent<RectTransform>());
        return unitUI;
    }

    // 이미지 경로를 기반으로 Sprite 로드
    private static Sprite LoadSprite(string profilePath)
    {
        if (string.IsNullOrEmpty(profilePath))
        {
            Debug.LogWarning("LoadSprite: Profile 경로가 비어 있습니다.");
            return null;
        }

        // Resources 폴더 기준으로 경로에서 확장자 제거
        string resourcePath = profilePath.Replace("Assets/Resources/", "").Replace(".png", "");

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogError($"LoadSprite: {resourcePath}에서 Sprite를 찾을 수 없습니다.");
        }
        return sprite;
    }
}
