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
            Sprite preSprite = null;
            if (unitData.tag == "UNIT")
            {
                Debug.Log(unitData.idleSprite);
                Debug.Log(unitData.attackSprite);
                var idleSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.idleSprite);
                var attackSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.attackSprite);
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, idleSprites, attackSprites); // �ִϸ��̼� ����
                
                preSprite = (Sprite) AssetDatabase.LoadAssetAtPath(unitData.idleSprite, typeof(Sprite));

            } else if (unitData.tag == "ENEMY")
            {
                var walkSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.walkSprite);
                var dieSprites = spriteSheetWindow.CreateSpriteSheetFromUnitData(unitData.dieSprite);
                aniController = animationWindow.GenerateAnimationsFromUnitData(unitData, walkSprites, dieSprites);  // �ִϸ��̼� ����
                preSprite = (Sprite)AssetDatabase.LoadAssetAtPath(unitData.walkSprite, typeof(Sprite));
            }

            // ������ ����
            prefabWindow.CreatePrefabFromUnitData(unitData, aniController, preSprite);
        }
        
    }
}
public partial class SpriteSheetAutoSlicer : EditorWindow
{
    public Object CreateSpriteSheetFromUnitData(string url)
    { // unitData�� ����Ͽ� ��������Ʈ ��Ʈ�� �����մϴ�.
        this.SliceSpriteSheet(url); //�ڸ���
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
        this.spriteSheets.Add(spr1);
        this.spriteSheets.Add(spr2);
        this.animatorControllerName = unitData.id;
        if (unitData.tag == "UNIT")
        {
            this.animationNames.Add("IdleAnimation");
            this.animationNames.Add("AttackAnimation");
        }
        else if (unitData.tag == "ENEMY")
        {
            this.animationNames.Add("WalkAnimation");
            this.animationNames.Add("DieAnimation");
            this.isEnemy = true;
        }
        return GenerateAnimations();
    }
}

// ���� PrefabCreatorWindow Ȯ���� ���� UnitData�� ó��
public partial class PrefabCreatorWindow : EditorWindow
{
    public void CreatePrefabFromUnitData(UnitData unitData, AnimatorController aniController, Sprite preSprite)
    {
        // unitData�� ����Ͽ� �������� ��������Ʈ, �ִϸ����� ���� �����մϴ�.
        // ����:
        this.prefabName = unitData.id;
        this.animatorController = aniController;
        this.sprite = preSprite;
        if (unitData.tag == "UNIT")
        {
            this.unitDatabase = (ScriptableObject)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Data/UnitDatabase.asset", typeof(ScriptableObject));
            this.bulletPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/UI_shot.prefab", typeof(GameObject));
            CreateUnitPrefab();
        }
        else if (unitData.tag == "ENEMY") {
            CreateEnemyPrefab(unitData); 
        }
            
        // �ʿ��� �߰� ����
        unitData.unitPrefab = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/"+ unitData.id + ".prefab", typeof(GameObject));
        CreateMyAsset(unitData);
    }

    public void CreateMyAsset(UnitData asset)
    {
        // ScriptableObject �ν��Ͻ� ����

        // ���ϴ� ��� ����
        string path = "";
        if (asset.tag == "UNIT") path = "Assets/Scripts/Data/Unit_UnitData";
        else if (asset.tag == "ENEMY") path = "Assets/Scripts/Data/Enemy_UnitData"; 

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // ��ο� ���ϸ� ����
        string assetPathAndName = Path.Combine(path, asset.id + ".asset");

        // ������ ���ϸ��� �̹� �����ϴ��� Ȯ��
        if (File.Exists(assetPathAndName))
        {
            Debug.LogWarning("���� �̸��� ScriptableObject�� �̹� �����մϴ�: " + assetPathAndName);
            return; // ������ �̸��� ������ ������ �������� ����
        }

        // ScriptableObject ����
        AssetDatabase.CreateAsset(asset, assetPathAndName);

        // AssetDatabase ������Ʈ
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
