using UnityEditor;
using UnityEngine;


/// <summary>
/// 유닛만 생성가능 Enemy 구현 안함
/// </summary>
public partial class PrefabCreatorWindow : EditorWindow
{
    private string prefabName = "NewPrefab";
    private Sprite sprite;
    private RuntimeAnimatorController animatorController;
    private string sortingLayerName = "Units";
    private GameObject bulletPrefab;
    private ScriptableObject unitDatabase;
    private string tag = "Ally";
    private int layer = 3;

    [MenuItem("Tools/Prefab Creator Window")]
    public static void ShowWindow()
    {
        GetWindow<PrefabCreatorWindow>("Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Creator", EditorStyles.boldLabel);

        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), false);
        animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(RuntimeAnimatorController), false);
        sortingLayerName = EditorGUILayout.TextField("Sorting Layer", sortingLayerName);
        bulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", bulletPrefab, typeof(GameObject), false);
        unitDatabase = (ScriptableObject)EditorGUILayout.ObjectField("Unit Database", unitDatabase, typeof(ScriptableObject), false);
        tag = EditorGUILayout.TagField("Tag", tag);
        layer = EditorGUILayout.LayerField("Layer", layer);

        if (GUILayout.Button("Create Prefab"))
        {
            CreateUnitPrefab();
        }
    }

    private void CreateUnitPrefab()
    {
        GameObject newPrefab = new GameObject(prefabName);

        // Set the Tag and Layer
        newPrefab.tag = tag;
        newPrefab.layer = layer;

        // Add SpriteRenderer and assign the selected sprite and sorting layer
        SpriteRenderer spriteRenderer = newPrefab.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = sortingLayerName;

        // Add Animator and assign the selected animator controller
        Animator animator = newPrefab.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Add Rigidbody2D
        Rigidbody2D rb = newPrefab.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 1;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // Add BoxCollider2D
        BoxCollider2D collider = newPrefab.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0.0518552f, 0.0131112f);
        collider.size = new Vector2(0.8970137f, 1.166349f);

        // Add custom scripts
        Unit unit = newPrefab.AddComponent<Unit>();
        Draggable draggable = newPrefab.AddComponent<Draggable>();
        UnitMerger unitMerger = newPrefab.AddComponent<UnitMerger>();

        // Set the SpriteRenderer reference in Draggable script
        draggable.SetSpriteRenderer(spriteRenderer);

        // Set the Bullet Prefab in Unit script
        unit.bulletPrefab = bulletPrefab;

        // Set the Unit Database in UnitMerger script
        unitMerger.unitDatabase = (UnitDatabase) unitDatabase;

        // Create the prefab
        string localPath = "Assets/Prefabs/" + prefabName + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        PrefabUtility.SaveAsPrefabAsset(newPrefab, localPath);

        DestroyImmediate(newPrefab);

        Debug.Log("Prefab created at " + localPath);
    }


    private void CreateEnemyPrefab(UnitData unitData)
    {

        GameObject newPrefab = new GameObject(prefabName);

        // Set the Tag and Layer
        newPrefab.tag = "Untagged";
        newPrefab.layer = layer;
        newPrefab.transform.localScale = new Vector2(3,3);

        // Add SpriteRenderer and assign the selected sprite and sorting layer
        SpriteRenderer spriteRenderer = newPrefab.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.flipX = true;

        // Add Animator and assign the selected animator controller
        Animator animator = newPrefab.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Add Rigidbody2D
        Rigidbody2D rb = newPrefab.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 1;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // Add custom scripts
        Enemy enemy = newPrefab.AddComponent<Enemy>();
        // Set the SpriteRenderer reference in Draggable script
        enemy.damageTextPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/DamageText.prefab", typeof(GameObject));
        enemy.unitData = unitData;
        // Create the prefab
        string localPath = "Assets/Prefabs/" + prefabName + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        PrefabUtility.SaveAsPrefabAsset(newPrefab, localPath);

        DestroyImmediate(newPrefab);

        Debug.Log("Prefab created at " + localPath);
    }
}
