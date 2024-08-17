using UnityEngine;
using UnityEditor;
using System.IO;

public partial class SpriteSheetAutoSlicer : EditorWindow
{
    private string folderPath = "Assets/Sprites/Chactor2"; // 기본 경로 설정
    private string filePath = "Assets/Sprites/Chactor2/렉돌 공격 1.png"; // 기본 경로 설정
    private int columns = 2;
    private int rows = 1;
    private Vector2 pivot = new Vector2(0.5f, 0.5f);
    private int pixelsPerUnit = 100;

    [MenuItem("Tools/Sprite Sheet Auto Slicer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSheetAutoSlicer>("Sprite Sheet Auto Slicer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Sheet Auto Slicer", EditorStyles.boldLabel);

        filePath = EditorGUILayout.TextField("File Path", filePath);
        //folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        columns = EditorGUILayout.IntField("Columns", columns);
        rows = EditorGUILayout.IntField("Rows", rows);
        pivot = EditorGUILayout.Vector2Field("Pivot", pivot);
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);

        if (GUILayout.Button("Slice Sprite Sheets"))
        {
            SliceSpriteSheets();
        }

        if (GUILayout.Button("Slice Sprite Sheet"))
        {
            SliceSpriteSheet(filePath);
        }
    }

    private void SliceSpriteSheets()
    {
        string[] files = Directory.GetFiles(folderPath, "*.png");

        foreach (string file in files)
        {
            string assetPath = file.Replace(Application.dataPath, "Assets");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = pixelsPerUnit;

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                int textureWidth = texture.width;
                int textureHeight = texture.height;

                int spriteWidth = textureWidth / columns;
                int spriteHeight = textureHeight / rows;

                SpriteMetaData[] metas = new SpriteMetaData[columns * rows];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        SpriteMetaData meta = new SpriteMetaData();
                        meta.rect = new Rect(j * spriteWidth, (rows - i - 1) * spriteHeight, spriteWidth, spriteHeight);
                        meta.name = $"{Path.GetFileNameWithoutExtension(assetPath)}_{i}_{j}";
                        meta.pivot = pivot;
                        metas[i * columns + j] = meta;
                    }
                }

                importer.spritesheet = metas;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        Debug.Log("Sprite Sheets sliced successfully.");
    }

    public void SliceSpriteSheet(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("The specified file does not exist: " + path);
            return;
        }

        string assetPath = path.Replace(Application.dataPath, "Assets");
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            int textureWidth = texture.width;
            int textureHeight = texture.height;

            int spriteWidth = textureWidth / columns;
            int spriteHeight = textureHeight / rows;

            SpriteMetaData[] metas = new SpriteMetaData[columns * rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    SpriteMetaData meta = new SpriteMetaData();
                    meta.rect = new Rect(j * spriteWidth, (rows - i - 1) * spriteHeight, spriteWidth, spriteHeight);
                    meta.name = $"{Path.GetFileNameWithoutExtension(assetPath)}_{i}_{j}";
                    meta.pivot = pivot;
                    metas[i * columns + j] = meta;
                }
            }

            importer.spritesheet = metas;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log("Sprite Sheet sliced successfully: " + path);
        }
        else
        {
            Debug.LogError("Failed to load the TextureImporter for the specified file.");
        }
    }
}
