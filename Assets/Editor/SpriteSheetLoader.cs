using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SpriteSheetLoader : MonoBehaviour
{
    public string filePath = "Assets/Sprites/mySprite.png"; // 스프라이트 시트 경로
    public GameObject targetObject; // 스프라이트를 적용할 오브젝트

    void Start()
    {
        //LoadAndApplySprite();
    }

    void LoadAndApplySprite()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
            return;
        }

        Sprite[] sprites = LoadSpritesFromSheet(filePath);

        if (sprites.Length > 0)
        {
            // 첫 번째 스프라이트를 오브젝트에 적용
            targetObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
        }
        else
        {
            Debug.LogError("No sprites were found in the specified sheet.");
        }
    }

    public static Sprite[] LoadSpritesFromSheet(string path)
    {
        string assetPath = path.Replace(Application.dataPath, "Assets");
        Debug.Log(assetPath);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath) as Sprite[];

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites found at the specified path.");
        }

        return sprites;
    }

    
}
