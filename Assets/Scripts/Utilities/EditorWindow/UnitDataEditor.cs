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
        GUILayout.Label("JSON���� ���� ������ ��������", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON ����", jsonFile, typeof(TextAsset), false);

        if (GUILayout.Button("���� ������ ��������"))
        {
            ImportUnitData();
        }
    }

    private void ImportUnitData()
    {
        if (jsonFile == null)
        {
            Debug.LogWarning("JSON ������ ���õ��� �ʾҽ��ϴ�.");
            return;
        }

        // JSON �Ľ�
        var unitDataList = JsonConvert.DeserializeObject<List<UnitData>>(jsonFile.text);
        // �� ���� �����Ϳ� ���� ó��
        foreach (var unitData in unitDataList)
        {
            if (unitData == null)
            {
                Debug.LogWarning("JSON �����͸� �Ľ��ϴ� �� �����߽��ϴ�.");
                return;
            }

            // ��������Ʈ ��Ʈ ����
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
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, idleSprites, attackSprites); // �ִϸ��̼� ����

            } else if (unitData.tag == "ENEMY")
            {
                var walkSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.walkSprite);
                var dieSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.dieSprite);
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, walkSprites, dieSprites);  // �ִϸ��̼� ����
            }

            // ������ ����
            prefabWindow.CreatePrefabFromUnitData(unitData, aniController);
        }
        
    }
}
public partial class SpriteSheetAutoSlicer : EditorWindow
{
    public Object CreateSpriteSheetFromUnitData(string url)
    { // unitData�� ����Ͽ� ��������Ʈ ��Ʈ�� �����մϴ�.
        SliceSpriteSheet(url); //�ڸ���
        Object sprites = AssetDatabase.LoadAssetAtPath(url, typeof(Object));
        return sprites;
    }
}

// ���� AnimationSetupWindow Ȯ���� ���� UnitData�� ó��
public partial class AnimationSetupWindow : EditorWindow
{
    public AnimatorController GenerateAnimationsFromUnitData(UnitData unitData, Object spr1, Object spr2)
    {
        // unitData�� spriteSheets �� animationNames�� ��ȯ�Ͽ�
        // ���� GenerateAnimations() �޼��带 ȣ���մϴ�.
        // ������ unitData.RenderSetting.UnitSprite ��ο� ��������Ʈ�� �ִٰ� �����մϴ�.
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

// ���� PrefabCreatorWindow Ȯ���� ���� UnitData�� ó��
public partial class PrefabCreatorWindow : EditorWindow
{
    public void CreatePrefabFromUnitData(UnitData unitData, AnimatorController aniController)
    {
        // unitData�� ����Ͽ� �������� ��������Ʈ, �ִϸ����� ���� �����մϴ�.
        // ����:
        this.prefabName = unitData.id;

        // �ʿ��� �߰� ����
        //CreatePrefab();
    }
}
