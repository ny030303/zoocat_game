using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor.Animations;

public class UnitDataImporter : EditorWindow
{
    private TextAsset jsonFile;

    [MenuItem("Tools/Unit Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<UnitDataImporter>("Unit Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON에서 유닛 데이터 가져오기", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON 파일", jsonFile, typeof(TextAsset), false);

        if (GUILayout.Button("유닛 데이터 가져오기"))
        {
            ImportUnitData();
        }
    }

    private void ImportUnitData()
    {
        if (jsonFile == null)
        {
            Debug.LogWarning("JSON 파일이 선택되지 않았습니다.");
            return;
        }

        // JSON 파싱
        var unitDataList = JsonConvert.DeserializeObject<List<UnitData>>(jsonFile.text);
        // 각 유닛 데이터에 대해 처리
        foreach (var unitData in unitDataList)
        {
            if (unitData == null)
            {
                Debug.LogWarning("JSON 데이터를 파싱하는 데 실패했습니다.");
                return;
            }

            // 스프라이트 시트 생성
            var spriteSheetWindow = ScriptableObject.CreateInstance<SpriteSheetAutoSlicer>();
            var animationWindow = ScriptableObject.CreateInstance<AnimationSetupWindow>();
            var prefabWindow = ScriptableObject.CreateInstance<PrefabCreatorWindow>();

            AnimatorController aniController = null;

            if (unitData.tag == "UNIT")
            {
                Debug.Log(unitData.idleSprite);
                Debug.Log(unitData.attackSprite);
                var idleSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.idleSprite);
                var attackSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.attackSprite);
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, idleSprites, attackSprites); // 애니메이션 생성

            } else if (unitData.tag == "ENEMY")
            {
                var walkSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.walkSprite);
                var dieSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.dieSprite);
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, walkSprites, dieSprites);  // 애니메이션 생성
            }

            // 프리팹 생성
            prefabWindow.CreatePrefabFromUnitData(unitData, aniController);
        }
        
    }
}
public partial class SpriteSheetAutoSlicer : EditorWindow
{
    public Object CreateSpriteSheetFromUnitData(string url)
    { // unitData를 사용하여 스프라이트 시트를 설정합니다.
        SliceSpriteSheet(url); //자르기
        Object sprites = AssetDatabase.LoadAssetAtPath(url, typeof(Object));
        return sprites;
    }
}

// 기존 AnimationSetupWindow 확장을 통해 UnitData를 처리
public partial class AnimationSetupWindow : EditorWindow
{
    public AnimatorController GenerateAnimationsFromUnitData(UnitData unitData, Object spr1, Object spr2)
    {
        // unitData를 spriteSheets 및 animationNames로 변환하여
        // 기존 GenerateAnimations() 메서드를 호출합니다.
        // 간단히 unitData.RenderSetting.UnitSprite 경로에 스프라이트가 있다고 가정합니다.
        spriteSheets.Add(spr1);
        spriteSheets.Add(spr2);
        animatorControllerName = unitData.id;
        if (unitData.tag == "UNIT")
        {
            animationNames.Add("IdleAnimation");
            animationNames.Add("AttackAnimation");
        }
        else if (unitData.tag == "ENEMY")
        {
            animationNames.Add("WalkAnimation");
            animationNames.Add("DieAnimation");
            isEnemy = true;
        }
        return GenerateAnimations();
    }
}

// 기존 PrefabCreatorWindow 확장을 통해 UnitData를 처리
public partial class PrefabCreatorWindow : EditorWindow
{
    public void CreatePrefabFromUnitData(UnitData unitData, AnimatorController aniController)
    {
        // unitData를 사용하여 프리팹의 스프라이트, 애니메이터 등을 설정합니다.
        // 예시:
        this.prefabName = unitData.id;

        // 필요한 추가 설정
        //CreatePrefab();
    }
}
