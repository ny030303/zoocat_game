using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectCreator : EditorWindow
{
    public static void CreateMyAsset(UnitData asset)
    {
        // ScriptableObject �ν��Ͻ� ����

        // ���ϴ� ��� ����
        string path = "Assets/Scripts/Data/Unit_UnitData"; // ���ϴ� ��η� ���� ����
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
