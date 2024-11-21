using System.Collections.Generic;
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
        // 유저 데이터 로드
        //UserData userdata = UserManager.Instance.currentUser;
        //Debug.Log("Selected Units[0]: " + userdata.selectedUnits[0]);

        // 유닛 정보를 업데이트
        int[] selectedUnits = { 1001, 1002, 1003, 1004, 1005 };
        UpdateUnitImages(selectedUnits);
    }

    // 정보를 업데이트하는 함수
    public void UpdateUnitImages(int[] selectedUnits)
    {
        // 유닛 컨테이너 초기화 (기존 UI 클리어)
        foreach (Transform child in unitContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // UnitData 목록 가져오기
        List<UnitData> allUnits = lobbyUserManager.unitDatabase.unitDeck;

        // 선택된 유닛 정보 업데이트
        foreach (int unitId in selectedUnits)
        {
            // "CHA_XXXX" 형식의 ID를 가진 유닛 찾기
            string formattedId = $"CHA_{unitId:D4}";
            UnitData unit = allUnits.Find(u => u.id == formattedId);

            if (unit != null)
            {
                Debug.Log($"유닛 찾음: {unit.unitName}");

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
            }
            else
            {
                Debug.LogWarning($"유닛 ID {formattedId}을(를) 찾을 수 없습니다.");
            }
        }

    }

    // 이미지 경로를 기반으로 Sprite 로드
    private Sprite LoadSprite(string profilePath)
    {
        if (string.IsNullOrEmpty(profilePath))
        {
            Debug.LogWarning("LoadSprite: Profile 경로가 비어 있습니다.");
            return null;
        }

        // Resources 폴더 기준으로 경로에서 확장자 제거
        string resourcePath = profilePath.Replace("Assets/Resources/", "");

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogError($"LoadSprite: {resourcePath}에서 Sprite를 찾을 수 없습니다.");
        }
        return sprite;
    }
}
