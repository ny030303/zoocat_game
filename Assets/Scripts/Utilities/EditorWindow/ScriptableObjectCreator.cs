using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectCreator : EditorWindow
{
    public static void CreateMyAsset(UnitData asset)
    {
        // ScriptableObject 인스턴스 생성

        // 원하는 경로 설정
        string path = "Assets/Scripts/Data/Unit_UnitData"; // 원하는 경로로 변경 가능
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 경로와 파일명 지정
        string assetPathAndName = Path.Combine(path, asset.id + ".asset");

        // 동일한 파일명이 이미 존재하는지 확인
        if (File.Exists(assetPathAndName))
        {
            Debug.LogWarning("같은 이름의 ScriptableObject가 이미 존재합니다: " + assetPathAndName);
            return; // 동일한 이름의 파일이 있으면 생성하지 않음
        }

        // ScriptableObject 생성
        AssetDatabase.CreateAsset(asset, assetPathAndName);

        // AssetDatabase 업데이트
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
}
